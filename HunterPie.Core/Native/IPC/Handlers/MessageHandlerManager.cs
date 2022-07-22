using HunterPie.Core.Native.IPC.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HunterPie.Core.Native.IPC.Handlers
{
    using MessageHandlerProvider = Dictionary<IPCMessageType, List<IMessageHandler>>;

    public static class MessageHandlerManager
    {

        private static Lazy<MessageHandlerProvider> _handlerProvider = new Lazy<MessageHandlerProvider>(InitProvider);
        private static MessageHandlerProvider handlerProvider => _handlerProvider.Value;

        public static void Dispatch(IPCMessageType type, byte[] message)
        {
            if (!handlerProvider.ContainsKey(type))
                return;

            foreach (var handler in handlerProvider[type])
                handler.Handle(message);
        }

        private static MessageHandlerProvider InitProvider()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.GetInterface(nameof(IMessageHandler)) is not null)
                .Select(@class => (IMessageHandler)Activator.CreateInstance(@class))
                .GroupBy(handler => handler.Type)
                .ToDictionary(group => group.Key, group => group.ToList());
        }
    }
}
