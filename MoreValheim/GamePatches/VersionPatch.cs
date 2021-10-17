using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreValheim.GamePatches
{
    class VersionPatch
    {
        public static Version version;

        public static void GetVersionString(ref string __result)
        {
            //We need to append out version to ensure the server and the client has the mod installed
            __result = __result + "@" + version;
        }
    }
}
