using HunterPie.Core.Domain.Interfaces;
using HunterPie.Core.Extensions;
using System;

namespace HunterPie.Core.Native.IPC.Handlers
{
    public class MessageDispatcher<T> : IEventDispatcher
    {
        public static event EventHandler<T> OnReceived;

        protected void DispatchMessage(T data) => this.Dispatch(OnReceived, data);
    }
}
