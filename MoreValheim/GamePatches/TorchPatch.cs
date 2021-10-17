using HarmonyLib;

namespace MoreValheim.GamePatches
{
    /// <summary>
    /// Patches Players StatusEffect Update to incooperate Torch Freeze/Cold reduction
    /// </summary>
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("UpdateEnvStatusEffects")]
    class TorchPatch
    {
        static AccessTools.FieldRef<Player, float> m_nearFireTimerRef = AccessTools.FieldRefAccess<Player, float>("m_nearFireTimer");
        static AccessTools.FieldRef<Player, bool> m_underRoofRef = AccessTools.FieldRefAccess<Player, bool>("m_underRoof");
        static AccessTools.FieldRef<Player, SEMan> m_semanRef = AccessTools.FieldRefAccess<Player, SEMan>("m_seman");
        static AccessTools.FieldRef<Player, bool> m_safeInHomeRef = AccessTools.FieldRefAccess<Player, bool>("m_safeInHome");

        static FastInvokeHandler GetDamageModifiersHandler = MethodInvoker.GetHandler(AccessTools.Method(typeof(Player), "GetDamageModifiers"));
        //static AccessTools.FieldRef<Player, >

        static bool Prefix(Player __instance, float dt)
        {
            m_nearFireTimerRef(__instance) += dt;
            HitData.DamageModifiers damageModifiers = (HitData.DamageModifiers)GetDamageModifiersHandler.Invoke(__instance);
            //flag1
            bool isNearFire = m_nearFireTimerRef(__instance) < 0.25f;
            //flag2
            bool hasBurningStatus = m_semanRef(__instance).HaveStatusEffect("Burning");
            //flag3
            bool inShelter = __instance.InShelter();
            HitData.DamageModifier modifier = damageModifiers.GetModifier(HitData.DamageType.Frost);
            //flag4
            bool isFreezing = EnvMan.instance.IsFreezing();
            bool isCold = EnvMan.instance.IsCold();
            //flag5
            bool isWet = EnvMan.instance.IsWet();
            //flag6
            bool isSensed = __instance.IsSensed();
            //flag7
            bool isWet2 = m_semanRef(__instance).HaveStatusEffect("Wet");
            //flag8
            bool isSitting = __instance.IsSitting();
            //flag9
            bool isInsideCozyArea = EffectArea.IsPointInsideArea(__instance.transform.position, EffectArea.Type.WarmCozyArea, 1.0f) != null;
            bool hasTorchEqquiped = __instance.GetInventory().GetEquipedtems().FindIndex(item => item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Torch) != -1;
            //flag10
            bool shouldBeFreezing = isFreezing && !isNearFire && !inShelter;
            //flag11
            bool shouldBeCold = isCold && !isNearFire || isFreezing & isNearFire && !inShelter || ((!isFreezing ? 0 : (!isNearFire ? 1 : 0)) & (inShelter ? 1 : 0)) != 0;

            if (((modifier == HitData.DamageModifier.Resistant ? 1 : (modifier == HitData.DamageModifier.VeryResistant ? 1 : 0)) | (isInsideCozyArea ? 1 : 0)) != 0)
            {
                shouldBeFreezing = false;
                shouldBeCold = false;
            }
            //Custom Code
            if (hasTorchEqquiped)
            {
                if (shouldBeFreezing)
                {
                    shouldBeFreezing = false;
                    shouldBeCold = true;
                }
                else
                {
                    shouldBeCold = false;
                }
            }
            if (isWet && !m_underRoofRef(__instance))
                m_semanRef(__instance).AddStatusEffect("Wet", true);
            if (inShelter)
                m_semanRef(__instance).AddStatusEffect("Shelter");
            else
                m_semanRef(__instance).RemoveStatusEffect("Shelter");
            if (isNearFire)
                m_semanRef(__instance).AddStatusEffect("CampFire");
            else
                m_semanRef(__instance).RemoveStatusEffect("CampFire");

            bool isResting = ((isSensed || !(isSitting | inShelter) || (shouldBeCold || shouldBeFreezing) || isWet2 && !isInsideCozyArea ? 0 : (!hasBurningStatus ? 1 : 0)) & (isNearFire ? 1 : 0)) != 0;
            if (isResting)
                m_semanRef(__instance).AddStatusEffect("Resting");
            else
                m_semanRef(__instance).RemoveStatusEffect("Resting");
            m_safeInHomeRef(__instance) = isResting & inShelter;
            if (shouldBeFreezing)
            {
                if (m_semanRef(__instance).RemoveStatusEffect("Cold", true))
                    return false;
                m_semanRef(__instance).AddStatusEffect("Freezing");
            }
            else if (shouldBeCold)
            {
                if (m_semanRef(__instance).RemoveStatusEffect("Freezing", true) || m_semanRef(__instance).AddStatusEffect("Cold") == null)
                    return false;
                __instance.ShowTutorial("cold");
            }
            else
            {
                m_semanRef(__instance).RemoveStatusEffect("Cold");
                m_semanRef(__instance).RemoveStatusEffect("Freezing");
            }
            return false;
        }
    }
}
