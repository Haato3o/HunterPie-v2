#include "IPCService.h"
#include <thread>
#define DEFAULT_ADDRESS L"127.0.0.1"
#define DEFAULT_PORT    22002
#define IPC_DEFAULT_BUFFER_SIZE 8192

using namespace Core::Server;
using namespace Core::Server::Models;

IPCService* IPCService::m_Instance;
IPCService::IPCService() {}

IPCService* IPCService::GetInstance()
{
    if (m_Instance == nullptr)
        m_Instance = new IPCService();

    return m_Instance;
}

size_t IPCService::SendIPCMessage(IPCMessage* message, size_t size)
{
    if (message->type >= UNKNOWN)
        return 0;

    send(m_Client, (char*)message, size, 0);
    
    return size;
}

void IPCService::RegisterMessageHandler(
    IPCMessageType type,
    LPVOID callback
)
{
    if (m_MessageCallbacks.find(type) == m_MessageCallbacks.end())
        m_MessageCallbacks.insert({ type, new std::vector<LPVOID>() });

    m_MessageCallbacks.at(type)->push_back(callback);
}

bool IPCService::Initialize()
{
    if (m_IsInitialized)
        return m_IsInitialized;

    WSADATA wsaData;
    WORD ver = MAKEWORD(2, 2);

    if (WSAStartup(ver, &wsaData) != 0)
    {
        LOG("WSA Error: %d", WSAGetLastError());
        return false;
    }

    SOCKET listenSocket;
    listenSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    
    if (listenSocket == INVALID_SOCKET)
    {
        LOG("Listen socket error: %d", WSAGetLastError());
        return false;
    }

    SOCKADDR_IN addrServer;
    addrServer.sin_family = AF_INET;
    addrServer.sin_port = htons(DEFAULT_PORT);
    InetPton(AF_INET, DEFAULT_ADDRESS, &addrServer.sin_addr.s_addr);

    ZeroMemory(&addrServer.sin_zero, 8);

    if (bind(listenSocket, (SOCKADDR*)&addrServer, sizeof(addrServer)) == SOCKET_ERROR)
    {
        LOG("Error binding socket: %d", WSAGetLastError());
        closesocket(listenSocket);
        WSACleanup();
        return false;
    }

    if (listen(listenSocket, 5) == SOCKET_ERROR)
    {
        LOG("Error listening: %d", WSAGetLastError());
        closesocket(listenSocket);
        WSACleanup();
        return false;
    }

    m_Client = accept(listenSocket, NULL, NULL);

    if (m_Client == INVALID_SOCKET)
    {
        LOG("Error accept socket: %d", WSAGetLastError());
        closesocket(listenSocket);
        WSACleanup();
        return false;
    }

    closesocket(listenSocket);

    m_IsInitialized = true;

    Listen();

    return m_IsInitialized;
}

void IPCService::DispatchIPCMessage(void* data)
{
    IPCMessage* message = (IPCMessage*)data;

    LOG("Received message: %d", message->type);

    if (m_MessageCallbacks.find(message->type) == m_MessageCallbacks.end())
        return;

    for (auto callbackPtr : *m_MessageCallbacks.at(message->type))
    {
        IPCMessageCallback callback = (IPCMessageCallback)callbackPtr;
        callback(data);
    }
}

void IPCService::Listen()
{
    LOG("Listening to %s:%d", "127.0.0.1", DEFAULT_PORT);

    std::thread ipcThread([this]()
    {
        char buffer[IPC_DEFAULT_BUFFER_SIZE];
        int dataSize;

        while (true)
        {
            ZeroMemory(buffer, sizeof(buffer));

            dataSize = recv(m_Client, buffer, sizeof(buffer), 0);

            if (dataSize <= 0)
                break;

            uint8_t* data = (uint8_t*)malloc(dataSize + 1);

            std::memcpy(data, buffer, dataSize);

            DispatchIPCMessage(data);

            free(data);
        }

        RestartIPCService();
    });

    ipcThread.join();
}

void IPCService::RestartIPCService()
{
    if (!m_IsInitialized)
        return;

    LOG("Lost socket connection");

    closesocket(m_Client);
    WSACleanup();

    m_IsInitialized = false;

    Initialize();
}