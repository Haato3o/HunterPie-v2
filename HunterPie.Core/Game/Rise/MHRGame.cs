using HunterPie.Core.Address.Map;
using HunterPie.Core.Domain;
using HunterPie.Core.Domain.Interfaces;
using HunterPie.Core.Domain.Process;
using HunterPie.Core.Extensions;
using HunterPie.Core.Game.Client;
using HunterPie.Core.Game.Enums;
using HunterPie.Core.Game.Environment;
using HunterPie.Core.Game.Rise.Definitions;
using HunterPie.Core.Game.Rise.Entities;
using HunterPie.Core.Game.Rise.Entities.Chat;
using HunterPie.Core.Native.IPC.Handlers.Internal.Damage;
using HunterPie.Core.Native.IPC.Handlers.Internal.Damage.Models;
using HunterPie.Core.Native.IPC.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HunterPie.Core.Game.Rise
{
#pragma warning disable IDE0051 // Remove unused private members
    public class MHRGame : Scannable, IGame, IEventDispatcher, IDisposable
    {
        public const uint MAXIMUM_MONSTER_ARRAY_SIZE = 5;
        public const long ALL_TARGETS = 0;
        public const int CHAT_MAX_SIZE = 0x40;
        public const int TRAINING_ROOM_ID = 5;
        
        private readonly MHRChat _chat = new MHRChat();
        private readonly MHRPlayer _player;
        private float _timeElapsed;
        private (int, DateTime) _lastTeleport = (0, DateTime.Now);
        private int _deaths;
        private bool _isHudOpen;
        private DateTime _lastDamageUpdate = DateTime.MinValue;
        readonly Dictionary<long, IMonster> _monsters = new();
        readonly Dictionary<long, EntityDamageData[]> _damageDone = new();

        public IPlayer Player => _player;
        public List<IMonster> Monsters { get; } = new();

        public IChat Chat => _chat;

        public bool IsHudOpen
        {
            get => _isHudOpen;
            private set
            {
                if (value != _isHudOpen)
                {
                    _isHudOpen = value;
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

        public event EventHandler<IMonster> OnMonsterSpawn;
        public event EventHandler<IMonster> OnMonsterDespawn;
        public event EventHandler<IGame> OnHudStateChange;
        public event EventHandler<IGame> OnTimeElapsedChange;
        public event EventHandler<IGame> OnDeathCountChange;

        public MHRGame(IProcessManager process) : base(process)
        {
            _player = new MHRPlayer(process);

            ScanManager.Add(
                this,
                Player as Scannable
            );

            HookEvents();
        }
        
        private void HookEvents()
        {
            DamageMessageHandler.OnReceived += OnReceivePlayersDamage;
            _player.OnStageUpdate += OnPlayerStageUpdate;
        }

        public void Dispose()
        {
            DamageMessageHandler.OnReceived -= OnReceivePlayersDamage;
            _player.OnStageUpdate -= OnPlayerStageUpdate;
        }


        [ScannableMethod]
        private void ScanChat()
        {
            long chatArrayPtr = _process.Memory.Read(
                AddressMap.GetAbsolute("CHAT_ADDRESS"),
                AddressMap.Get<int[]>("CHAT_OFFSETS")
            );
            long chatArray = _process.Memory.Read<long>(chatArrayPtr);
            int chatCount = _process.Memory.Read<int>(chatArrayPtr + 0x8);

            if (chatCount <= 0)
                return;

            long[] chatMessagePtrs = _process.Memory.Read<long>(chatArray + 0x20, (uint)chatCount);

            bool isChatOpen = false;

            for (int i = 0; i < chatCount; i++)
            {
                long messagePtr = chatMessagePtrs[i];

                MHRChatMessageStructure message = _process.Memory.Read<MHRChatMessageStructure>(messagePtr);

                if (message.Type != 0 && message.Type != 1)
                    continue;

                if (!isChatOpen)
                    isChatOpen |= message.Visibility == 2;

                if (_chat.ConstainsMessage(messagePtr))
                    continue;

                MHRChatMessage messageData = DerefChatMessage(message);

                _chat.AddMessage(messagePtr, messageData);
            }

            if (!isChatOpen)
                isChatOpen |= _process.Memory.Deref<byte>(
                    AddressMap.GetAbsolute("CHAT_UI_ADDRESS"),
                    AddressMap.Get<int[]>("CHAT_UI_OFFSETS")
                ) == 1;

            _chat.IsChatOpen = isChatOpen;
        }

        [ScannableMethod]
        private void GetElapsedTime()
        {
            float elapsedTime = _process.Memory.Deref<float>(
                AddressMap.GetAbsolute("QUEST_ADDRESS"),
                AddressMap.Get<int[]>("QUEST_TIMER_OFFSETS")
            );

            TimeElapsed = elapsedTime > 0 
                ? elapsedTime 
                : (float)(DateTime.Now - _lastTeleport.Item2).TotalSeconds;

            if (Player.StageId != _lastTeleport.Item1)
            {
                _lastTeleport = (Player.StageId, DateTime.Now);
            }
        }

        [ScannableMethod]
        private void GetPartyMembersDamage()
        {
            if ((DateTime.Now - _lastDamageUpdate).TotalMilliseconds < 100)
                return;

            _lastDamageUpdate = DateTime.Now;

            if (Player.InHuntingZone)
                DamageMessageHandler.RequestHuntStatistics(ALL_TARGETS);
        }

        [ScannableMethod]
        private void ScanUIState()
        {
            byte isHudOpen = _process.Memory.Deref<byte>(
                AddressMap.GetAbsolute("MOUSE_ADDRESS"),
                AddressMap.Get<int[]>("MOUSE_OFFSETS")
            );

            IsHudOpen = isHudOpen == 1;
        }

        [ScannableMethod]
        private void ScanMonstersArray()
        {
            // Only scans for monsters in hunting areas
            if (!Player.InHuntingZone && Player.StageId != TRAINING_ROOM_ID)
            {
                if (_monsters.Keys.Count > 0)
                    foreach (long mAddress in _monsters.Keys)
                        HandleMonsterDespawn(mAddress);

                return;
            }

            long address = _process.Memory.Read(
                AddressMap.GetAbsolute("MONSTERS_ADDRESS"),
                AddressMap.Get<int[]>("MONSTER_LIST_OFFSETS")
            );

            uint monsterArraySize = _process.Memory.Read<uint>(address - 0x8);
            HashSet<long> monsterAddresses = _process.Memory.Read<long>(address + 0x20, Math.Min(MAXIMUM_MONSTER_ARRAY_SIZE, monsterArraySize))
                .ToHashSet();

            long[] toDespawn = _monsters.Keys.Where(address => !monsterAddresses.Contains(address))
                .ToArray();

            foreach (long mAddress in toDespawn)
                HandleMonsterDespawn(mAddress);

            long[] toSpawn = monsterAddresses.Where(address => !_monsters.ContainsKey(address))
                .ToArray();

            foreach (long mAddress in toSpawn)
                HandleMonsterSpawn(mAddress);

        }

        private void HandleMonsterSpawn(long monsterAddress)
        {
            if (monsterAddress == 0 || _monsters.ContainsKey(monsterAddress))
                return;

            IMonster monster = new MHRMonster(_process, monsterAddress);
            _monsters.Add(monsterAddress, monster);
            Monsters.Add(monster);
            ScanManager.Add(monster as Scannable);

            this.Dispatch(OnMonsterSpawn, monster);
        }

        private void HandleMonsterDespawn(long address)
        {
            IMonster monster = _monsters[address]; 
            _monsters.Remove(address);
            _damageDone.Remove(address);
            Monsters.Remove(monster);
            ScanManager.Remove(monster as Scannable);

            this.Dispatch(OnMonsterDespawn, monster);
        }

        #region Damage helpers

        private void OnPlayerStageUpdate(object sender, EventArgs e)
        {
            DamageMessageHandler.ClearAllHuntStatisticsExcept(Array.Empty<long>());
            DamageMessageHandler.RequestHuntStatistics(ALL_TARGETS);
        }

        private void OnReceivePlayersDamage(object sender, ResponseDamageMessage e)
        {
            long target = e.Target;

            _damageDone[target] = e.Entities;

            EntityDamageData[] damages = _damageDone.Values.SelectMany(entity => entity)
                .GroupBy(entity => entity.Entity.Index)
                .Select(group =>
                {
                    EntityDamageData entity = group.ElementAt(0);

                    return new EntityDamageData
                    {
                        Target = entity.Target,
                        Entity = entity.Entity,
                        RawDamage = group.Sum(e => e.RawDamage),
                        ElementalDamage = group.Sum(e => e.ElementalDamage)
                    };
                })
                .ToArray();

            _player.UpdatePartyMembersDamage(damages);
        }

        #endregion

        #region Chat helpers
        private MHRChatMessage DerefChatMessage(MHRChatMessageStructure message)
        {
            return message.Type switch
            {
                0x0 => DerefNormalChatMessage(message),
                _ => DerefUnknownTypeMessage(message)
            };
        }

        private MHRChatMessage DerefNormalChatMessage(MHRChatMessageStructure message)
        {
            int messageStringLength = _process.Memory.Read<int>(message.Message + 0x10);
            int messageAuthorLength = _process.Memory.Read<int>(message.Author + 0x10);

            string messageString = _process.Memory.Read(message.Message + 0x14, (uint)messageStringLength * 2, Encoding.Unicode);
            string messageAuthor = _process.Memory.Read(message.Author + 0x14, (uint) messageAuthorLength * 2, Encoding.Unicode);

            return new()
            {
                Message = messageString,
                Author = messageAuthor,
                Type = AuthorType.Player,
                PlayerSlot = message.PlayerSlot,
            };
        }

        private MHRChatMessage DerefAutoChatMessage(MHRChatMessageStructure message)
        {
            int messageAuthorLength = _process.Memory.Read<int>(message.Author + 0x10);
            string messageAuthor = _process.Memory.Read(messageAuthorLength + 0x14, (uint)messageAuthorLength * 2, Encoding.Unicode);

            return new()
            {
                Message = "<Auto message>",
                Author = messageAuthor,
                Type = AuthorType.Auto
            };
        }

        private MHRChatMessage DerefUnknownTypeMessage(MHRChatMessageStructure message) => new() { Type = AuthorType.None };

        #endregion
    }
#pragma warning restore IDE0051 // Remove unused private members
}
