using HunterPie.Core.Address.Map;
using HunterPie.Core.Domain;
using HunterPie.Core.Domain.DTO;
using HunterPie.Core.Domain.Interfaces;
using HunterPie.Core.Domain.Process;
using HunterPie.Core.Extensions;
using HunterPie.Core.Game.Client;
using HunterPie.Core.Game.Enums;
using HunterPie.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HunterPie.Core.Game.World.Entities
{
    public class MHWPlayer : Scannable, IPlayer, IEventDispatcher
    {
        #region consts
        private readonly static Stage[] peaceZones =
        {
            Stage.Astera,
            Stage.SelianaSupplyCache,
            Stage.ResearchBase,
            Stage.Seliana,
            Stage.SelianaGatheringHub,
            Stage.LivingQuarters,
            Stage.PrivateQuarters,
            Stage.PrivateSuite,
            Stage.SelianaRoom
        };
        #endregion

        #region Private fields

        private long _playerAddress;
        private Stage _zoneId;
        private Weapon _weaponId;
        private SpecializedTool _primaryTool = new SpecializedTool();
        private SpecializedTool _secondaryTool = new SpecializedTool();

        #endregion

        #region Public fields

        public long PlayerAddress
        {
            get => _playerAddress;
            private set
            {
                if (value != _playerAddress)
                {
                    _playerAddress = value;

                    this.Dispatch(
                        value != 0
                        ? OnLogin
                        : OnLogout,
                        EventArgs.Empty
                    );

                    if (value != 0)
                        Log.Debug($"Logged in! Name: {Name}, HR: {HighRank}, MR: {MasterRank}, PlayTime: {PlayTime} seconds");
                }
            }
        }
        public string Name { get; private set; }
        public short HighRank { get; private set; }
        public short MasterRank { get; private set; }
        public int PlayTime { get; private set; }

        /// <summary>
        /// Player stage id
        /// </summary>
        public Stage ZoneId
        {
            get => _zoneId;
            set
            {
                if (value != _zoneId)
                {
                    if (peaceZones.Contains(value) && !peaceZones.Contains(_zoneId))
                        this.Dispatch(OnVillageEnter);
                    else if (!peaceZones.Contains(value) && peaceZones.Contains(_zoneId))
                        this.Dispatch(OnVillageLeave);

                    _zoneId = value;
                }
            }
        }

        /// <summary>
        /// Player weapon type
        /// </summary>
        public Weapon WeaponId
        {
            get => _weaponId;
            set
            {
                if (value != _weaponId)
                {
                    _weaponId = value;
                }
            }
        }



        public ref readonly SpecializedTool PrimaryTool => ref _primaryTool;
        public ref readonly SpecializedTool SecondaryTool => ref _secondaryTool;

        public bool IsLoggedOn => _playerAddress != 0;

        int IPlayer.HighRank => throw new NotImplementedException();

        public int StageId => throw new NotImplementedException();

        public List<IPartyMember> Party => throw new NotImplementedException();

        public IReadOnlyCollection<IAbnormality> Abnormalities => throw new NotImplementedException();

        IParty IPlayer.Party => throw new NotImplementedException();
        #endregion

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

        internal MHWPlayer(IProcessManager process) : base(process) { }

        [ScannableMethod(typeof(ZoneData))]
        private void GetZoneData()
        {
            ZoneData data = new();

            long zoneAddress = _process.Memory.Read(
                AddressMap.GetAbsolute("ZONE_OFFSET"),
                AddressMap.Get<int[]>("ZoneOffsets")
            );

            data.ZoneId = (Stage)_process.Memory.Read<int>(zoneAddress);

            Next(ref data);

            ZoneId = data.ZoneId;

        }

        [ScannableMethod(typeof(PlayerInformationData))]
        private void GetBasicData()
        {
            PlayerInformationData data = new();
            if (ZoneId == Stage.MainMenu)
            {
                PlayerAddress = 0;
                return;
            }

            long firstSaveAddress = _process.Memory.Read(
                AddressMap.GetAbsolute("LEVEL_OFFSET"),
                AddressMap.Get<int[]>("LevelOffsets")
            );

            uint currentSaveSlot = _process.Memory.Read<uint>(firstSaveAddress + 0x44);
            long nextPlayerSave = 0x27E9F0;
            long currentPlayerSaveHeader =
                _process.Memory.Read<long>(firstSaveAddress) + nextPlayerSave * currentSaveSlot;

            if (currentPlayerSaveHeader != _playerAddress)
            {
                data.Name = _process.Memory.Read(currentPlayerSaveHeader + 0x50, 32);
                data.HighRank = _process.Memory.Read<short>(currentPlayerSaveHeader + 0x90);
                data.MasterRank = _process.Memory.Read<short>(currentPlayerSaveHeader + 0xD4);
                data.PlayTime = _process.Memory.Read<int>(currentPlayerSaveHeader + 0xA0);

                Next(ref data);

                Name = data.Name;
                HighRank = data.HighRank;
                MasterRank = data.MasterRank;
                PlayTime = data.PlayTime;

                PlayerAddress = currentPlayerSaveHeader;
            }

        }

        [ScannableMethod(typeof(PlayerEquipmentData))]
        private void GetWeaponData()
        {
            PlayerEquipmentData data = new();

            if (!IsLoggedOn)
                return;

            long address = _process.Memory.Read(
                AddressMap.GetAbsolute("WEAPON_OFFSET"),
                AddressMap.Get<int[]>("WeaponOffsets")
            );

            data.WeaponType = (Weapon)_process.Memory.Read<byte>(address);
            int[] tools = _process.Memory.Read<int>(address, 2);
            data.PrimaryTool = (SpecializedToolType)tools[0];
            data.SecondaryTool = (SpecializedToolType)tools[1];

            Next(ref data);

            WeaponId = data.WeaponType;

            return;
        }

        public IGear GetCurrentGear()
        {
            throw new NotImplementedException();
        }
    }
}
