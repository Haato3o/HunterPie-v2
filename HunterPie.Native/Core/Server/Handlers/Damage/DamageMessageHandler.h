#pragma once
#include "../../../Entities/damage.h"
#include "../../Models/Messages.h"
#include "../../IPCService.h"
#include "../MessageHandler.h"

namespace Core
{
    namespace Server
    {
        namespace Models
        {
            struct RequestHuntStatisticsMessage : IPCMessage
            {
                intptr_t target;
            };

            struct ResponseHuntStatisticsMessage : IPCMessage
            {
                intptr_t target;
                Entities::EntityDamageData entities[10];
            };

            struct RequestDeleteHuntStatisticsMessage : IPCMessage
            {
                intptr_t target;
            };

            struct RequestClearHuntStatisticsMessage : IPCMessage
            {
                intptr_t targetsToKeep[10];
            };
        }

        namespace Handlers
        {
            class DamageMessageHandler : public MessageHandler
            {
            public:
                virtual void Initialize();
                virtual const char* GetName();
            };
        }
    }
}