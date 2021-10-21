using HarmonyLib;
using MoreValheim.MonoBehaviours;


namespace MoreValheim.GamePatches
{
    /// <summary>
    /// Adds custom item to ZNetScene to make them save persistent
    /// </summary>
    [HarmonyPatch(typeof(ZNetScene))]
    [HarmonyPatch("Awake")]
    class ZNetSceneAwakePatch
    {
        static void Prefix(ZNetScene __instance)
        {
            if (!MoreValheimDB.instance.loaded)
                MoreValheimDB.instance.LoadAssets();
            __instance.m_prefabs.AddRange(MoreValheimDB.instance.m_customItems);
            __instance.m_prefabs.AddRange(MoreValheimDB.instance.m_customPieces);
        }
    }
}
