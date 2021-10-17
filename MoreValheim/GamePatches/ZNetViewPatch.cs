using HarmonyLib;


namespace MoreValheim.GamePatches
{
    class ZNetViewPatch
    {
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
}
