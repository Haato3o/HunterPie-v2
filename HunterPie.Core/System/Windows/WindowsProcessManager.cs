using System;
using System.Diagnostics;
using System.Threading;
using HunterPie.Core.Extensions;
using System.Linq;
using HunterPie.Core.Logger;
using HunterPie.Core.Domain.Interfaces;
using HunterPie.Core.Domain.Memory;
using HunterPie.Core.Domain.Process;
using HunterPie.Core.Address.Map;
using HunterPie.Core.System.Windows.Native;
using HunterPie.Core.System.Windows.Memory;
using HunterPie.Core.Events;
using HunterPie.Core.Domain.Enums;

namespace HunterPie.Core.System.Windows
{
    abstract class WindowsProcessManager : IProcessManager, IEventDispatcher
    {

        protected Thread pooler;
        private bool _isProcessForeground;
        private bool _shouldPauseThread;
        protected bool ShouldPollProcess = true;
       
        private IMemory memory; 
        private IntPtr pHandle;

        public event EventHandler<ProcessEventArgs> OnGameStart;
        public event EventHandler<ProcessEventArgs> OnGameClosed;
        public event EventHandler<ProcessEventArgs> OnGameFocus;
        public event EventHandler<ProcessEventArgs> OnGameUnfocus;

        public virtual string Name { get; }
        public virtual GameProcess Game { get; }

        public int Version { get; private set; }
        public Process? Process { get; private set; }
        public int ProcessId { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsProcessForeground
        {
            get => _isProcessForeground;
            private set
            {
                if (_isProcessForeground != value)
                {
                    _isProcessForeground = value;

                    this.Dispatch(
                        value ? OnGameFocus 
                              : OnGameUnfocus, 
                        new ProcessEventArgs(Name)
                    );
                }
            }
        }

        public IMemory Memory => memory;

        public void Initialize()
        {
            Log.Info($"Started scanning for process {Name}...");

            pooler = new Thread(new ThreadStart(ExecutePolling))
            {
                Name = "PollingBackgroundThread",
                IsBackground = true,
            };
            pooler.Start();
        }

        private void ExecutePolling()
        {
            while (ShouldPollProcess)
            {
                if (_shouldPauseThread)
                {
                    try { Thread.Sleep(Timeout.Infinite); }
                    catch(ThreadInterruptedException) { continue;  }
                }

                PollProcessInfo();
                Thread.Sleep(150);
            }
        }

        private void PollProcessInfo()
        {
            if (Process is not null && Process!.HasExited)
            {
                OnProcessExit();
                return;
            }

            Process mhProcess = Process.GetProcessesByName(Name)
                .FirstOrDefault(process => !string.IsNullOrEmpty(process?.MainWindowTitle));

            if (mhProcess is null)
                return;

            if (Process is not null)
            {
                IsProcessForeground = User32.GetForegroundWindow() == Process.MainWindowHandle;
                mhProcess.Dispose();
                return;
            }

            if (ShouldOpenProcess(mhProcess))
            {
                Process = mhProcess;
                ProcessId = mhProcess.Id;
                pHandle = Kernel32.OpenProcess(Kernel32.PROCESS_ALL_ACCESS, false, ProcessId);

                if (pHandle == IntPtr.Zero)
                {
                    Log.Error("Failed to open game process. Run HunterPie as Administrator!");
                    ShouldPollProcess = false;
                    return;
                }

                IsRunning = true;

                memory = new WindowsMemory(pHandle);

                AddressMap.Add("BASE", (long)Process.MainModule.BaseAddress);

                this.Dispatch(OnGameStart, new(Name));
            }
        }

        protected abstract bool ShouldOpenProcess(Process process);

        private void OnProcessExit()
        {
            Process.Dispose();
            Process = null;

            Kernel32.CloseHandle(pHandle);
            
            pHandle = IntPtr.Zero;

            this.Dispatch(OnGameClosed, new(Name));
        }

        public void Pause() => _shouldPauseThread = true;

        public void Resume()
        {
            _shouldPauseThread = false;
            pooler.Interrupt();
        }
    }
}
