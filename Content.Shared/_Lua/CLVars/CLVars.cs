// LuaWorld - This file is licensed under AGPLv3
// Copyright (c) 2025 LuaWorld
// See AGPLv3.txt for details.

using Robust.Shared.Configuration;

namespace Content.Shared.Lua.CLVar
{
    [CVarDefs]
    public sealed partial class CLVars
    {
        public static readonly CVarDef<bool> NetDynamicTick =
            CVarDef.Create("net.dynamictick", false, CVar.ARCHIVE | CVar.SERVER | CVar.REPLICATED);
        public static readonly CVarDef<int> NetDynamicTickMinTickrate =
            CVarDef.Create("net.min_tickrate", 20, CVar.SERVERONLY | CVar.ARCHIVE);
        public static readonly CVarDef<int> NetDynamicTickMaxTickrate =
            CVarDef.Create("net.max_tickrate", 50, CVar.SERVERONLY | CVar.ARCHIVE);
        public static readonly CVarDef<float> NetDynamicTickCheckInterval =
            CVarDef.Create("net.check_interval", 1f, CVar.SERVERONLY | CVar.ARCHIVE);
        public static readonly CVarDef<float> NetDynamicTickLowFpsMin =
            CVarDef.Create("net.low_fps_min", 4f, CVar.SERVERONLY | CVar.ARCHIVE);
        public static readonly CVarDef<float> NetDynamicTickLowFpsMax =
            CVarDef.Create("net.low_fps_max", 20f, CVar.SERVERONLY | CVar.ARCHIVE);
        public static readonly CVarDef<float> NetDynamicTickDecreaseDelay =
            CVarDef.Create("net.decrease_delay", 15f, CVar.SERVERONLY | CVar.ARCHIVE);
        public static readonly CVarDef<float> NetDynamicTickIncreaseDelay =
            CVarDef.Create("net.increase_delay", 1200f, CVar.SERVERONLY | CVar.ARCHIVE);
    }
}
