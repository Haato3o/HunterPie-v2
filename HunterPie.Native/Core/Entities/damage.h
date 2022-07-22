#pragma once
#include <string>
#include "weapon.h"

namespace Core
{
    namespace Entities
    {
        enum EntityType
        {
            PLAYER,
            NPC,
            PET,
            COMPANION,
            ENVIRONMENT,
            MONSTER,
            UNKNOWN
        };

        struct Entity
        {
            int index;
            EntityType type;
        };

        struct EntityDamageData
        {
            intptr_t target;
            Entity source;
            float rawDamage;
            float elementalDamage;
        };
    }
}