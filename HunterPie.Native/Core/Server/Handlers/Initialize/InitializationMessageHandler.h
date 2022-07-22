#pragma once
#include "../MessageHandler.h"
#include "../../IPCService.h"
#include "../../Models/Messages.h"

namespace Core
{
    namespace Server
    {
        namespace Models
        {
            struct RequestIPCInitializationMessage : IPCMessage
            {
                uintptr_t addresses[256];
            };

            struct ResponseInitMHHooksMessage : IPCMessage
            {
                int status;
            };
        }

        namespace Handlers
        {
            class InitializationMessageHandler : public MessageHandler
            {
            public:
                virtual void Initialize();
                virtual const char* GetName();
            };
        }
    }
}