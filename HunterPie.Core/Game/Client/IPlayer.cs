using HunterPie.Core.Game.Enums;
using System;
using System.Collections.Generic;

namespace HunterPie.Core.Game.Client
{
    public interface IPlayer
    {
        public string Name { get; }
        public int HighRank { get; }
        public int MasterRank { get; }
        public int StageId { get; }
        public bool InHuntingZone { get; }
        public Weapon WeaponId { get; }
        public IParty Party { get; }
        public IReadOnlyCollection<IAbnormality> Abnormalities { get; }

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
    }
}
