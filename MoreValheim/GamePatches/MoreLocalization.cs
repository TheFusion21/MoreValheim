using HarmonyLib;
using System.Collections.Generic;
using System.IO;

namespace MoreValheim.GamePatches
{
    [HarmonyPatch(typeof(Localization))]
    [HarmonyPatch("SetupLanguage")]
    class MoreLocalization
    {
        static FastInvokeHandler StripCitationsHandler = MethodInvoker.GetHandler(AccessTools.Method(typeof(Localization), "StripCitations"));
        static FastInvokeHandler DoQuoteLineSplitHandler = MethodInvoker.GetHandler(AccessTools.Method(typeof(Localization), "DoQuoteLineSplit"));
        static FastInvokeHandler AddWordHandler = MethodInvoker.GetHandler(AccessTools.Method(typeof(Localization), "AddWord"));

        static void Postfix(Localization __instance, ref bool __result, string language)
        {
            StringReader reader = new StringReader(Properties.Resources.morelocalization);
            string[] strArray = reader.ReadLine().Split(',');
            int index1 = -1;
            for (int index2 = 0; index2 < strArray.Length; ++index2)
            {
                if (((string)StripCitationsHandler.Invoke(__instance, strArray[index2])) == language)
                {
                    index1 = index2;
                    break;
                }
            }
            if (index1 == -1)
            {
                ZLog.LogWarning((object)("Failed to find language:" + language));
                return;
            }
            foreach (List<string> stringList in (List<List<string>>)DoQuoteLineSplitHandler.Invoke(__instance, reader))
            {
                if (stringList.Count != 0)
                {
                    string key = stringList[0];
                    if (!key.StartsWith("//") && key.Length != 0 && stringList.Count > index1)
                    {
                        string text = stringList[index1];
                        if (string.IsNullOrEmpty(text) || text[0] == '\r')
                            text = stringList[1];
                        AddWordHandler.Invoke(__instance, key, text);
                    }
                }
            }
            ZLog.Log((object)("Loaded morevalheim localization for language: " + language));
        }
    }
}
