using HunterPie.Core.Address.Map;
using HunterPie.Core.Domain;
using HunterPie.Core.Domain.DTO;
using HunterPie.Core.Domain.DTO.Monster;
using HunterPie.Core.Domain.Interfaces;
using HunterPie.Core.Domain.Process;
using HunterPie.Core.Extensions;
using HunterPie.Core.Game.Data;
using HunterPie.Core.Game.Enums;
using HunterPie.Core.Game.Environment;
using HunterPie.Core.Game.Rise.Definitions;
using HunterPie.Core.Game.Rise.Entities;
using HunterPie.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace HunterPie.Core.Game.Demos.Sunbreak.Entities.Monster
{
    public class MHRSunbreakDemoMonster : Scannable, IMonster, IEventDispatcher
    {
        private readonly long _address;

        private int _id = -1;
        private float _health = -1;
        private bool _isTarget;
        private bool _isEnraged;
        private Target _target;
        private Crown _crown;
        private float _stamina;
        private MHRMonsterAilment _enrage = new MHRMonsterAilment("STATUS_ENRAGE");
        private readonly Dictionary<long, MHRMonsterPart> parts = new();
        private readonly Dictionary<long, MHRMonsterAilment> ailments = new();
        private readonly List<Element> _weaknesses = new();

        public int Id
        {
            get => _id;
            private set
            {
                if (_id != value)
                {
                    _id = value;
                    GetMonsterWeaknesses();

                    this.Dispatch(OnSpawn);
                }
            }
        }

        public string Name => MHRSunbreakDemoContext.Strings.GetMonsterNameById(Id);

        public float Health
        {
            get => _health;
            private set
            {
                if (_health != value)
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

        public IMonsterPart[] Parts => parts.Values.ToArray();
        public IMonsterAilment[] Ailments => ailments.Values.ToArray();

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

        public Element[] Weaknesses => _weaknesses.ToArray();

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
        public event EventHandler<EventArgs> OnEnrageStateChange;
        public event EventHandler<EventArgs> OnTargetChange;
        public event EventHandler<IMonsterPart> OnNewPartFound;
        public event EventHandler<IMonsterAilment> OnNewAilmentFound;
        public event EventHandler<Element[]> OnWeaknessesChange;
        public event EventHandler<IMonster> OnCaptureThresholdChange;

        public MHRSunbreakDemoMonster(IProcessManager process, long address) : base(process)
        {
            _address = address;

            Log.Debug($"Initialized monster at address {address:X}");
        }

        private void GetMonsterWeaknesses()
        {
            var data = MonsterData.GetMonsterData(Id);

            if (data.HasValue)
            {
                _weaknesses.AddRange(data.Value.Weaknesses);
                this.Dispatch(OnWeaknessesChange, Weaknesses);
            }
        }

        [ScannableMethod(typeof(MonsterInformationData))]
        private void GetMonsterBasicInformation()
        {
            MonsterInformationData dto = new();

            int monsterId = _process.Memory.Read<int>(_address + 0x2B4);

            dto.Id = monsterId;

            Next(ref dto);
            
            Id = dto.Id;
        }

        [ScannableMethod(typeof(HealthData))]
        private void GetMonsterHealthData()
        {
            HealthData dto = new();

            long healthComponent = _process.Memory.ReadPtr(_address, AddressMap.Get<int[]>("MONSTER_HEALTH_COMPONENT_OFFSETS"));
            long healthPtr = _process.Memory.ReadPtr(healthComponent, AddressMap.Get<int[]>("MONSTER_HEALTH_COMPONENT_ENCODED_OFFSETS"));

            float currentHealth = _process.Memory.Read<float>(healthPtr + 0x18);

            dto.Health = currentHealth;
            dto.MaxHealth = _process.Memory.Read<float>(healthComponent + 0x18);

            Next(ref dto);

            MaxHealth = dto.MaxHealth;
            Health = dto.Health;
        }

        [ScannableMethod]
        private void ScanLockon()
        {

            int cameraStyleType = _process.Memory.Deref<int>(
                AddressMap.GetAbsolute("LOCKON_ADDRESS"),
                AddressMap.Get<int[]>("LOCKON_CAMERA_STYLE_OFFSETS")
            );

            long cameraStylePtr = _process.Memory.Read(
                AddressMap.GetAbsolute("LOCKON_ADDRESS"),
                AddressMap.Get<int[]>("LOCKON_OFFSETS")
            );

            cameraStylePtr += cameraStyleType * 8;

            long monsterAddress = _process.Memory.Deref<long>(
                    cameraStylePtr,
                    new[] { 0x78 }
            );

            IsTarget = monsterAddress == _address;

            if (IsTarget)
                Target = Target.Self;
            else if (monsterAddress != 0)
                Target = Target.Another;
            else
                Target = Target.None;
        }

        [ScannableMethod(typeof(MHRStaminaStructure))]
        private void GetMonsterStamina()

        {
            long staminaPtr = _process.Memory.ReadPtr(_address, AddressMap.Get<int[]>("MONSTER_STAMINA_OFFSETS"));

            MHRStaminaStructure structure = _process.Memory.Read<MHRStaminaStructure>(staminaPtr);

            MaxStamina = structure.MaxStamina;
            Stamina = structure.Stamina;
        }

        [ScannableMethod]
        private void GetMonsterParts()
        {
            // Flinch array
            long monsterFlinchPartsPtr = _process.Memory.ReadPtr(_address, AddressMap.Get<int[]>("MONSTER_FLINCH_HEALTH_COMPONENT_OFFSETS"));
            uint monsterFlinchPartsArrayLength = _process.Memory.Read<uint>(monsterFlinchPartsPtr + 0x1C);

            // Breakable array
            long monsterBreakPartsArrayPtr = _process.Memory.ReadPtr(_address, AddressMap.Get<int[]>("MONSTER_BREAK_HEALTH_COMPONENT_OFFSETS"));
            uint monsterBreakPartsArrayLength = _process.Memory.Read<uint>(monsterBreakPartsArrayPtr + 0x1C);

            // Severable array
            long monsterSeverPartsArrayPtr = _process.Memory.ReadPtr(_address, AddressMap.Get<int[]>("MONSTER_SEVER_HEALTH_COMPONENT_OFFSETS"));
            uint monsterSeverPartsArrayLenght = _process.Memory.Read<uint>(monsterSeverPartsArrayPtr + 0x1C);

            if (monsterFlinchPartsArrayLength == monsterBreakPartsArrayLength
                && monsterFlinchPartsArrayLength == monsterSeverPartsArrayLenght)
            {
                long[] monsterFlinchArray = _process.Memory.Read<long>(monsterFlinchPartsPtr + 0x20, monsterFlinchPartsArrayLength);
                long[] monsterBreakArray = _process.Memory.Read<long>(monsterBreakPartsArrayPtr + 0x20, monsterBreakPartsArrayLength);
                long[] monsterSeverArray = _process.Memory.Read<long>(monsterSeverPartsArrayPtr + 0x20, monsterSeverPartsArrayLenght);

                DerefPartsAndScan(monsterFlinchArray, monsterBreakArray, monsterSeverArray);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DerefPartsAndScan(long[] flinchPointers, long[] partsPointers, long[] severablePointers)
        {
            for (int i = 0; i < flinchPointers.Length; i++)
            {

                long flinchPart = flinchPointers[i];
                long breakablePart = partsPointers[i];
                long severablePart = severablePointers[i];

                MHRPartStructure partInfo = new()
                {
                    MaxHealth = _process.Memory.Read<float>(breakablePart + 0x18),
                    MaxFlinch = _process.Memory.Read<float>(flinchPart + 0x18),
                    MaxSever = _process.Memory.Read<float>(severablePart + 0x18)
                };

                // Skip invalid parts
                if (partInfo.MaxFlinch < 0 && partInfo.MaxHealth < 0 && partInfo.MaxSever < 0)
                    continue;

                // TODO: Read all this in 1 pass
                long encodedFlinchHealthPtr = _process.Memory.ReadPtr(flinchPart, AddressMap.Get<int[]>("MONSTER_HEALTH_COMPONENT_ENCODED_OFFSETS"));
                long encodedBreakableHealthPtr = _process.Memory.ReadPtr(breakablePart, AddressMap.Get<int[]>("MONSTER_HEALTH_COMPONENT_ENCODED_OFFSETS"));
                long encodedSeverableHealthPtr = _process.Memory.ReadPtr(severablePart, AddressMap.Get<int[]>("MONSTER_HEALTH_COMPONENT_ENCODED_OFFSETS"));

                // Flinch values
                partInfo.Flinch = _process.Memory.Read<float>(encodedFlinchHealthPtr + 0x18);

                // Break values
                partInfo.Health = _process.Memory.Read<float>(encodedBreakableHealthPtr + 0x18);

                // Sever values
                partInfo.Sever = _process.Memory.Read<float>(encodedSeverableHealthPtr + 0x18);

                if (!parts.ContainsKey(flinchPart))
                {
                    string partName = MonsterData.GetMonsterPartData(Id, i)?.String ?? "PART_UNKNOWN";
                    var dummy = new MHRMonsterPart(partName, partInfo);
                    parts.Add(flinchPart, dummy);

                    Log.Debug($"Found {partName} for {Name} -> Flinch: {flinchPart:X} Break: {breakablePart:X} Sever: {severablePart:X}");
                    this.Dispatch(OnNewPartFound, dummy);
                }

                IUpdatable<MHRPartStructure> monsterPart = parts[flinchPart];
                monsterPart.Update(partInfo);
            }
        }
    }
}
