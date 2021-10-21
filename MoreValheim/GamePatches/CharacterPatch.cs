using HarmonyLib;

namespace MoreValheim.GamePatches
{
    /// <summary>
    /// This patch ensures all mobs spawned have at least one star
    /// </summary>
    [HarmonyPatch(typeof(Character))]
    [HarmonyPatch("Awake")]
    class CharacterPatch
    {
        static void Prefix(Character __instance)
        {
            if(!__instance.IsPlayer())
            {
                ZNetView nview = __instance.GetComponent<ZNetView>();
                if (nview == null)
                    return;

                if(nview.GetZDO().GetInt("level", 1) <= 1)
                {
                    nview.GetZDO().Set("level", 2);
                }
            }
        }
    }
}
