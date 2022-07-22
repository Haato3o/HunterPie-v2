#pragma once
#include <vector>
#include <string>

namespace Core
{
    namespace Server
    {
        namespace Handlers
        {
            class MessageHandler
            {
            public:
                virtual void Initialize() {};
                virtual const char* GetName() { return ""; };
                static std::vector<MessageHandler*> GetAvailableHandlers();
            };
        }
    }
}