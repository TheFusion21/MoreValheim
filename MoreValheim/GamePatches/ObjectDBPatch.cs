using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace MoreValheim.GamePatches
{
    [HarmonyPatch(typeof(ObjectDB))]
    [HarmonyPatch("UpdateItemHashes")]
    class ObjectDBUpdateItemHashesPatch
    {
        static void Prefix(ObjectDB __instance)
        {
            if (!MoreValheimDB.instance.loaded)
                MoreValheimDB.instance.LoadAssets();
            MoreValheimDB.instance.UpdateItemHashes();
        }
    }

    [HarmonyPatch(typeof(ObjectDB))]
    [HarmonyPatch("GetItemPrefab")]
    [HarmonyPatch(new System.Type[] { typeof(string) })]
    class ObjectDBGetItemPrefabPatch1
    {
        static void Postfix(ObjectDB __instance, ref GameObject __result, string name)
        {
            if (__result != null)
                return;
            //Search for custom item
            foreach (GameObject gameObject in MoreValheimDB.instance.m_customItems)
            {
                if (gameObject.name == name)
                {
                    __result = gameObject;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ObjectDB))]
    [HarmonyPatch("GetItemPrefab")]
    [HarmonyPatch(new System.Type[] { typeof(int) })]
    class ObjectDBGetItemPrefabPatch2
    {
        static void Postfix(ObjectDB __instance, ref GameObject __result, int hash)
        {
            if (__result != null)
                return;
            GameObject gameObject;
            if(MoreValheimDB.instance.m_customItemByHash.TryGetValue(hash, out gameObject))
            {
                //Search for custom item
                __result = gameObject;
            }
        }
    }

    [HarmonyPatch(typeof(ObjectDB))]
    [HarmonyPatch("GetStatusEffect")]
    class ObjectDBGetStatusEffectPatch
    {
        static void Postfix(ObjectDB __instance, ref StatusEffect __result, string name)
        {
            if (__result != null)
                return;
            foreach (StatusEffect statusEffect in MoreValheimDB.instance.m_customStatusEffects)
            {
                if (statusEffect.name == name)
                {
                    return;

                }
            }
            __result = null;
        }
    }

    [HarmonyPatch(typeof(ObjectDB))]
    [HarmonyPatch("GetAllItems")]
    class ObjectDBGetAllItemsPatch
    {
        static void Postfix(ObjectDB __instance, ref List<ItemDrop> __result, ItemDrop.ItemData.ItemType type, string startWith)
        {
            foreach (GameObject gameObject in MoreValheimDB.instance.m_customItems)
            {
                ItemDrop component = gameObject.GetComponent<ItemDrop>();
                if (component.m_itemData.m_shared.m_itemType == type && component.gameObject.name.StartsWith(startWith))
                    __result.Add(component);
            }
        }
    }

    [HarmonyPatch(typeof(ObjectDB))]
    [HarmonyPatch("GetRecipe")]
    class ObjectDBGetRecipePatch
    {
        static void Postfix(ObjectDB __instance, ref Recipe __result, ItemDrop.ItemData item)
        {
            if (__result != null)
                return;
            foreach (Recipe recipe in MoreValheimDB.instance.m_customRecipes)
            {
                Debug.Log(recipe.m_item.m_itemData.m_shared.m_name);
                Debug.Log(item.m_shared.m_name);
                if (recipe.m_item != null && recipe.m_item.m_itemData.m_shared.m_name == item.m_shared.m_name)
                {
                    __result = recipe;
                    return;
                }
            }
        }
    }
}
