using HunterPie.Core.Address.Map;
using HunterPie.Core.Domain;
using HunterPie.Core.Domain.Interfaces;
using HunterPie.Core.Domain.Process;
using HunterPie.Core.Extensions;
using HunterPie.Core.Game.Client;
using HunterPie.Core.Game.Data;
using HunterPie.Core.Game.Data.Schemas;
using HunterPie.Core.Game.Enums;
using HunterPie.Core.Game.Rise.Definitions;
using HunterPie.Core.Game.Rise.Entities.Activities;
using HunterPie.Core.Game.Rise.Entities.Party;
using HunterPie.Core.Game.Rise.Utils;
using HunterPie.Core.Native.IPC.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace HunterPie.Core.Game.Rise.Entities
{
    public class MHRPlayer : Scannable, IPlayer, IEventDispatcher
    {
        #region Private
        private int SaveSlotId;
        private string _name;
        private int _stageId = -1;
        private Weapon _weaponId;
        private readonly Dictionary<string, IAbnormality> abnormalities = new();
        private readonly MHRParty _party = new();
        private MHRStageStructure _stageData = new();
        private MHRStageStructure _lastStageData = new();
        #endregion
        
        public string Name
        {
            get => _name;
            private set
            {
                if (value != _name)
                {
                    _name = value;
                    FindPlayerSaveSlot();
                    this.Dispatch(value is ""
                        ? OnLogout
                        : OnLogin);

                }
            }
        }

        public int HighRank { get; private set; }

        public int MasterRank { get; private set; }

        public int StageId
        {
            get => _stageId;
            private set
            {
                if (value != _stageId)
                {
                    if (_stageData.IsVillage() && value != 5 && ((_lastStageData.IsHuntingZone() || StageId == 5) || _lastStageData.IsIrrelevantStage()))
                        this.Dispatch(OnVillageEnter);
                    else if (_stageData.IsHuntingZone() || value == 5)
                        this.Dispatch(OnVillageLeave);

                    int temp = _stageId;
                    
                    _stageId = value;
                    this.Dispatch(OnStageUpdate);

                }
            }
        }

        public Weapon WeaponId
        {
            get => _weaponId;
            private set
            {
                if (value != _weaponId)
                {
                    _weaponId = value;
                    this.Dispatch(OnWeaponChange);
                }
            }
        }

        public bool InHuntingZone => _stageData.IsHuntingZone() || StageId == 5;

        public IParty Party => _party;
        
        public IReadOnlyCollection<IAbnormality> Abnormalities => abnormalities.Values;

        public MHRWirebug[] Wirebugs { get; } = { new(), new(), new() };

        public MHRArgosy Argosy { get; } = new();
        public MHRTrainingDojo TrainingDojo { get; } = new();
        public MHRMeowmasters Meowmasters { get; } = new();

        public event EventHandler<EventArgs> OnLogin;
        public event EventHandler<EventArgs> OnLogout;
        public event EventHandler<EventArgs> OnHealthUpdate;
        public event EventHandler<EventArgs> OnStaminaUpdate;
        public event EventHandler<EventArgs> OnDeath;
        public event EventHandler<EventArgs> OnActionUpdate;
        public event EventHandler<EventArgs> OnStageUpdate;
        public event EventHandler<EventArgs> OnVillageEnter;
        public event EventHandler<EventArgs> OnVillageLeave;
        public event EventHandler<EventArgs> OnAilmentUpdate;
        public event EventHandler<EventArgs> OnWeaponChange;
        public event EventHandler<IAbnormality> OnAbnormalityStart;
        public event EventHandler<IAbnormality> OnAbnormalityEnd;
        public event EventHandler<MHRWirebug[]> OnWirebugsRefresh;
        
        public MHRPlayer(IProcessManager process) : base(process) { }

        // TODO: Add DTOs for middlewares

        [ScannableMethod]
        private void GetStageData()
        {
            long stageAddress = _process.Memory.Read(
                AddressMap.GetAbsolute("STAGE_ADDRESS"),
                AddressMap.Get<int[]>("STAGE_OFFSETS")
            );

            if (stageAddress == 0x00000000)
                return;

            // TODO: Transform this into a structure instead of an array
            MHRStageStructure stageData = _process.Memory.Read<MHRStageStructure>(stageAddress + 0x60);
            
            int zoneId;
            if (stageData.IsMainMenu())
                zoneId = -1;
            else if (stageData.IsVillage())
                zoneId = stageData.VillageId;
            else if (stageData.IsLoadingScreen())
                zoneId = -2;
            else if (stageData.IsSelectingCharacter())
                zoneId = 199;
            else
                zoneId = stageData.HuntingId + 200;

            var tempStageData = _stageData;
            _stageData = stageData;
            _lastStageData = tempStageData;

            StageId = zoneId;
        }

        [ScannableMethod]
        private void GetPlayerSaveData()
        {
            if (_stageData.IsMainMenu())
            {
                Name = "";
                return;
            }

            long currentPlayerSaveAddress = _process.Memory.Read(
                AddressMap.GetAbsolute("CHARACTER_ADDRESS"),
                AddressMap.Get<int[]>("CHARACTER_OFFSETS")
            );

            long namePtr = _process.Memory.Read<long>(currentPlayerSaveAddress);
            int nameLength = _process.Memory.Read<int>(namePtr + 0x10);
            string name = _process.Memory.Read(namePtr + 0x14, (uint)(nameLength * 2), encoding: Encoding.Unicode);

            Name = name;
        }

        private void FindPlayerSaveSlot()
        {
            if (_stageData.IsMainMenu())
            {
                Name = "";
                SaveSlotId = -1;
                return;
            }

            long currentPlayerSaveAddress = _process.Memory.Read(
                AddressMap.GetAbsolute("CHARACTER_ADDRESS"),
                AddressMap.Get<int[]>("CHARACTER_OFFSETS")
            );
            long namePtr = _process.Memory.Read<long>(currentPlayerSaveAddress);

            long saveAddress = _process.Memory.Read(
                AddressMap.GetAbsolute("SAVE_ADDRESS"), 
                AddressMap.Get<int[]>("SAVE_OFFSETS")
            );

            for (int i = 0; i < 3; i++)
            {
                int[] nameOffsets = { i * 8 + 0x20, 0x10 };

                long saveNamePtr = _process.Memory.Deref<long>(saveAddress, nameOffsets);

                if (saveNamePtr == namePtr)
                {
                    SaveSlotId = i;
                    return;
                }
            }
        }

        [ScannableMethod]
        private void GetPlayerLevel()
        {
            if (SaveSlotId < 0)
                return;

            long saveAddress = _process.Memory.Read(
                AddressMap.GetAbsolute("SAVE_ADDRESS"),
                AddressMap.Get<int[]>("SAVE_OFFSETS")
            );

            int[] levelOffsets = { SaveSlotId * 8 + 0x20, 0x18 };

            var level = _process.Memory.Deref<MHRPlayerLevelStructure>(saveAddress, levelOffsets);

            HighRank = level.HighRank;
            MasterRank = level.MasterRank;
        }

        [ScannableMethod]
        private void GetPlayerWeaponData()
        {
            long weaponIdPtr = _process.Memory.Read(
                AddressMap.GetAbsolute("WEAPON_ADDRESS"),
                AddressMap.Get<int[]>("WEAPON_OFFSETS")    
            );

            int weaponId = _process.Memory.Read<int>(weaponIdPtr + 0x8C);

            WeaponId = weaponId.ToWeaponId();
        }

        [ScannableMethod]
        private void GetPlayerAbnormalitiesCleanup()
        {
            long debuffsPtr = _process.Memory.Read(
                AddressMap.GetAbsolute("ABNORMALITIES_ADDRESS"),
                AddressMap.Get<int[]>("DEBUFF_ABNORMALITIES_OFFSETS")
            );

            if (!InHuntingZone || debuffsPtr == 0)
                ClearAbnormalities();
        }

        [ScannableMethod]
        private void GetPlayerConsumableAbnormalities()
        {
            if (!InHuntingZone)
                return;

            long consumableBuffs = _process.Memory.Read(
                AddressMap.GetAbsolute("ABNORMALITIES_ADDRESS"), 
                AddressMap.Get<int[]>("CONS_ABNORMALITIES_OFFSETS")
            );

            if (consumableBuffs == 0)
                return;

            AbnormalitySchema[] consumableSchemas = AbnormalityData.GetAllAbnormalitiesFromCategory(AbnormalityData.Consumables);

            foreach (AbnormalitySchema schema in consumableSchemas)
            {
                int abnormSubId = schema.DependsOn switch
                {
                    0 => 0,
                    _ => _process.Memory.Read<int>(consumableBuffs + schema.DependsOn)
                };

                MHRConsumableStructure abnormality = new();

                if (abnormSubId == schema.WithValue)
                    abnormality = _process.Memory.Read<MHRConsumableStructure>(consumableBuffs + schema.Offset);

                abnormality.Timer /= AbnormalityData.TIMER_MULTIPLIER;

                HandleAbnormality<MHRConsumableAbnormality, MHRConsumableStructure>(schema, abnormality.Timer, abnormality);
            }
        }

        [ScannableMethod]
        private void GetPlayerDebuffAbnormalities()
        {
            // Only scan in hunting zone due to invalid pointer when not in a hunting zone...
            if (!InHuntingZone)
                return;

            long debuffsPtr = _process.Memory.Read(
                AddressMap.GetAbsolute("ABNORMALITIES_ADDRESS"), 
                AddressMap.Get<int[]>("DEBUFF_ABNORMALITIES_OFFSETS")
            );

            if (debuffsPtr == 0)
                return;

            AbnormalitySchema[] debuffSchemas = AbnormalityData.GetAllAbnormalitiesFromCategory(AbnormalityData.Debuffs);
            
            foreach (AbnormalitySchema schema in debuffSchemas)
            {
                int abnormSubId = schema.DependsOn switch
                {
                    0 => 0,
                    _ => _process.Memory.Read<int>(debuffsPtr + schema.DependsOn)
                };

                MHRDebuffStructure abnormality = new();

                // Only read memory if the required sub Id is the required one for this abnormality
                if (abnormSubId == schema.WithValue)
                    abnormality = _process.Memory.Read<MHRDebuffStructure>(debuffsPtr + schema.Offset);

                abnormality.Timer /= AbnormalityData.TIMER_MULTIPLIER;

                HandleAbnormality<MHRDebuffAbnormality, MHRDebuffStructure>(schema, abnormality.Timer, abnormality);
            }
        }

        [ScannableMethod]
        private void GetSessionPlayers()
        {
            int questState = _process.Memory.Deref<int>(
                AddressMap.GetAbsolute("QUEST_ADDRESS"),
                AddressMap.Get<int[]>("QUEST_STATUS_OFFSETS")
            );

            if (questState.IsQuestFinished())
                return;

            // Only scan party members when the player is not in the quest end screen
            if (!questState.IsInQuest() && !StageId.IsTrainingRoom())
            {
                _party.Clear();
                return;
            }

            long playersArrayPtr = _process.Memory.Deref<long>(
                AddressMap.GetAbsolute("CHARACTER_ADDRESS"),
                AddressMap.Get<int[]>("SOS_SESSION_PLAYER_OFFSETS")
            );

            long playersWeaponPtr = _process.Memory.Deref<long>(
                AddressMap.GetAbsolute("SESSION_PLAYERS_ADDRESS"),
                AddressMap.Get<int[]>("SESSION_PLAYER_OFFSETS")
            );

            (int index, bool isValid, MHRCharacterData data)[] sessionPlayersArray = _process.Memory.Read<long>(playersArrayPtr + 0x20, 4)
                .Select(pointer => (isValid: pointer != 0, pointer))
                .Select((player, index) => (index, player.isValid, _process.Memory.Read<MHRCharacterData>(player.pointer)))
                .ToArray();

            bool isSos = sessionPlayersArray.Any(player => player.isValid);

            if (!isSos)
            {
                playersArrayPtr = _process.Memory.Deref<long>(
                    AddressMap.GetAbsolute("CHARACTER_ADDRESS"),
                    AddressMap.Get<int[]>("CHARACTER_INFO_OFFSETS")
                );

                sessionPlayersArray = _process.Memory.Read<long>(playersArrayPtr + 0x20, 4)
                                                     .Select(pointer => (isValid: pointer != 0, pointer))
                                                     .Select((player, index) => (index, player.isValid, _process.Memory.Read<MHRCharacterData>(player.pointer)))
                                                     .ToArray();
            }

            bool isOnlineSession = sessionPlayersArray.Any(player => player.isValid);

            long[] playerWeaponsPtr = _process.Memory.Read<long>(playersWeaponPtr + 0x20, 4);

            // In case player DC'd
            if (!isOnlineSession)
            {
                if (string.IsNullOrEmpty(Name))
                    return;

                _party.Update(new MHRPartyMemberData()
                {
                    Index = 0,
                    Name = Name,
                    HighRank = HighRank,
                    WeaponId = WeaponId,
                    IsMyself = true
                });
                return;
            }

            foreach (var playerData in sessionPlayersArray)
            {
                long weaponPtr = playerWeaponsPtr[playerData.index];

                if (weaponPtr == 0)
                {
                    _party.Remove(playerData.index);
                    continue;
                }

                string name = _process.Memory.Read(playerData.data.NamePointer + 0x14, 32, Encoding.Unicode);

                if (string.IsNullOrEmpty(name))
                    continue;

                Weapon weapon = _process.Memory.Read<int>(weaponPtr + 0x134).ToWeaponId();

                MHRPartyMemberData memberData = new MHRPartyMemberData
                {
                    Index = playerData.index,
                    Name = name,
                    HighRank = playerData.data.HighRank,
                    WeaponId = weapon,
                    IsMyself = name == Name
                };

                _party.Update(memberData);
            }
        }

        [ScannableMethod]
        private void GetPlayerSongAbnormalities()
        {
            if (!InHuntingZone)
                return;

            long songBuffsPtr = _process.Memory.Read(
                AddressMap.GetAbsolute("ABNORMALITIES_ADDRESS"), 
                AddressMap.Get<int[]>("HH_ABNORMALITIES_OFFSETS")
            );

            if (songBuffsPtr == 0)
                return;

            uint songBuffsLength = _process.Memory.Read<uint>(songBuffsPtr + 0x1C);
            long[] songBuffPtrs = _process.Memory.Read<long>(songBuffsPtr + 0x20, songBuffsLength);

            DerefSongBuffs(songBuffPtrs);
        }
        
        [ScannableMethod(typeof(MHRWirebugStructure))]
        private void GetPlayerWirebugs()
        {
            long wirebugsArrayPtr = _process.Memory.Read(
                AddressMap.GetAbsolute("ABNORMALITIES_ADDRESS"), 
                AddressMap.Get<int[]>("WIREBUG_DATA_OFFSETS")
            );

            if (wirebugsArrayPtr == 0)
            {
                this.Dispatch(OnWirebugsRefresh, Array.Empty<MHRWirebug>());
                return;
            }

            int wirebugsArrayLength = Math.Min(Wirebugs.Length, _process.Memory.Read<int>(wirebugsArrayPtr + 0x1C));
            long[] wirebugsPtrs = _process.Memory.Read<long>(wirebugsArrayPtr + 0x20, (uint)wirebugsArrayLength);

            bool shouldDispatchEvent = false;
            for (int i = 0; i < wirebugsArrayLength; i++)
            {
                long wirebugPtr = wirebugsPtrs[i];
                MHRWirebugStructure data = _process.Memory.Read<MHRWirebugStructure>(wirebugPtr);
                data.Cooldown /= AbnormalityData.TIMER_MULTIPLIER;
                data.MaxCooldown /= AbnormalityData.TIMER_MULTIPLIER;
                
                if (wirebugPtr != Wirebugs[i].Address)
                {
                    shouldDispatchEvent = true;
                    Wirebugs[i].Address = wirebugPtr;
                }

                IUpdatable<MHRWirebugStructure> wirebug = Wirebugs[i];
                wirebug.Update(data);
            }

            // Update last Wirebug with extra data
            MHRWirebugExtrasStructure extraData = _process.Memory.Deref<MHRWirebugExtrasStructure>(
                AddressMap.GetAbsolute("ABNORMALITIES_ADDRESS"), 
                AddressMap.Get<int[]>("WIREBUG_EXTRA_DATA_OFFSETS")
            );
            extraData.Timer /= AbnormalityData.TIMER_MULTIPLIER;
            IUpdatable<MHRWirebugExtrasStructure> lastWirebug = Wirebugs[Wirebugs.Length - 1];
            lastWirebug.Update(extraData);

            if (shouldDispatchEvent)
                this.Dispatch(OnWirebugsRefresh, Wirebugs);
        }

        [ScannableMethod(typeof(MHRSubmarineData))]
        private void GetArgosy()
        {
            
            long argosyAddress = _process.Memory.Read(
                AddressMap.GetAbsolute("ARGOSY_ADDRESS"), 
                AddressMap.Get<int[]>("ARGOSY_OFFSETS")
            );

            int submarineArrayLength = _process.Memory.Read<int>(argosyAddress + 0x1C);
            MHRSubmarineData[] submarines = new MHRSubmarineData[submarineArrayLength];

            long[] submarinePtrs = _process.Memory.Read<long>(argosyAddress + 0x20, (uint)submarineArrayLength);
            
            // Read submarines data
            for (int i = 0; i < submarineArrayLength; i++)
            {
                ref long submarinePtr = ref submarinePtrs[i];
                MHRSubmarineStructure data = _process.Memory.Read<MHRSubmarineStructure>(submarinePtr);
                submarines[i].Data = data;
            }

            // Read submarine items array data
            for (int i = 0; i < submarines.Length; i++)
            {
                ref MHRSubmarineData submarine = ref submarines[i];

                int itemsArrayLength = _process.Memory.Read<int>(submarine.Data.ItemArrayPtr + 0x1C);
                long[] itemsPtr = _process.Memory.Read<long>(submarine.Data.ItemArrayPtr + 0x20, (uint)itemsArrayLength);
                MHRSubmarineItemStructure[] items = new MHRSubmarineItemStructure[itemsArrayLength];

                for (int j = 0; j < itemsArrayLength; j++)
                    items[j] = _process.Memory.Read<MHRSubmarineItemStructure>(itemsPtr[j]);

                submarine.Items = items;
            }

            for (int i = 0; i < Math.Min(Argosy.Submarines.Length, submarineArrayLength); i++)
            {
                IUpdatable<MHRSubmarineData> localData = Argosy.Submarines[i];
                localData.Update(submarines[i]);
            }
            
        }

        [ScannableMethod(typeof(MHRTrainingDojoData))]
        private void GetTrainingDojo()
        {
            int[] staticTrainingData = _process.Memory.Read<int>(AddressMap.GetAbsolute("DATA_TRAINING_DOJO_ROUNDS_LEFT"), 5); 
            MHRTrainingDojoData data = new()
            {
                Rounds = staticTrainingData[0],
                MaxRounds = staticTrainingData[1],
                Boosts = staticTrainingData[3],
                MaxBoosts = staticTrainingData[4],
                Buddies = new MHRBuddyData[6]
            };

            long trainingDojo = _process.Memory.Read(
                AddressMap.GetAbsolute("TRAINING_DOJO_ADDRESS"), 
                AddressMap.Get<int[]>("TRAINING_DOJO_OFFSETS")
            );

            data.BuddiesCount = _process.Memory.Read<int>(trainingDojo + 0x18);

            long trainingDojoBuddyArray = _process.Memory.Read(
                AddressMap.GetAbsolute("TRAINING_DOJO_ADDRESS"),
                AddressMap.Get<int[]>("TRAINING_DOJO_BUDDY_ARRAY_OFFSETS")
            );

            long[] buddyPtrs = _process.Memory.Read<long>(trainingDojoBuddyArray + 0x20, 6);

            for (int i = 0; i < data.BuddiesCount; i++)
            {
                data.Buddies[i] = DerefBuddyData(buddyPtrs[i]);
            }

            Next(ref data);

            IUpdatable<MHRTrainingDojoData> model = TrainingDojo;
            model.Update(data);
        }

        [ScannableMethod]
        private void GetPartyData()
        {
            long partyArrayPtr = _process.Memory.Read(
                AddressMap.GetAbsolute("SESSION_PLAYERS_ADDRESS"),
                AddressMap.Get<int[]>("SESSION_PLAYERS_ARRAY_OFFSETS")
            );

            long[] playerAddresses = _process.Memory.Read<long>(partyArrayPtr + 0x20, (uint)_party.MaxSize);

            int membersCount = playerAddresses.Count(address => address != 0x0);

            _party.Size = membersCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MHRBuddyData DerefBuddyData(long buddyPtr)
        {
            long namePtr = _process.Memory.ReadPtr(buddyPtr, AddressMap.Get<int[]>("BUDDY_NAME_OFFSETS"));
            long levelPtr = _process.Memory.ReadPtr(buddyPtr, AddressMap.Get<int[]>("BUDDY_LEVEL_OFFSETS"));
            int nameLength = _process.Memory.Read<int>(namePtr + 0x10);

            MHRBuddyData data = new()
            {
                Name = _process.Memory.Read(namePtr + 0x14, (uint)nameLength * 2, Encoding.Unicode),
                Level = _process.Memory.Read<int>(levelPtr + 0x24)
            };

            return data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DerefSongBuffs(long[] buffs)
        {
            int id = 0;
            AbnormalitySchema[] schemas = AbnormalityData.GetAllAbnormalitiesFromCategory(AbnormalityData.Songs);
            foreach (long buffPtr in buffs)
            {
                MHRHHAbnormality abnormality = _process.Memory.Read<MHRHHAbnormality>(buffPtr);
                abnormality.Timer /= AbnormalityData.TIMER_MULTIPLIER;

                AbnormalitySchema maybeSchema = schemas[id];
                
                if (maybeSchema is AbnormalitySchema schema)
                    HandleAbnormality<MHRSongAbnormality, MHRHHAbnormality>(schema, abnormality.Timer, abnormality);

                id++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearAbnormalities()
        {
            foreach (IAbnormality abnormality in abnormalities.Values)
                this.Dispatch(OnAbnormalityEnd, abnormality);

            abnormalities.Clear();
        }

        private void HandleAbnormality<T, S>(AbnormalitySchema schema, float timer, S newData) 
            where T : IAbnormality, IUpdatable<S>
            where S : struct
        {
            if (abnormalities.ContainsKey(schema.Id) && timer <= 0)
            {
                IUpdatable<S> abnorm = (IUpdatable<S>)abnormalities[schema.Id];

                abnorm.Update(newData);

                abnormalities.Remove(schema.Id);
                this.Dispatch(OnAbnormalityEnd, (IAbnormality)abnorm);
            }
            else if (abnormalities.ContainsKey(schema.Id) && timer > 0)
            {

                IUpdatable<S> abnorm = (IUpdatable<S>)abnormalities[schema.Id];
                abnorm.Update(newData);
            }
            else if (!abnormalities.ContainsKey(schema.Id) && timer > 0)
            {
                if (schema.Icon == "ICON_MISSING")
                    Logger.Log.Info($"Missing abnormality: {schema.Id}");

                IUpdatable<S> abnorm = (IUpdatable<S>)Activator.CreateInstance(typeof(T), schema);

                abnormalities.Add(schema.Id, (IAbnormality)abnorm);
                abnorm.Update(newData);
                this.Dispatch(OnAbnormalityStart, (IAbnormality)abnorm);
            }
        }

        internal void UpdatePartyMembersDamage(EntityDamageData[] entities)
        {
            foreach (EntityDamageData entity in entities)
                _party.Update(entity);

        }
    }
}
