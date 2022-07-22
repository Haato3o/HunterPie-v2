using HunterPie.Core.Client.Configuration.Overlay;
using HunterPie.Core.Game.Enums;
using HunterPie.UI.Architecture.Test;
using System;

namespace HunterPie.UI.Overlay.Widgets.Monster.ViewModels
{
    internal class MockBossMonsterViewModel : BossMonsterViewModel
    {
        private readonly MonsterWidgetConfig _mockConfig;

        public MockBossMonsterViewModel(MonsterWidgetConfig config) : base(config)
        {
            _mockConfig = config;
            InitMock();
        }

        private void InitMock()
        {
            MockParts();
            MockAilments();
            MockWeakenesses();

            MockBehavior.Run(() =>
            {
                for (int i = 0; i < Parts.Count / 2; i++)
                {
                    var part = Parts[i];

                    if (!part.IsPartBroken)
                        part.Health -= Math.Min(20, part.MaxHealth);

                    part.Flinch -= Math.Min(20, part.MaxFlinch);

                    if (!part.IsPartSevered)
                        part.Sever -= Math.Min(150, part.MaxSever);

                    if (part.Sever < 0)
                    {
                        part.Sever = part.MaxSever;
                        part.IsPartSevered = true;
                    }

                    if (part.Health < 0)
                    {
                        part.Health = part.MaxHealth;
                        part.IsPartBroken = true;
                    }

                    if (part.Flinch < 0)
                        part.Flinch = part.MaxFlinch;
                }
            });
        }

        private void MockParts()
        {
            for (int i = 0; i < 2; i++)
            {
                Parts.Add(
                    new MonsterPartViewModel(_mockConfig)
                    {
                        Name = $"Severable {i}",
                        IsKnownPart = true,
                        Health = 500.0,
                        MaxHealth = 500.0,
                        Sever = 2000.0,
                        MaxSever = 2000.0,
                        Flinch = 200.0,
                        MaxFlinch = 200.0,
                        Breaks = 0,
                        MaxBreaks = 0,
                        IsPartBroken = (i & 1) == 1,
                        Type = PartType.Severable
                    }
                );
            }

            for (int i = 0; i < 2; i++)
            {
                Parts.Add(
                    new MonsterPartViewModel(_mockConfig)
                    {
                        Name = $"Breakable {i}",
                        IsKnownPart = true,
                        Health = 500.0,
                        MaxHealth = 500.0,
                        Flinch = 250.0,
                        MaxFlinch = 250.0,
                        Breaks = 0,
                        MaxBreaks = 0,
                        IsPartBroken = (i & 1) == 1,
                        Type = PartType.Breakable
                    }
                );
            }

            for (int i = 0; i < 10; i++)
            {
                Parts.Add(
                    new MonsterPartViewModel(_mockConfig)
                    {
                        Name = $"Part {i}",
                        IsKnownPart = true,
                        Flinch = 250.0,
                        MaxFlinch = 250.0,
                        Breaks = 0,
                        MaxBreaks = 0,
                        Type = PartType.Flinch
                    }
                );
            }
        }

        private void MockAilments()
        {
            string[] ailmentNames =
            {
                "AILMENT_UNKNOWN", "AILMENT_PARALYSIS", "AILMENT_SLEEP", "AILMENT_STUN", "AILMENT_POISON",
                "AILMENT_BLAST", "AILMENT_EXHAUST", "AILMENT_MOUNT", "AILMENT_FLASH", "AILMENT_WATER", "AILMENT_FIRE",
                "AILMENT_ICE", "AILMENT_THUNDER", "AILMENT_DUNG", "STATUS_ENRAGE"
            };
            foreach (string name in ailmentNames)
            {
                Ailments.Add(
                    new MonsterAilmentViewModel(_mockConfig)
                    {
                        Name = name,
                        Timer = 100.0,
                        MaxTimer = 100.0,
                        Buildup = 100.0,
                        MaxBuildup = 100.0
                    }
                );
            }
        }

        private void MockWeakenesses()
        {
            Random random = new();

            for (int i = 0; i < 2; i++)
            {
                int maxElements = Enum.GetValues(typeof(Element)).Length - 1;
                Element randomElement = (Element)random.Next(maxElements);
                Weaknesses.Add(randomElement);
            }
        }
    }
}
