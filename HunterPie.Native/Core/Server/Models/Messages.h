#pragma once
#include "../../../pch.h"

namespace Core
{
    namespace Server
    {
        namespace Models
        {
            typedef uint32_t IPCMessageVersion;

            enum IPCMessageType
            {
                INIT_IPC_MEMORY_ADDRESSES,
                INIT_MH_HOOKS,
                GET_HUNT_STATISTICS,
                DELETE_HUNT_STATISTICS,
                CLEAR_HUNT_STATISTICS,
                UNKNOWN
            };

            struct IPCMessage
            {
                IPCMessageType type;
                IPCMessageVersion version;
            };
        }
    }
}