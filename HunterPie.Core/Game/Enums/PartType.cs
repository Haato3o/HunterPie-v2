﻿using System;

namespace HunterPie.Core.Game.Enums
{
    [Flags]
    public enum PartType : int
    {
        Flinch = 1 << 1,
        Breakable = 1 << 2,
        Severable = 1 << 3
    }
}
