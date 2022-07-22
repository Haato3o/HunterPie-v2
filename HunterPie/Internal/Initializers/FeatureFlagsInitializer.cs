using HunterPie.Core.Client;
using HunterPie.Core.Domain.Features;
using HunterPie.Core.Domain.Features.Data;
using HunterPie.Core.Domain.Features.Domain;
using HunterPie.Domain.Interfaces;
using HunterPie.Features;
using System.Collections.Generic;
using System.Linq;

namespace HunterPie.Internal.Initializers
{
    internal class FeatureFlagsInitializer : IInitializer
    {
        
        public readonly static DefaultFeatureFlags Features = new();

        public void Init()
        {
            IFeatureFlagRepository localRepository = new LocalFeatureFlagRepository(Features.ReadOnlyFlags);

            HashSet<string> supportedFlags = Features.Flags.Keys.ToHashSet();

            ConfigManager.Register("internal/feature-flags.json", Features.Flags);

            string[] loadedFlags = Features.Flags.Keys.ToArray();

            foreach (string loadedFlag in loadedFlags)
                if (!supportedFlags.Contains(loadedFlag))
                    Features.Flags.Remove(loadedFlag);

            FeatureFlagManager.Initialize(localRepository);
        }
    }
}
