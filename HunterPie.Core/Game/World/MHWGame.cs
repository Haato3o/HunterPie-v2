using HunterPie.Core.Address.Map;
using HunterPie.Core.Domain;
using HunterPie.Core.Domain.Interfaces;
using HunterPie.Core.Domain.Process;
using HunterPie.Core.Extensions;
using HunterPie.Core.Game.Client;
using HunterPie.Core.Game.Environment;
using HunterPie.Core.Game.World.Crypto;
using HunterPie.Core.Game.World.Entities;
using HunterPie.Core.Game.World.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HunterPie.Core.Game.World
{
    public class MHWGame : Scannable, IGame, IEventDispatcher
    {
        private readonly MHWPlayer _player;
        private readonly Dictionary<long, IMonster> _monsters = new();
        private bool _isMouseVisible;
        private float _timeElapsed;
        private int _deaths;

        public event EventHandler<IMonster> OnMonsterSpawn;
        public event EventHandler<IMonster> OnMonsterDespawn;
        public event EventHandler<IGame> OnHudStateChange;
        public event EventHandler<IGame> OnTimeElapsedChange;
        public event EventHandler<IGame> OnDeathCountChange;

        public IPlayer Player => _player;
        public List<IMonster> Monsters { get; } = new();

        public IChat Chat => throw new NotImplementedException();

        public bool IsHudOpen
        {
            get => _isMouseVisible;
            private set
            {
                if (value != _isMouseVisible)
                {
                    _isMouseVisible = value;
                    this.Dispatch(OnHudStateChange, this);
                }
            }
        }

        public float TimeElapsed
        {
            get => _timeElapsed;
            private set
            {
                if (value != _timeElapsed)
                {
                    _timeElapsed = value;
                    this.Dispatch(OnTimeElapsedChange, this);
                }
            }
        }

        public int Deaths
        {
            get => _deaths;
            private set
            {
                if (value != _deaths)
                {
                    _deaths = value;
                    this.Dispatch(OnDeathCountChange, this);
                }
            }
        }

        public MHWGame(IProcessManager process) : base(process)
        {
            _player = new(process);

            ScanManager.Add(_player, this);
        }

        [ScannableMethod]
        private void GetMouseVisibilityState()
        {
            bool isMouseVisible = _process.Memory.Deref<int>(
                AddressMap.GetAbsolute("GAME_MOUSE_INFO_ADDRESS"),
                AddressMap.Get<int[]>("MOUSE_VISIBILITY_OFFSETS")
            ) == 1;

            IsHudOpen = isMouseVisible;
        }

        [ScannableMethod]
        private void GetTimeElapsed()
        {
            long questEndTimerPtrs = _process.Memory.Read(
                AddressMap.GetAbsolute("QUEST_DATA_ADDRESS"),
                AddressMap.Get<int[]>("QUEST_TIMER_OFFSETS")
            );
            ulong[] timers = _process.Memory.Read<ulong>(questEndTimerPtrs, 2);
            ulong encryptKey = timers[0];
            ulong encryptedValue = timers[1];

            float questMaxTimer = _process.Memory.Read<uint>(questEndTimerPtrs + 0x1C)
                                                 .ApproximateHigh(MHWGameUtils.MaxQuestTimers)
                                                 .ToSeconds();

            float elapsed = MHWCrypto.DecryptQuestTimer(encryptedValue, encryptKey);

            TimeElapsed = Math.Max(0, questMaxTimer - elapsed);
        }

        [ScannableMethod]
        private void GetDeathCounter()
        {
            int deathCounter = _process.Memory.Deref<int>(
                AddressMap.GetAbsolute("QUEST_DATA_ADDRESS"),
                AddressMap.Get<int[]>("QUEST_DEATH_COUNTER_OFFSETS")
            );

            Deaths = deathCounter;
        }

        [ScannableMethod]
        private void GetMonsterDoubleLinkedList()
        {   
            long doubleLinkedListHead = _process.Memory.Read(
                AddressMap.GetAbsolute("MONSTER_ADDRESS"),
                AddressMap.Get<int[]>("MONSTER_OFFSETS")
            );

            long next = doubleLinkedListHead;
            bool isBigMonster;
            HashSet<long> monsterAddresses = new();
            do
            {
                long monsterEmPtr = _process.Memory.Read<long>(next + 0x2A0);
                string monsterEm = _process.Memory.Read(monsterEmPtr + 0x0C, 64);

                isBigMonster = monsterEmPtr != 0 
                    && monsterEm.StartsWith("em\\em")
                    && !monsterEm.StartsWith("em\\ems");

                if (!isBigMonster)
                    break;

                monsterAddresses.Add(next);

                string em = monsterEm.Split('\\')
                    .ElementAtOrDefault(1);

                HandleMonsterSpawn(next, em);

                next = _process.Memory.Read<long>(next - 0x30) + 0x40;
            } while (isBigMonster);

            long[] toDespawn = _monsters.Keys.Where(address => !monsterAddresses.Contains(address))
                .ToArray();

            foreach (long monsterAddress in toDespawn)
                HandleMonsterDespawn(monsterAddress);
        }

        private void HandleMonsterSpawn(long address, string em)
        {
            if (_monsters.ContainsKey(address))
                return;

            IMonster monster = new MHWMonster(_process, address, em);
            _monsters.Add(address, monster);
            Monsters.Add(monster);
            ScanManager.Add(monster as Scannable);

            this.Dispatch(OnMonsterSpawn, monster);
        }

        private void HandleMonsterDespawn(long address)
        {
            IMonster monster = _monsters[address];
            _monsters.Remove(address);
            Monsters.Remove(monster);
            ScanManager.Remove(monster as Scannable);

            this.Dispatch(OnMonsterDespawn, monster);
        }

        public void Dispose() {}
    }
}
