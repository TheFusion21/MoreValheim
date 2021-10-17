using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace MoreValheim.GamePatches
{

    /// <summary>
    /// Add drop for sand on shores
    /// </summary>
    [HarmonyPatch(typeof(TerrainOp))]
    [HarmonyPatch("OnPlaced")]
    class TerrainOpPatch
    {
        
        static void Postfix(TerrainOp __instance)
        {
            if (__instance.m_spawnOnPlaced.name.ToLower() == "stone")
            {
                bool rightDepth = false;
                List<Heightmap> heightmaps = new List<Heightmap>();
                Heightmap.FindHeightmap(__instance.transform.position, __instance.GetRadius(), heightmaps);

                foreach (var heightmap in heightmaps)
                {
                    float waterDepth = Heightmap.GetOceanDepthAll(__instance.transform.position);
                    if (waterDepth > 1.0 && waterDepth < 2.0)
                    {
                        rightDepth = true;
                        break;
                    }
                }
                if (UnityEngine.Random.value < 0.65f && rightDepth)
                {
                    GameObject gameObject = GameObject.Instantiate(ObjectDB.instance.GetItemPrefab("Sand"), __instance.transform.position + Vector3.up * 0.5f + (Vector3)(UnityEngine.Random.insideUnitCircle * 0.2f), Quaternion.identity);
                    gameObject.GetComponent<ItemDrop>().m_itemData.m_stack = Random.Range(1, 2);
                    gameObject.GetComponent<Rigidbody>().velocity = Vector3.up * 4f;
                }

            }
        }
    }
}
