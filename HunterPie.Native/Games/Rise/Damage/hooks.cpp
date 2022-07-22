#include "hooks.h"
#include "../../../libs/MinHook/MinHook.h"
#include "../../../Core/Managers/Damage/manager.h"
#include "../../../Core/Utils/addresses.h"
#include "../Utils/RiseUtils.h"

using namespace Games::Rise::Damage;
using namespace HunterPie::Core::Damage;
using namespace Games::Rise::Damage::Hook;

uintptr_t fnCalculateEntityDamagePtr = (uintptr_t)nullptr;

EntityType GetEntityByDamageType(int damageType);
bool HookFunctions();

MHREntityData* Hook::CalculateEntityDamage(
    intptr_t arg1,
    Monster* target,
    intptr_t arg3,
    intptr_t arg4,
    intptr_t arg5,
    void* arg6
)
{
    MHREntityData* damageData = ogCalculateEntityDamage(
        arg1, 
        target, 
        arg3, 
        arg4, 
        arg5, 
        arg6
    );

    if (!Utils::IsBigMonster(target->id))
        return damageData;

    LOG("Hit monster id: %d", target->id);

    Entity entity = Entity{
        damageData->Id,
        GetEntityByDamageType(damageData->attackerDamageType)
    };

    // Each player's main pet will inherits the owner's index, but be flagged as a PET
    if (entity.type == PET && entity.index <= 3)
        entity.index = entity.index + 4 + 1;

    EntityDamageData entityData = EntityDamageData{
        (intptr_t)target,
        entity,
        damageData->rawDamage,
        damageData->elementalDamage,
    };

    DamageTrackManager::GetInstance()->UpdateDamage(entityData);

    return damageData;
}

bool DamageHooks::Init(uintptr_t* pointers)
{
    fnCalculateEntityDamagePtr = pointers[FUN_CALCULATE_ENTITY_DAMAGE];

    LOG("Added trampoline to function %016X", fnCalculateEntityDamagePtr);

    return HookFunctions();
}

bool HookFunctions()
{
    MH_STATUS status = MH_CreateHook(
        (fnCalculateEntityDamage)fnCalculateEntityDamagePtr,
        &CalculateEntityDamage,
        reinterpret_cast<LPVOID*>(&ogCalculateEntityDamage)
    );

    LOG("%s status: %s", NAMEOF(fnCalculateEntityDamage), MH_StatusToString(status));

    return status == MH_OK;
}

EntityType GetEntityByDamageType(int damageType)
{
    switch (damageType)
    {
        case 0:
            return PLAYER;
        case 0x15:
        case 0x16:
        case 0x17:
            return PET;
        default:
            return UNKNOWN;
    }
}