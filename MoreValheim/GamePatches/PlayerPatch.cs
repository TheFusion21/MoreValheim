using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace MoreValheim.GamePatches
{
    /// <summary>
    /// Add search from MoreValheimDB
    /// </summary>
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("GetAvailableRecipes")]
    class PlayerGetAvailableRecipesPatch
    {
        static AccessTools.FieldRef<Player, HashSet<string>> m_knownRecipesRef = AccessTools.FieldRefAccess<Player, HashSet<string>>("m_knownRecipes");

        static bool Prefix(Player __instance, ref List<Recipe> available)
        {
            available.Clear();
            foreach (Recipe recipe in ObjectDB.instance.m_recipes)
            {
                if (recipe.m_enabled && (recipe.m_item.m_itemData.m_shared.m_dlc.Length <= 0 || DLCMan.instance.IsDLCInstalled(recipe.m_item.m_itemData.m_shared.m_dlc)) && ((m_knownRecipesRef(__instance).Contains(recipe.m_item.m_itemData.m_shared.m_name) || __instance.NoCostCheat()) && (__instance.RequiredCraftingStation(recipe, 1, false) || __instance.NoCostCheat())))
                    available.Add(recipe);
            }
            foreach (Recipe recipe in MoreValheimDB.instance.m_customRecipes)
            {
                if (recipe.m_enabled && (recipe.m_item.m_itemData.m_shared.m_dlc.Length <= 0 || DLCMan.instance.IsDLCInstalled(recipe.m_item.m_itemData.m_shared.m_dlc)) && ((m_knownRecipesRef(__instance).Contains(recipe.m_item.m_itemData.m_shared.m_name) || __instance.NoCostCheat()) && (__instance.RequiredCraftingStation(recipe, 1, false) || __instance.NoCostCheat())))
                    available.Add(recipe);
            }
            return false;
        }
    }

    /// <summary>
    /// Add search from MoreValheimDB
    /// </summary>
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("UpdateKnownRecipesList")]
    class PlayerUpdateKnownRecipesListPatch
    {
        static AccessTools.FieldRef<Player, HashSet<string>> m_knownRecipesRef = AccessTools.FieldRefAccess<Player, HashSet<string>>("m_knownRecipes");

        static void Prefix(Player __instance)
        {
            if (Game.instance == null)
                return;

            foreach (Recipe recipe in MoreValheimDB.instance.m_customRecipes)
            {
                if (recipe.m_enabled && !m_knownRecipesRef(__instance).Contains(recipe.m_item.m_itemData.m_shared.m_name) && __instance.HaveRequirements(recipe, true, 0))
                {
                    Debug.Log("Adding custom Recipe: " + recipe.m_item.m_itemData.m_shared.m_name);
                    __instance.AddKnownRecipe(recipe);
                }
            }

        }
    }

    /// <summary>
    /// Add custom pieces to player buildpiece list
    /// </summary>
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("SetPlaceMode")]
    class PlayerSetPlaceModePatch
    {
        static AccessTools.FieldRef<Player, PieceTable> m_buildPiecesRef = AccessTools.FieldRefAccess<Player, PieceTable>("m_buildPieces");
        static FastInvokeHandler UpdateAvailablePiecesListHandler = MethodInvoker.GetHandler(AccessTools.Method(typeof(Player), "UpdateAvailablePiecesList"));
        static AccessTools.FieldRef<Player, HashSet<string>> m_knownRecipesRef = AccessTools.FieldRefAccess<Player, HashSet<string>>("m_knownRecipes");

        static void Postfix(Player __instance)
        {
            if (m_buildPiecesRef(__instance) == null)
                return;
            foreach(var piece in MoreValheimDB.instance.m_customPieces)
            {
                if(m_buildPiecesRef(__instance).m_pieces.FindIndex(g => g.name == piece.name) == -1)
                {
                    m_buildPiecesRef(__instance).m_pieces.Add(piece);
                }
            }
            
            UpdateAvailablePiecesListHandler.Invoke(__instance);
        }
    }
}
