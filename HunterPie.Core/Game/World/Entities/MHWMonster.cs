using HunterPie.Core.Address.Map;
using HunterPie.Core.Domain;
using HunterPie.Core.Domain.Interfaces;
using HunterPie.Core.Domain.Process;
using HunterPie.Core.Extensions;
using HunterPie.Core.Game.Data;
using HunterPie.Core.Game.Data.Schemas;
using HunterPie.Core.Game.Enums;
using HunterPie.Core.Game.Environment;
using HunterPie.Core.Game.World.Definitions;
using HunterPie.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HunterPie.Core.Game.World.Entities
{
    public class MHWMonster : Scannable, IMonster, IEventDispatcher
    {
        #region Private
        private readonly long _address;
        private int _id = -1;
        private int _doubleLinkedListIndex;
        private float _health = -1;
        private bool _isTarget;
        private bool _isEnraged;
        private Target _target;
        private Crown _crown;
        private float _stamina;
        private readonly MHWMonsterAilment _enrage = new MHWMonsterAilment("STATUS_ENRAGE");
        private (long, MHWMonsterPart)[] _parts;
        private List<(long, MHWMonsterAilment)> _ailments;
        #endregion

        public int Id
        {
            get => _id;
            private set
            {
                if (value != _id)
                {
                    _id = value;
                    this.Dispatch(OnSpawn);
                }
            }
        }

        public string Em { get; private set; }

        public string Name => MHWContext.Strings.GetMonsterNameById(Id);

        public float Health
        {
            get => _health;
            private set
            {
                if (value != _health)
                {
                    _health = value;
                    this.Dispatch(OnHealthChange);

                    if (Health <= 0)
                        this.Dispatch(OnDeath);
                }
            }
        }

        public float MaxHealth { get; private set; }

        public float Stamina
        {
            get => _stamina;
            private set
            {
                if (value != _stamina)
                {
                    _stamina = value;
                    this.Dispatch(OnStaminaChange);
                }
            }
        }

        public float MaxStamina { get; private set; }

        public bool IsTarget
        {
            get => _isTarget;
            private set
            {
                if (_isTarget != value)
                {
                    _isTarget = value;
                    this.Dispatch(OnTarget);
                }
            }
        }

        public IMonsterPart[] Parts => _parts?
                                        .Select(v => v.Item2)
                                        .ToArray<IMonsterPart>() ?? Array.Empty<IMonsterPart>();

        public IMonsterAilment[] Ailments => _ailments?
                                              .Select(a => a.Item2)
                                              .ToArray<IMonsterAilment>() ?? Array.Empty<IMonsterAilment>();
        public Target Target
        {
            get => _target;
            private set
            {
                if (_target != value)
                {
                    _target = value;
                    this.Dispatch(OnTargetChange);
                }
            }
        }

        public Crown Crown
        {
            get => _crown;
            private set
            {
                if (_crown != value)
                {
                    _crown = value;
                    this.Dispatch(OnCrownChange);
                }
            }
        }

        public bool IsEnraged
        {
            get => _isEnraged;
            private set
            {
                if (value != _isEnraged)
                {
                    _isEnraged = value;
                    this.Dispatch(OnEnrageStateChange);
                }
            }
        }

        public IMonsterAilment Enrage => _enrage;

        public Element[] Weaknesses => Array.Empty<Element>();

        // TODO: Maybe v2.3.0?
        public float CaptureThreshold => 0;

        public event EventHandler<EventArgs> OnSpawn;
        public event EventHandler<EventArgs> OnLoad;
        public event EventHandler<EventArgs> OnDespawn;
        public event EventHandler<EventArgs> OnDeath;
        public event EventHandler<EventArgs> OnCapture;
        public event EventHandler<EventArgs> OnTarget;
        public event EventHandler<EventArgs> OnCrownChange;
        public event EventHandler<EventArgs> OnHealthChange;
        public event EventHandler<EventArgs> OnStaminaChange;
        public event EventHandler<EventArgs> OnActionChange;
        public event EventHandler<EventArgs> OnTargetChange;
        public event EventHandler<IMonsterPart> OnNewPartFound;
        public event EventHandler<IMonsterAilment> OnNewAilmentFound;
        public event EventHandler<EventArgs> OnEnrageStateChange;
        public event EventHandler<Element[]> OnWeaknessesChange;
        public event EventHandler<IMonster> OnCaptureThresholdChange;

        public MHWMonster(IProcessManager process, long address, string em) : base(process)
        {
            _address = address;
            Em = em;
            
            Log.Debug($"Initialized monster at address {address:X}");
        }

        [ScannableMethod]
        private void GetMonsterBasicInformation()
        {
            int monsterId = _process.Memory.Read<int>(_address + 0x12280);
            int doubleLinkedListIndex = _process.Memory.Read<int>(_address + 0x1228C);

            Id = monsterId;
            _doubleLinkedListIndex = doubleLinkedListIndex;
        }

        [ScannableMethod]
        private void GetMonsterHealthData()
        {
            long monsterHealthPtr = _process.Memory.Read<long>(_address + 0x7670);
            float[] healthValues = _process.Memory.Read<float>(monsterHealthPtr + 0x60, 2);

            MaxHealth = healthValues[0];
            Health = healthValues[1];
        }

        [ScannableMethod]
        private void GetMonsterStaminaData()
        {
            float[] staminaValues = _process.Memory.Read<float>(_address + 0x1C0F0, 2);

            MaxStamina = staminaValues[1];
            Stamina = staminaValues[0];
        }

        [ScannableMethod]
        private void GetMonsterCrownData()
        {
            float sizeModifier = _process.Memory.Read<float>(_address + 0x7730);
            float sizeMultiplier = _process.Memory.Read<float>(_address + 0x188);
            
            if (sizeModifier <= 0 || sizeModifier >= 2)
                sizeModifier = 1;

            float monsterSizeMultiplier = sizeMultiplier / sizeModifier;

            MonsterSizeSchema? crownData = MonsterData.GetMonsterData(Id)?.Size;

            if (crownData is null)
                return;

            MonsterSizeSchema crown = crownData.Value;

            if (monsterSizeMultiplier >= crown.Gold)
                Crown = Crown.Gold;
            else if (monsterSizeMultiplier >= crown.Silver)
                Crown = Crown.Silver;
            else if (monsterSizeMultiplier <= crown.Mini)
                Crown = Crown.Mini;
            else
                Crown = Crown.None;
        }

        [ScannableMethod]
        private void GetMonsterEnrage()
        {
            MHWMonsterStatusStructure enrageStructure = _process.Memory.Read<MHWMonsterStatusStructure>(_address + 0x1BE30);
            IUpdatable<MHWMonsterStatusStructure> enrage = _enrage;

            IsEnraged = enrageStructure.Duration > 0;

            enrage.Update(enrageStructure);
        }

        [ScannableMethod]
        private void GetLockedOnMonster()
        {
            int lockedOnDoubleLinkedListIndex = _process.Memory.Deref<int>(
                AddressMap.GetAbsolute("LOCKON_ADDRESS"),
                AddressMap.Get<int[]>("LOCKEDON_MONSTER_INDEX_OFFSETS")
            );

            IsTarget = lockedOnDoubleLinkedListIndex == _doubleLinkedListIndex;

            if (IsTarget)
                Target = Target.Self;
            else if (lockedOnDoubleLinkedListIndex != -1)
                Target = Target.Another;
            else
                Target = Target.None;
                
        }

        [ScannableMethod]
        private void GetMonsterParts()
        {
            long monsterPartPtr = _process.Memory.Read<long>(_address + 0x1D058);

            if (monsterPartPtr == 0)
                return;

            long monsterPartAddress = monsterPartPtr + 0x40;
            long monsterSeverableAddress = monsterPartPtr + 0x1FC8;

            MonsterDataSchema? monsterSchema = MonsterData.GetMonsterData(Id);

            if (!monsterSchema.HasValue)
                return;

            MonsterDataSchema monsterInfo = monsterSchema.Value;

            if (_parts is null)
            {
                _parts = new (long, MHWMonsterPart)[monsterInfo.Parts.Length];
                for (int i = 0; i < _parts.Length; i++)
                    _parts[i] = (0, null);
            }

            int normalPartIndex = 0;

            for (int pIndex = 0; pIndex < monsterInfo.Parts.Length; pIndex++)
            {
                var (cachedAddress, part) = _parts[pIndex];
                IUpdatable<MHWMonsterPartStructure> updatable = _parts[pIndex].Item2;
                MonsterPartSchema partSchema = monsterInfo.Parts[pIndex];
                MHWMonsterPartStructure partStructure = new();

                // If the part address has been cached already, we can just read them
                if (cachedAddress > 0)
                {
                    partStructure = _process.Memory.Read<MHWMonsterPartStructure>(cachedAddress);

                    // Alatreon elemental explosion level
                    if (Id == 87 && partStructure.Index == 3)
                        partStructure.Counter = _process.Memory.Read<int>(_address + 0x20920);

                    updatable.Update(partStructure);
                    continue;
                }

                if (partSchema.IsSeverable)
                {
                    while (monsterSeverableAddress < (monsterSeverableAddress + 0x120 * 32))
                    {
                        if (_process.Memory.Read<int>(monsterSeverableAddress) <= 0xA0)
                            monsterSeverableAddress += 0x8;

                        partStructure = _process.Memory.Read<MHWMonsterPartStructure>(monsterSeverableAddress);

                        if (partStructure.Index == partSchema.Id && partStructure.MaxHealth > 0)
                        {
                            MHWMonsterPart newPart = new(
                                partSchema.String, 
                                partSchema.IsSeverable, 
                                partSchema.TenderizeIds
                            );
                            _parts[pIndex] = (monsterSeverableAddress, newPart);

                            this.Dispatch(OnNewPartFound, newPart);

                            do
                            {
                                monsterSeverableAddress += 0x78;
                            } while (partStructure.Equals(_process.Memory.Read<MHWMonsterPartStructure>(monsterSeverableAddress)));

                            break;
                        }

                        monsterSeverableAddress += 0x78;
                    }
                } else
                {
                    long address = monsterPartAddress + (normalPartIndex * 0x1F8);
                    partStructure = _process.Memory.Read<MHWMonsterPartStructure>(address);

                    MHWMonsterPart newPart = new(
                        partSchema.String,
                        partSchema.IsSeverable,
                        partSchema.TenderizeIds
                    );

                    _parts[pIndex] = (address, newPart);

                    this.Dispatch(OnNewPartFound, newPart);

                    normalPartIndex++;
                }

                updatable = _parts[pIndex].Item2;
                updatable.Update(partStructure);
            }
        }

        [ScannableMethod]
        private void GetMonsterPartTenderizes()
        {
            MHWTenderizeInfoStructure[] tenderizeInfos = _process.Memory.Read<MHWTenderizeInfoStructure>(
                _address + 0x1C458,
                10
            );

            foreach (MHWTenderizeInfoStructure tenderizeInfo in tenderizeInfos)
            {
                if (tenderizeInfo.PartId == uint.MaxValue)
                    continue;

                var parts = _parts.Select(p => p.Item2)
                                  .Where(p => p.HasTenderizeId(tenderizeInfo.PartId))
                                  .ToArray();

                foreach (IUpdatable<MHWTenderizeInfoStructure> part in parts)
                    part.Update(tenderizeInfo);
            }
        }

        [ScannableMethod]
        private void GetMonsterAilments()
        {
            if (_ailments is null)
            {
                _ailments = new(32);
                long monsterAilmentArrayElement = _address + 0x1BC40;
                long monsterAilmentPtr = _process.Memory.Read<long>(monsterAilmentArrayElement);
                
                while (monsterAilmentPtr > 1)
                {
                    long currentMonsterAilmentPtr = monsterAilmentPtr;
                    // Comment from V1 so I don't forget: There's a gap between the monsterAilmentPtr and the actual ailment data
                    MHWMonsterAilmentStructure structure = _process.Memory.Read<MHWMonsterAilmentStructure>(currentMonsterAilmentPtr + 0x148);
                    
                    monsterAilmentArrayElement += sizeof(long);
                    monsterAilmentPtr = _process.Memory.Read<long>(monsterAilmentArrayElement);
                    
                    if (structure.Owner != _address)
                        break;

                    AilmentDataSchema ailmentSchema = MonsterData.GetAilmentData(structure.Id);
                    if (ailmentSchema.IsUnknown)
                        continue;

                    MHWMonsterAilment ailment = new MHWMonsterAilment(ailmentSchema.String);

                    _ailments.Add((currentMonsterAilmentPtr, ailment));
                    this.Dispatch(OnNewAilmentFound, ailment);

                    IUpdatable<MHWMonsterAilmentStructure> updatable = ailment;
                    updatable.Update(structure);
                }

                return;
            }

            for (int i = 0; i < _ailments.Count; i++)
            {
                var (address, ailment) = _ailments[i];

                MHWMonsterAilmentStructure structure = _process.Memory.Read<MHWMonsterAilmentStructure>(address + 0x148);
                IUpdatable<MHWMonsterAilmentStructure> updatable = ailment;
                updatable.Update(structure);
            }
        }
    }
}
