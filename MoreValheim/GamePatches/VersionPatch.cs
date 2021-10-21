using System;
using HarmonyLib;

namespace MoreValheim.GamePatches
{
    /// <summary>
    /// Patches GetVersionString from Internal Version class to ensure mod on Server and Client
    /// </summary>
    class VersionPatch
    {
        public static Version version;

        [HarmonyAfter(new string[] { "org.bepinex.plugins.valheim_plus" })]
        public static void GetVersionString(ref string __result)
        {
            __result = __result + "@" + version;
        }
    }
}
