using HunterPie.Core.Domain.Enums;
using HunterPie.Core.Domain.Memory;
using HunterPie.Core.Events;
using System;
using SystemProcess = System.Diagnostics.Process;

namespace HunterPie.Core.Domain.Process
{
    public interface IProcessManager
    {
        public SystemProcess Process { get; }
        public GameProcess Game { get; }

        public event EventHandler<ProcessEventArgs> OnGameStart;
        public event EventHandler<ProcessEventArgs> OnGameClosed;
        public event EventHandler<ProcessEventArgs> OnGameFocus;
        public event EventHandler<ProcessEventArgs> OnGameUnfocus;

        public IMemory Memory { get; }

        public void Initialize();
        public void Pause();
        public void Resume();
    }
}
