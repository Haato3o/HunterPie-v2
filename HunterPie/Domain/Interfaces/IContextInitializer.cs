using HunterPie.Core.Game;

namespace HunterPie.Domain.Interfaces
{
    internal interface IContextInitializer
    {
        public void Initialize(Context context);
        public void Unload(Context context);
    }
}
