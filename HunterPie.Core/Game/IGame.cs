using HunterPie.Core.Game.Client;
using HunterPie.Core.Game.Environment;
using System;
using System.Collections.Generic;

namespace HunterPie.Core.Game
{
    public interface IGame : IDisposable
    {

        public IPlayer Player { get; }
        public List<IMonster> Monsters { get; }

        public IChat Chat { get; }

        public bool IsHudOpen { get; }

        public float TimeElapsed { get; }
        public int Deaths { get; }

        public event EventHandler<IMonster> OnMonsterSpawn;
        public event EventHandler<IMonster> OnMonsterDespawn;
        public event EventHandler<IGame> OnHudStateChange;
        public event EventHandler<IGame> OnTimeElapsedChange;
        public event EventHandler<IGame> OnDeathCountChange;
    }
}
