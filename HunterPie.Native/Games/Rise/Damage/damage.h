#pragma once
#include "../../../pch.h"
#include "../Common/Monster.h"

using namespace Games::Rise::Common;

namespace Games
{
    namespace Rise
    {
        namespace Damage
        {
            struct MHREntityData
            {
                intptr_t unk0;
                int32_t unk1;
                int32_t unk2;
                int32_t unk3;
                float rawDamage;
                float elementalDamage;
                int unk4;
                float unk5;
                float unk6;
                float unk7;
                float unk8;
                float unk9;
                float unk10;
                float unk11;
                float unk12;
                uint8_t padding[112]; // A lot of values, but we gonna keep an array here since I don't want to map them all
                int attackerDamageType;
                int unk14;
                int unk15;
                int Id;
            };

            using fnCalculateEntityDamage = MHREntityData* (*)(
                intptr_t arg1,
                Monster* target,
                intptr_t arg3,
                intptr_t arg4,
                intptr_t arg5,
                void* arg6
            );

            static MHREntityData* (*ogCalculateEntityDamage)(
                intptr_t arg1,
                Monster* target,
                intptr_t arg3,
                intptr_t arg4,
                intptr_t arg5,
                void* arg6
            );
        }
    }
}