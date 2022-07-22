using HunterPie.Core.Domain.Process;
using HunterPie.Core.Game.Rise.Data;

namespace HunterPie.Core.Game.Demos.Sunbreak
{
    public class MHRSunbreakDemoContext : Context
    {
        private static MHRStrings _strings;
        public static MHRStrings Strings => _strings;

        internal MHRSunbreakDemoContext(IProcessManager process)
        {
            _strings = new MHRStrings(process);
            Game = new MHRSunbreakDemoGame(process);
            Process = process;
        }

        public override void Dispose()
        {
            
        }
    }
}
