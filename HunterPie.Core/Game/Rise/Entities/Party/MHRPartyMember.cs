using HunterPie.Core.Domain.Interfaces;
using HunterPie.Core.Extensions;
using HunterPie.Core.Game.Client;
using HunterPie.Core.Game.Enums;
using HunterPie.Core.Game.Rise.Definitions;
using HunterPie.Core.Native.IPC.Models.Common;
using System;

namespace HunterPie.Core.Game.Rise.Entities.Party
{
    public class MHRPartyMember : IPartyMember, IEventDispatcher, IUpdatable<MHRPartyMemberData>, IUpdatable<EntityDamageData>
    {
        private int _damage;
        private Weapon _weapon;

        public string Name { get; private set; }

        public int Damage
        {
            get => _damage;
            private set
            {
                if (value != _damage)
                {
                    _damage = value;
                    this.Dispatch(OnDamageDealt, this);
                }
            }
        }

        public Weapon Weapon
        {
            get => _weapon;
            private set
            {
                if (value != _weapon)
                {
                    _weapon = value;
                    this.Dispatch(OnWeaponChange, this);
                }
            }
        }

        public int Slot { get; private set; }

        public bool IsMyself { get; private set; }

        public event EventHandler<IPartyMember> OnDamageDealt;
        public event EventHandler<IPartyMember> OnWeaponChange;

        void IUpdatable<MHRPartyMemberData>.Update(MHRPartyMemberData data)
        {
            Name = data.Name;
            Weapon = data.WeaponId;
            IsMyself = data.IsMyself;
            Slot = data.Index;
        }

        void IUpdatable<EntityDamageData>.Update(EntityDamageData data)
        {
            float totalDamage = data.RawDamage + data.ElementalDamage;
            Damage = (int)totalDamage;
        }
    }
}
