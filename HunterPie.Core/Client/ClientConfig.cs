﻿using HunterPie.Core.Client.Configuration;
using HunterPie.Core.Logger;

namespace HunterPie.Core.Client
{
    public class ClientConfig
    {
        public const string CONFIG_NAME = "config.json";

        private readonly Config _config = new Config();
        private static ClientConfig _instance;

        public static Config Config
        {
            get => _instance._config;
        }

        private ClientConfig()
        {
            _instance = this;
            ConfigManager.Register(CONFIG_NAME, _config);

            Log.Info("Initialized HunterPie Client configuration.");
        }

        internal static void Initialize()
        {
            if (_instance is not null)
                return; 

            _ = new ClientConfig();
        }
    }
}
