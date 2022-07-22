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
using HunterPie.Core.Game.Rise.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace HunterPie.Core.Game.Demos.Sunbreak.Entities.Player
{
    public class MHRSunbreakDemoPlayer : Scannable, IPlayer, IEventDispatcher
    {
        #region Private
        private int SaveSlotId;
        private string _name;
        private readonly Dictionary<string, IAbnormality> abnormalities = new();
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

        public int StageId { get; private set; }

        public bool InHuntingZone { get; private set; }

        public Weapon WeaponId { get; private set; }

        public IParty Party { get; private set; }

        public IReadOnlyCollection<IAbnormality> Abnormalities => abnormalities.Values;

        public int MasterRank { get; private set; }

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

        public MHRSunbreakDemoPlayer(IProcessManager process) : base(process) { }

        [ScannableMethod]
        private void ScanStageData()
        {
            long stageAddress = _process.Memory.Read(
                AddressMap.GetAbsolute("STAGE_ADDRESS"),
                AddressMap.Get<int[]>("STAGE_OFFSETS")
            );

            // TODO: Transform this into a structure instead of an array
            int[] stageIds = _process.Memory.Read<int>(stageAddress + 0x150, 4);

            bool isVillage = stageIds[0] == 4;
            bool isMainMenu = stageIds[0] == 0;

            int villageId = stageIds[1];
            int huntId = stageIds[3];

            int zoneId = isMainMenu switch
            {
                true => -1,
                false => isVillage
                ? villageId
                : huntId + 200
            };

            StageId = zoneId;
        }

        [ScannableMethod]
        private void ScanPlayerSaveData()
        {
            if (StageId == -1 || StageId == 199)
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
            if (StageId == -1 || StageId == 199)
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
        private void AbnormalitiesCleanup()
        {
            long debuffsPtr = _process.Memory.Read(
                AddressMap.GetAbsolute("ABNORMALITIES_ADDRESS"),
                AddressMap.Get<int[]>("DEBUFF_ABNORMALITIES_OFFSETS")
            );

            if (debuffsPtr == 0)
                ClearAbnormalities();
        }

        [ScannableMethod]
        private void GetPlayerConsumableAbnormalities()
        {

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
        private void GetPlayerSongAbnormalities()
        {

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearAbnormalities()
        {
            foreach (IAbnormality abnormality in abnormalities.Values)
                this.Dispatch(OnAbnormalityEnd, abnormality);

            abnormalities.Clear();
        }
    }
}
