﻿using Lime.Messaging.Resources;
using Lime.Protocol.Serialization;
using Lime.Protocol.Serialization.Newtonsoft;
using Serilog;
using SimpleInjector;
using StackExchange.Redis;
using Take.Blip.Builder.Actions;
using Take.Blip.Builder.Actions.CreateTicket;
using Take.Blip.Builder.Actions.DeleteVariable;
using Take.Blip.Builder.Actions.ExecuteScript;
using Take.Blip.Builder.Actions.ForwardMessageToDesk;
using Take.Blip.Builder.Actions.ManageList;
using Take.Blip.Builder.Actions.MergeContact;
using Take.Blip.Builder.Actions.ProcessCommand;
using Take.Blip.Builder.Actions.ProcessContentAssistant;
using Take.Blip.Builder.Actions.ProcessHttp;
using Take.Blip.Builder.Actions.Redirect;
using Take.Blip.Builder.Actions.SendCommand;
using Take.Blip.Builder.Actions.SendMessage;
using Take.Blip.Builder.Actions.SendMessageFromHttp;
using Take.Blip.Builder.Actions.SendRawMessage;
using Take.Blip.Builder.Actions.SetBucket;
using Take.Blip.Builder.Actions.SetVariable;
using Take.Blip.Builder.Actions.TrackEvent;
using Take.Blip.Builder.Actions.TrackContactsJourney;
using Take.Blip.Builder.Diagnostics;
using Take.Blip.Builder.Storage;
using Take.Blip.Builder.Storage.Memory;
using Take.Blip.Builder.Utils;
using Take.Blip.Builder.Variables;
using Take.Blip.Client;
using Take.Blip.Client.Content;
using Take.Blip.Client.Extensions;
using Take.Elephant;

namespace Take.Blip.Builder.Hosting
{
    public static class ContainerExtensions
    {
        public static Container RegisterBuilder(this Container container)
        {
            return container
                .RegisterExternal()
                .RegisterBuilderRoot()
                .RegisterBuilderActions()
                .RegisterBuilderDiagnostics()
                .RegisterBuilderHosting()
                .RegisterBuilderStorage()
                .RegisterBuilderUtils()
                .RegisterBuilderVariables();
        }

        private static Container RegisterBuilderRoot(this Container container)
        {
            container.RegisterSingleton<IFlowManager, FlowManager>();
            container.RegisterSingleton<IStateManager, StateManager>();
            container.RegisterSingleton<IRedirectManager, RedirectManager>();
            container.RegisterSingleton<IContextProvider, ContextProvider>();
            container.RegisterDecorator<ISender, OwnerSenderDecorator>(Lifestyle.Singleton);
            container.RegisterSingleton<IUserOwnerResolver, UserOwnerResolver>();
            container.RegisterSingleton<IInputExpirationHandler, InputExpirationHandler>();

            return container;
        }

        private static Container RegisterBuilderActions(this Container container)
        {
            container.RegisterSingleton<IActionProvider, ActionProvider>();
            container.Collection.Register<IAction>(
                new[]
                {
                    typeof(ExecuteScriptAction),
                    typeof(SendMessageAction),
                    typeof(SendMessageFromHttpAction),
                    typeof(SendRawMessageAction),
                    typeof(SendCommandAction),
                    typeof(ProcessCommandAction),
                    typeof(TrackEventAction),
                    typeof(ProcessHttpAction),
                    typeof(ManageListAction),
                    typeof(MergeContactAction),
                    typeof(SetVariableAction),
                    typeof(SetBucketAction),
                    typeof(RedirectAction),
                    typeof(ForwardMessageToDeskAction),
                    typeof(CreateTicketAction),
                    typeof(DeleteVariableAction),
                    typeof(ProcessContentAssistantAction),
                    typeof(TrackContactsJourneyAction)
                });

            return container;
        }

        private static Container RegisterBuilderDiagnostics(this Container container)
        {
            container.RegisterSingleton<ITraceProcessor, TraceProcessor>();
            container.RegisterSingleton<ITraceManager, TraceManager>();

            return container;
        }

        private static Container RegisterBuilderHosting(this Container container)
        {
            container.RegisterSingleton<IConfiguration, ConventionsConfiguration>();

            return container;
        }

        private static Container RegisterBuilderStorage(this Container container)
        {
            container.RegisterSingleton<INamedSemaphore, MemoryNamedSemaphore>();
            container.RegisterSingleton<ISerializer<StorageDocument>, JsonSerializer<StorageDocument>>();
            container.RegisterSingleton<ISerializer<Contact>, JsonSerializer<Contact>>();
            container.RegisterSingleton<IConnectionMultiplexer>(() =>
            {
                var configuration = container.GetInstance<IConfiguration>();
                return ConnectionMultiplexer.Connect(configuration.RedisStorageConfiguration);
            });

            return container;
        }

        private static Container RegisterBuilderUtils(this Container container)
        {
            container.RegisterSingleton<IVariableReplacer, VariableReplacer>();
            container.RegisterSingleton<IHttpClient, HttpClientWrapper>();

            return container;
        }

        private static Container RegisterBuilderVariables(this Container container)
        {
            container.Collection.Register<IVariableProvider>(
                new[]
                {
                    typeof(ApplicationVariableProvider),
                    typeof(BucketVariableProvider),
                    typeof(CalendarVariableProvider),
                    typeof(ConfigurationVariableProvider),
                    typeof(ContactVariableProvider),
                    typeof(InputVariableProvider),
                    typeof(RandomVariableProvider),
                    typeof(StateVariableProvider),
                    typeof(TunnelVariableProvider),
                    typeof(TicketVariableProvider),
                    typeof(ResourceVariableProvider),
                });

            return container;
        }

        private static Container RegisterExternal(this Container container)
        {
            container.RegisterSingleton<IDocumentTypeResolver>( () => {
                var documentTypeResolver = new DocumentTypeResolver().WithBlipDocuments();
                documentTypeResolver.RegisterDocument<InputExpiration>();
                return documentTypeResolver;
                });
            container.RegisterSingleton<IEnvelopeSerializer, EnvelopeSerializer>();
            container.RegisterSingleton<IDocumentSerializer, DocumentSerializer>();
            container.RegisterSingleton<ILogger>(() => LoggerProvider.Logger);

            return container;
        }
    }
}