using HunterPie.Core.Domain.Process;
using HunterPie.Core.Game.Rise.Data;

namespace HunterPie.Core.Game.Rise
{
    public sealed class MHRContext : Context
    {

        private static MHRStrings _strings;
        public static MHRStrings Strings => _strings;

        internal MHRContext(IProcessManager process)
        {
            _strings = new MHRStrings(process);
            Game = new MHRGame(process);
            Process = process;
        }

        override public void Dispose()
        {
            Game.Dispose();
        }
    }
}
