using HunterPie.Core.Domain.Process;
using System;

namespace HunterPie.Core.Game
{
    public abstract class Context : IDisposable
    {
        public IGame Game { get; protected set; }
        public IProcessManager Process { get; protected set; }

        public abstract void Dispose();
    }
}
