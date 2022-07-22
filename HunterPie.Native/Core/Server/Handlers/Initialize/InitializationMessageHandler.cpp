#include "InitializationMessageHandler.h"
#include "../../../../Games/Rise/Damage/hooks.h"
#include "../../../../libs/MinHook/MinHook.h"

using namespace Core::Server;
using namespace Core::Server::Models;
using namespace Core::Server::Handlers;
using namespace Games::Rise;

void OnRequestIPCInitialization(RequestIPCInitializationMessage* message)
{
    MH_Initialize();
    Damage::Hook::DamageHooks().Init(message->addresses);

    IPCMessage response{
        INIT_IPC_MEMORY_ADDRESSES,
        2
    };

    IPCService::GetInstance()->SendIPCMessage(&response, sizeof(IPCMessage));
}

void OnRequestInitMHHooks(IPCMessage* message)
{
    MH_STATUS status = MH_EnableHook(MH_ALL_HOOKS);

    ResponseInitMHHooksMessage response{};

    WITH(response)
    {
        it.type = INIT_MH_HOOKS;
        it.version = 1;
        it.status = (int)status;
    }

    IPCService::GetInstance()->SendIPCMessage(&response, sizeof(ResponseInitMHHooksMessage));
}

void InitializationMessageHandler::Initialize()
{
    WITH_INSTANCE(IPCService::GetInstance())
    {
        it->RegisterMessageHandler(INIT_IPC_MEMORY_ADDRESSES, &OnRequestIPCInitialization);
        it->RegisterMessageHandler(INIT_MH_HOOKS, &OnRequestInitMHHooks);
    }
}

const char* InitializationMessageHandler::GetName()
{
    return NAMEOF(InitializationMessageHandler);
}