#include "../../../pch.h"
#include <iterator>
#include "manager.h"
#include <set>
#define ALL_TARGETS 0

using namespace HunterPie::Core::Damage;

DamageTrackManager* DamageTrackManager::m_Instance;
DamageTrackManager::DamageTrackManager() {}
DamageTrackManager DamageTrackManager::operator=(DamageTrackManager const &)
{
    return *this;
}

DamageTrackManager* DamageTrackManager::GetInstance()
{
    if (m_Instance == nullptr)
        m_Instance = new DamageTrackManager();

    return m_Instance;
}

void DamageTrackManager::UpdateDamage(EntityDamageData damageData)
{
    intptr_t& target = damageData.target;

    if (m_Trackings.find(target) == m_Trackings.end())
        m_Trackings.insert({ target, new HuntStatistics() });

    HuntStatistics*& statistics = m_Trackings.at(target);

    if (damageData.source.index >= std::size(statistics->entities) || damageData.source.index < 0)
        return;

    EntityDamageData& entityData = statistics->entities[damageData.source.index];
    EntityDamageData& totalEntityData = m_AllTargetsTotal.entities[damageData.source.index];

    entityData.target = damageData.target;
    memcpy(&entityData.source, &damageData.source, sizeof(Entity));
    memcpy(&totalEntityData.source, &damageData.source, sizeof(Entity));

    entityData.rawDamage += damageData.rawDamage;
    entityData.elementalDamage += damageData.elementalDamage;

    totalEntityData.rawDamage += damageData.rawDamage;
    totalEntityData.elementalDamage += damageData.elementalDamage;

    LOG("[DEBUG] Entity %d -> %08X : %f damage (total: %f)", entityData.source.index, target, entityData.rawDamage + entityData.elementalDamage, totalEntityData.rawDamage + totalEntityData.elementalDamage);
}

HuntStatistics* DamageTrackManager::GetHuntStatisticsBy(intptr_t target)
{
    if (target == ALL_TARGETS)
        return &m_AllTargetsTotal;

    if (m_Trackings.find(target) == m_Trackings.end())
        return nullptr;

    return m_Trackings.at(target);
}

void DamageTrackManager::DeleteBy(intptr_t target)
{    
    if (m_Trackings.find(target) == m_Trackings.end())
        return;

    for (auto entity : m_Trackings.at(target)->entities)
    {
        EntityDamageData& totalEntityData = m_AllTargetsTotal.entities[entity.source.index];
        totalEntityData.rawDamage -= entity.rawDamage;
        totalEntityData.elementalDamage -= entity.elementalDamage;
    }

    delete m_Trackings.at(target);
    m_Trackings.erase(target);
    LOG("Cleared monster at %08X", target);
}

void DamageTrackManager::ClearAllExcept(intptr_t* targets, size_t length)
{
    std::set<intptr_t> whitelist{};
    std::vector<intptr_t> toDelete{};

    for (int i = 0; i < length; i++)
        whitelist.insert(targets[i]);

    for (auto track : m_Trackings)
    {
        bool found = whitelist.find(track.first) != whitelist.end();

        if (!found)
            toDelete.push_back(track.first);
    }

    for (auto target : toDelete)
        DeleteBy(target);
}