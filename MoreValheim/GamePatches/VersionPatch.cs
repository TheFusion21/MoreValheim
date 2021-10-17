using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreValheim.GamePatches
{
    /// <summary>
    /// Patches GetVersionString from Internal Version class to ensure mod on Server and Client
    /// </summary>
    class VersionPatch
    {
        public static Version version;

        public static void GetVersionString(ref string __result)
        {
            __result = __result + "@" + version;
        }
    }
}
