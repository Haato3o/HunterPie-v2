#pragma once
#include "../../pch.h"
#include <unordered_map>
#include <vector>
#include <WinSock2.h>
#include <WS2tcpip.h>
#include "Models/Messages.h"
#pragma comment (lib, "ws2_32.lib")

using namespace Core::Server::Models;

namespace Core
{
    namespace Server
    {
        using IPCMessageCallback = void (*)(void*);

        class IPCService
        {
        public:
            bool Initialize();

            static IPCService* GetInstance();

            size_t SendIPCMessage(
                IPCMessage* message,
                size_t size
            );
            
            void RegisterMessageHandler(
                IPCMessageType type,
                LPVOID callback
            );

        private:
            static IPCService* m_Instance;
            std::unordered_map<IPCMessageType, std::vector<LPVOID>*> m_MessageCallbacks;
            SOCKET m_Client;
            bool m_IsInitialized;
            IPCService();

            void Listen();
            void DispatchIPCMessage(void* message);
            void RestartIPCService();
        };
    }
}