using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using MoreValheim.GamePatches;
using HarmonyLib.Tools;
using MoreValheim.MonoBehaviours;

namespace MoreValheim
{
    public class PieceInterface
    {
        public string m_craftingStation;
        public string m_piece;
        public RequirementInterface[] m_resources = new RequirementInterface[0];
    }

    public class RequirementInterface
    {
        [Header("Resource")]
        public string m_resItem;
        public int m_amount;
        [Header("Item")]
        public int m_amountPerLevel;
        [Header("Piece")]
        public bool m_recover;
    }
    
    public class RecipeInterface
    {
        public string m_name;
        public string m_item;
        public int m_amount;
        public bool m_enabled;
        [Header("Requirements")]
        public string m_craftingStation;
        public string m_repairStation;
        public int m_minStationLevel;
        [SerializeField]
        public RequirementInterface[] m_resources = new RequirementInterface[0];
    }

    [BepInPlugin("com.github.thefusion21.morevalheim", "More Valheim", "0.0.1.0")]
    public class MoreValheim : BaseUnityPlugin
    {
        private Harmony harmony;

        private bool IsVersionCompatible()
        {
            var t = AccessTools.TypeByName("Version");

            int major = (int)t.GetField("m_major", BindingFlags.Static | BindingFlags.Public).GetValue(null);
            int minor = (int)t.GetField("m_minor", BindingFlags.Static | BindingFlags.Public).GetValue(null);
            int patch = (int)t.GetField("m_patch", BindingFlags.Static | BindingFlags.Public).GetValue(null);

            if (major > 0)
                return false;
            if (minor > 203)
                return false;
            if (patch > 11)
                return false;
            return true;
        }
        
        private void PatchVersion()
        {
            VersionPatch.version = Info.Metadata.Version;

            var t = AccessTools.TypeByName("Version");
            var mi = t.GetMethod("GetVersionString", BindingFlags.Static | BindingFlags.Public);

            var postfix = typeof(VersionPatch).GetMethod("GetVersionString", BindingFlags.Static | BindingFlags.Public);

            var postfixMethod = new HarmonyMethod(postfix);

            harmony.Patch(mi, postfix: postfixMethod);
        }

        private void Awake()
        {
            //Check compatiblity to prevent using modded world/playerprofile that could get corrupted
            if(!IsVersionCompatible())
            {
                Logger.LogError($"[{Info.Metadata.Name}:{Info.Metadata.Version}] not compatible!");
                Destroy(this);
                return;
            }
            

            
            //Add the custom ObjectDB
            var db = gameObject.AddComponent<MoreValheimDB>();
            db.Logger = Logger;

            HarmonyFileLog.Enabled = true;
            //Custom patches
            harmony = new Harmony(Info.Metadata.GUID);
            
            harmony.PatchAll();
            Logger.LogInfo($"[{Info.Metadata.Name}:{Info.Metadata.Version}] loaded!");
        }

        private void Start()
        {
            PatchVersion();
        }
        private void OnDestroy()
        {
            if (MoreValheimDB.instance != null)
            {
                MoreValheimDB.instance.Clear();
                Destroy(MoreValheimDB.instance);
            }
            if(harmony != null)
                harmony.UnpatchSelf();
        }
    }
}
