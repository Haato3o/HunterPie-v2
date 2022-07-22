#include "DamageMessageHandler.h"
#include "../../../Managers/Damage/manager.h"
#include <iterator>

using namespace Core::Server;
using namespace Core::Server::Models;
using namespace Core::Server::Handlers;
using namespace Core::Entities;

void OnRequestHuntingStatistics(RequestHuntStatisticsMessage* message)
{
    IPCService* ipcService = IPCService::GetInstance();
    auto damageTracker = HunterPie::Core::Damage::DamageTrackManager::GetInstance();
    
    auto statistics = damageTracker->GetHuntStatisticsBy(message->target);
    
    ResponseHuntStatisticsMessage response{};
    response.type = GET_HUNT_STATISTICS;
    response.version = 1;
    response.target = message->target;

    if (statistics == nullptr)
    {    
        LOG("No monsters found with address %08X", message->target);
        ipcService->SendIPCMessage(&response, sizeof(ResponseHuntStatisticsMessage));
        return;
    }

    for (int i = 0; i < std::size(response.entities); i++)
    {
        std::memcpy(&response.entities[i], &statistics->entities[i], sizeof(EntityDamageData));
    }

    ipcService->SendIPCMessage(&response, sizeof(ResponseHuntStatisticsMessage));
}

void OnRequestDeleteHuntingStatistics(RequestDeleteHuntStatisticsMessage* message)
{
    auto damageTracker = HunterPie::Core::Damage::DamageTrackManager::GetInstance();

    damageTracker->DeleteBy(message->target);
}

void OnRequestClearHuntStatisticsMessage(RequestClearHuntStatisticsMessage* message)
{
    auto damageTracker = HunterPie::Core::Damage::DamageTrackManager::GetInstance();

    damageTracker->ClearAllExcept(message->targetsToKeep, 10);
}

void DamageMessageHandler::Initialize()
{
    WITH_INSTANCE(IPCService::GetInstance())
    {
        it->RegisterMessageHandler(GET_HUNT_STATISTICS, &OnRequestHuntingStatistics);
        it->GetInstance()->RegisterMessageHandler(DELETE_HUNT_STATISTICS, &OnRequestDeleteHuntingStatistics);
        it->GetInstance()->RegisterMessageHandler(CLEAR_HUNT_STATISTICS, &OnRequestClearHuntStatisticsMessage);
    }
}

const char* DamageMessageHandler::GetName()
{
    return NAMEOF(DamageMessageHandler);
}