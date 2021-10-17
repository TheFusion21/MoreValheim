using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Collections;
using System;

namespace MoreValheim.GamePatches
{
    /*
    [HarmonyPatch(typeof(Hud))]
    [HarmonyPatch("UpdatePieceList")]
    class HudPatch
    {
        static AccessTools.FieldRef<Hud, IList> m_pieceIconsRef = AccessTools.FieldRefAccess<Hud, IList>("m_pieceIcons");
        static AccessTools.FieldRef<Hud, GameObject> m_pieceIconPrefabRef = AccessTools.FieldRefAccess<Hud, GameObject>("m_pieceIconPrefab");
        static AccessTools.FieldRef<Hud, RectTransform> m_pieceListRootRef = AccessTools.FieldRefAccess<Hud, RectTransform>("m_pieceListRoot");
        static AccessTools.FieldRef<Hud, float> m_pieceIconSpacingRef = AccessTools.FieldRefAccess<Hud, float>("m_pieceIconSpacing");
        static AccessTools.FieldRef<Hud, Piece.PieceCategory> m_lastPieceCategoryRef = AccessTools.FieldRefAccess<Hud, Piece.PieceCategory>("m_lastPieceCategory");
        static AccessTools.FieldRef<Hud, float> m_pieceBarPosXRef = AccessTools.FieldRefAccess<Hud, float>("m_pieceBarPosX");
        static AccessTools.FieldRef<Hud, float> m_pieceBarTargetPosXRef = AccessTools.FieldRefAccess<Hud, float>("m_pieceBarTargetPosX");

        static FastInvokeHandler OnLeftClickPieceHandler = MethodInvoker.GetHandler(AccessTools.Method(typeof(Hud), "OnLeftClickPiece"));
        static FastInvokeHandler OnRightClickPieceHandler = MethodInvoker.GetHandler(AccessTools.Method(typeof(Hud), "OnRightClickPiece"));
        static FastInvokeHandler OnHoverPieceHandler = MethodInvoker.GetHandler(AccessTools.Method(typeof(Hud), "OnHoverPiece"));
        static FastInvokeHandler OnHoverPieceExitHandler = MethodInvoker.GetHandler(AccessTools.Method(typeof(Hud), "OnHoverPieceExit"));

        static FastInvokeHandler UpdatePieceBuildStatusHandler = MethodInvoker.GetHandler(AccessTools.Method(typeof(Hud), "UpdatePieceBuildStatus"));
        static FastInvokeHandler UpdatePieceBuildStatusAllHandler = MethodInvoker.GetHandler(AccessTools.Method(typeof(Hud), "UpdatePieceBuildStatusAll"));

        static Hud HudInstance = null;
        static int[] pageOffsets = new int[(int)Piece.PieceCategory.Max];
        static bool Prefix(Hud __instance, Player player, Vector2Int selectedNr, Piece.PieceCategory category, bool updateAllBuildStatuses)
        {
            if(__instance != HudInstance)
            {
                HudInstance = __instance;
            }
            var HudType = AccessTools.TypeByName("Hud");
            var pieceDataType = AccessTools.Inner(HudType, "PieceIconData");
            var m_goField = pieceDataType.GetField("m_go");
            var m_iconField = pieceDataType.GetField("m_icon");
            var m_markerField = pieceDataType.GetField("m_marker");
            var m_upgradeField = pieceDataType.GetField("m_upgrade");
            var m_tooltipField = pieceDataType.GetField("m_tooltip");
            

            IList m_pieceIcons = (IList)HudType.GetField("m_pieceIcons", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            List<Piece> buildPieces = player.GetBuildPieces();
            int num1 = 13;
            int num2 = 7;
            if (buildPieces.Count <= 1)
            {
                num1 = 1;
                num2 = 1;
            }
            if (m_pieceIcons.Count != num1 * num2)
            {
                foreach (object pieceIcon in m_pieceIcons)
                {
                    var m_go = m_goField.GetValue(pieceIcon);
                    UnityEngine.Object.Destroy((GameObject)m_go);
                }
                m_pieceIcons.Clear();
                for (int index1 = 0; index1 < num2; ++index1)
                {
                    for (int index2 = 0; index2 < num1; ++index2)
                    {
                        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(m_pieceIconPrefabRef(__instance), (Transform)m_pieceListRootRef(__instance));
                        (gameObject.transform as RectTransform).anchoredPosition = new Vector2((float)index2 * m_pieceIconSpacingRef(__instance), (float)-index1 * m_pieceIconSpacingRef(__instance));
                        object pieceIconData = AccessTools.CreateInstance(pieceDataType);
                        gameObject.transform.Find("icon").GetComponent<Image>().color = new Color(1f, 0.0f, 1f, 0.0f);
                        m_goField.SetValue(pieceIconData, gameObject);
                        m_tooltipField.SetValue(pieceIconData, gameObject.GetComponent<UITooltip>());
                        m_iconField.SetValue(pieceIconData, gameObject.transform.Find("icon").GetComponent<Image>());
                        m_markerField.SetValue(pieceIconData, gameObject.transform.Find("selected").gameObject);
                        m_upgradeField.SetValue(pieceIconData, gameObject.transform.Find("upgrade").gameObject);

                        UIInputHandler component = gameObject.GetComponent<UIInputHandler>();
                        component.m_onLeftDown += new Action<UIInputHandler>((UIInputHandler ih) =>
                        {
                            OnLeftClickPieceHandler.Invoke(__instance, ih);
                        });
                        component.m_onRightDown += new Action<UIInputHandler>((UIInputHandler ih) =>
                        {
                            OnRightClickPieceHandler.Invoke(__instance, ih);
                        });
                        component.m_onPointerEnter += new Action<UIInputHandler>((UIInputHandler ih) =>
                        {
                            OnHoverPieceHandler.Invoke(__instance, ih);
                        });
                        component.m_onPointerExit += new Action<UIInputHandler>((UIInputHandler ih) =>
                        {
                            OnHoverPieceExitHandler.Invoke(__instance, ih);
                        });

                        m_pieceIcons.Add(pieceIconData);
                    }
                }
            }
            for (int y = 0; y < num2; ++y)
            {
                for (int x = 0; x < num1; ++x)
                {
                    int index = y * num1 + x;
                    var pieceIcon = m_pieceIcons[index];
                    ((GameObject)m_markerField.GetValue(pieceIcon)).SetActive(new Vector2Int(x, y) == selectedNr);

                    if (index < buildPieces.Count)
                    {
                        Piece piece = buildPieces[index];
                        ((Image)m_iconField.GetValue(pieceIcon)).sprite = piece.m_icon;
                        ((Image)m_iconField.GetValue(pieceIcon)).enabled = true;
                        ((UITooltip)m_tooltipField.GetValue(pieceIcon)).m_text = piece.m_name;
                        ((GameObject)m_upgradeField.GetValue(pieceIcon)).SetActive(piece.m_isUpgrade);
                    }
                    else
                    {
                        ((Image)m_iconField.GetValue(pieceIcon)).enabled = false;
                        ((UITooltip)m_tooltipField.GetValue(pieceIcon)).m_text = "";
                        ((GameObject)m_upgradeField.GetValue(pieceIcon)).SetActive(false);
                    }
                }
            }
            UpdatePieceBuildStatusHandler.Invoke(__instance, buildPieces, player);
            if (updateAllBuildStatuses)
                UpdatePieceBuildStatusAllHandler.Invoke(__instance, buildPieces, player);
            if (m_lastPieceCategoryRef(__instance) == category)
                return false;
            m_lastPieceCategoryRef(__instance) = category;
            m_pieceBarPosXRef(__instance) = m_pieceBarTargetPosXRef(__instance);
            UpdatePieceBuildStatusAllHandler.Invoke(__instance, buildPieces, player);

            return false;
        }
    }*/
}
