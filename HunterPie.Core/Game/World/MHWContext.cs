using HunterPie.Core.Domain.Process;
using HunterPie.Core.Game.World.Data;
using System;

namespace HunterPie.Core.Game.World
{
    public sealed class MHWContext : Context
    {

        private static MHWStrings _strings;
        public static MHWStrings Strings => _strings;

        internal MHWContext(IProcessManager process)
        {
            _strings = new MHWStrings(process);
            Game = new MHWGame(process);
            Process = process;
        }

        public override void Dispose() {}
    }
}
