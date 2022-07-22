#pragma once
#include "../../pch.h"

namespace Core
{
    class IInitializable
    {
    public:
        virtual bool Init(uintptr_t* pointers) { return false; };
    };
}