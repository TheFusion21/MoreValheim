using MoreValheimInterface;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Logging;
using System.Reflection;

namespace MoreValheim
{
    class MoreValheimDB : MonoBehaviour
    {
        public ManualLogSource Logger;
        public static MoreValheimDB instance;
        public List<StatusEffect> m_customStatusEffects = new List<StatusEffect>();
        public List<GameObject> m_customItems = new List<GameObject>();
        public List<Recipe> m_customRecipes = new List<Recipe>();
        public Dictionary<int, GameObject> m_customItemByHash = new Dictionary<int, GameObject>();
        public List<GameObject> m_customPieces = new List<GameObject>();

        private readonly List<RecipeInterface> recipeInterfaces = new List<RecipeInterface>
        {
            new RecipeInterface
            {
                m_name = "Recipe_Glass2",
                m_item = "Glass",
                m_amount = 2,
                m_enabled = true,
                m_craftingStation = "$piece_forge",
                m_repairStation = "$piece_forge",
                m_minStationLevel = 1,
                m_resources = new RequirementInterface[]
                {
                    new RequirementInterface
                    {
                        m_resItem = "Sand",
                        m_amount = 10,
                        m_amountPerLevel = 1,
                        m_recover = false
                    }
                }
            },
            new RecipeInterface
            {
                m_name = "Recipe_Glass",
                m_item = "Glass",
                m_amount = 1,
                m_enabled = true,
                m_craftingStation = "$piece_forge",
                m_repairStation = "$piece_forge",
                m_minStationLevel = 1,
                m_resources = new RequirementInterface[]
                {
                    new RequirementInterface
                    {
                        m_resItem = "Sand",
                        m_amount = 5,
                        m_amountPerLevel = 1,
                        m_recover = false
                    }
                }
            }
        };
        private readonly List<PieceInterface> pieceInterfaces = new List<PieceInterface>
        {
            new PieceInterface
            {
                m_craftingStation = "$piece_workbench",
                m_piece = "glasswindow1",
                m_resources = new RequirementInterface[]
                {
                    new RequirementInterface
                    {
                        m_amount = 3,
                        m_recover = false,
                        m_resItem = "Wood",
                        m_amountPerLevel = 1
                    },
                    new RequirementInterface
                    {
                        m_amount = 2,
                        m_recover = false,
                        m_resItem = "Glass",
                        m_amountPerLevel = 1
                    }
                }
            },
            new PieceInterface
            {
                m_craftingStation = "$piece_workbench",
                m_piece = "glasswindow2",
                m_resources = new RequirementInterface[]
                {
                    new RequirementInterface
                    {
                        m_amount = 2,
                        m_recover = false,
                        m_resItem = "Wood",
                        m_amountPerLevel = 1
                    },
                    new RequirementInterface
                    {
                        m_amount = 1,
                        m_recover = false,
                        m_resItem = "Glass",
                        m_amountPerLevel = 1
                    }
                }
            },
            new PieceInterface
            {
                m_craftingStation = "$piece_workbench",
                m_piece = "glassdoor1",
                m_resources = new RequirementInterface[]
                {
                    new RequirementInterface
                    {
                        m_amount = 4,
                        m_recover = false,
                        m_resItem = "Wood",
                        m_amountPerLevel = 1
                    },
                    new RequirementInterface
                    {
                        m_amount = 1,
                        m_recover = false,
                        m_resItem = "Glass",
                        m_amountPerLevel = 1
                    }
                }
            },
            new PieceInterface
            {
                m_craftingStation = "$piece_workbench",
                m_piece = "woodplate",
                m_resources = new RequirementInterface[]
                {
                    new RequirementInterface
                    {
                        m_amount = 1,
                        m_recover = false,
                        m_resItem = "Wood",
                        m_amountPerLevel = 1
                    }
                }
            },
            new PieceInterface
            {
                m_craftingStation = "$piece_workbench",
                m_piece = "woodcup",
                m_resources = new RequirementInterface[]
                {
                    new RequirementInterface
                    {
                        m_amount = 1,
                        m_recover = false,
                        m_resItem = "Wood",
                        m_amountPerLevel = 1
                    }
                }
            },
            new PieceInterface
            {
                m_craftingStation = "$piece_workbench",
                m_piece = "tablecandle",
                m_resources = new RequirementInterface[]
                {
                    new RequirementInterface
                    {
                        m_amount = 1,
                        m_recover = false,
                        m_resItem = "Iron",
                        m_amountPerLevel = 1
                    },
                    new RequirementInterface
                    {
                        m_amount = 1,
                        m_recover = false,
                        m_resItem = "Honey",
                        m_amountPerLevel = 1
                    }
                }
            }
        };

        private ObjectDB odb;

        public bool loaded = false;

        private AssetBundle assets;

        private List<Material> materials = new List<Material>();

        //$piece_cauldron
        //$piece_forge
        //$piece_artisanstation
        //$piece_workbench
        //$piece_stonecutter
        private List<CraftingStation> stations;

        private void Awake()
        {
            
            instance = this;
        }

        private void OnDestroy()
        {
            if(assets != null)
                assets.Unload(true);
            
        }

        public void LoadAssets()
        {
            //Load the interface assembly to access interace e.g MaterialInterface
            Assembly.Load(Properties.Resources.MoreValheimInterface);

            assets = AssetBundle.LoadFromMemory(Properties.Resources.itemdrops);

            odb = FejdStartup.instance.m_gameMainPrefab.GetComponent<ObjectDB>();

            stations = new List<CraftingStation>(Resources.FindObjectsOfTypeAll<CraftingStation>());

            materials.AddRange(Resources.FindObjectsOfTypeAll<Material>());
            materials.AddRange(assets.LoadAllAssets<Material>());

            GameObject[] prefabs = assets.LoadAllAssets<GameObject>();

            foreach (var prefab in prefabs)
            {
                if (prefab.GetComponent<ItemDrop>() != null)
                    LoadItem(prefab);
                if (prefab.GetComponent<Piece>() != null)
                    LoadPiece(prefab);

            }
            foreach (var recipeInterface in recipeInterfaces)
            {
                var recipe = ScriptableObject.CreateInstance<Recipe>();
                recipe.name = recipeInterface.m_name;
                var recipeItem = odb.m_items.Find(item => item.name.ToLower() == recipeInterface.m_item.ToLower());
                if (recipeItem == null)
                    recipeItem = m_customItems.Find(item => item.name.ToLower() == recipeInterface.m_item.ToLower());

                if(recipeItem == null)
                {
                    Logger.LogError("Cant find recipe item " + recipeInterface.m_item);
                    return;
                }    
                recipe.m_item = recipeItem.GetComponent<ItemDrop>();
                recipe.m_amount = recipeInterface.m_amount;
                recipe.m_enabled = recipeInterface.m_enabled;
                recipe.m_craftingStation = stations.Find(s => s.name == recipeInterface.m_craftingStation);
                recipe.m_repairStation = stations.Find(s => s.name == recipeInterface.m_repairStation);
                recipe.m_minStationLevel = recipeInterface.m_minStationLevel;
                if (recipeInterface.m_resources == null)
                    continue;
                recipe.m_resources = new Piece.Requirement[recipeInterface.m_resources.Length];
                for(int i = 0;i<recipe.m_resources.Length;i++)
                {
                    var recipeItem2 = odb.m_items.Find(item => item.name == recipeInterface.m_resources[i].m_resItem);
                    if (recipeItem2 == null)
                        recipeItem2 = m_customItems.Find(item => item.name == recipeInterface.m_resources[i].m_resItem);
                    if (recipeItem2 == null)
                    {
                        Logger.LogInfo("Cant find recipe item " + recipeInterface.m_item);
                        return;
                    }
                    recipe.m_resources[i] = new Piece.Requirement
                    {
                        m_resItem = recipeItem2.GetComponent<ItemDrop>(),
                        m_amount = recipeInterface.m_resources[i].m_amount,
                        m_amountPerLevel = recipeInterface.m_resources[i].m_amountPerLevel,
                        m_recover = recipeInterface.m_resources[i].m_recover
                    };
                }
                m_customRecipes.Add(recipe);
            }

            Logger.LogInfo($"Loaded {m_customItems.Count} prefabs");
            Logger.LogInfo($"Loaded {m_customRecipes.Count} recipes");
            Logger.LogInfo($"Loaded {m_customStatusEffects.Count} status effects");
            Logger.LogInfo($"Loaded {m_customPieces.Count} pieces");
            loaded = true;
        }
        
        private void LoadItem(GameObject prefab)
        {
            prefab.layer = 12;

            var materialInterfaces = prefab.GetComponentsInChildren<MaterialInterface>(true);
            if (materialInterfaces.Length > 0)
            {
                bool found = false;

                foreach (var materialInterface in materialInterfaces)
                {
                    found = false;
                    var matNames = materialInterface.materialNames;

                    var renderer = materialInterface.GetComponent<Renderer>();
                    renderer.sharedMaterials = new Material[matNames.Length];
                    renderer.materials = new Material[matNames.Length];

                    for (int i = 0; i < matNames.Length; i++)
                    {
                        int index = materials.FindIndex(m => m.name.ToLower() == matNames[i].ToLower());
                        if (index != -1)
                        {
                            if (i == 0)
                            {
                                renderer.sharedMaterial = materials[index];
                                renderer.material = materials[index];
                            }
                            found = true;
                            renderer.sharedMaterials[i] = materials[index];
                            renderer.materials[i] = materials[index];
                            break;
                        }
                    }
                    if (!found)
                        return;
                }
            }
            this.m_customItems.Add(prefab);
        }

        private void LoadPiece(GameObject prefab)
        {
            PieceInterface pieceInterface = pieceInterfaces.Find(p => p.m_piece.ToLower() == prefab.name.ToLower());
            if (pieceInterface == null)
                return;

            foreach (var trans in prefab.GetComponentsInChildren<Transform>(true))
            {
                if(trans.gameObject.name.Contains("snappoint"))
                {
                    trans.tag = "snappoint";
                }
                trans.gameObject.layer = 10;
            }

            //Find every materialInterface to set apropiate material ingame
            var materialInterfaces = prefab.GetComponentsInChildren<MaterialInterface>(true);
            if (materialInterfaces.Length > 0)
            {
                bool found = false;

                foreach (var materialInterface in materialInterfaces)
                {
                    found = false;
                    var matNames = materialInterface.materialNames;

                    var renderer = materialInterface.GetComponent<Renderer>();
                    renderer.sharedMaterials = new Material[matNames.Length];
                    renderer.materials = new Material[matNames.Length];

                    for (int i = 0; i < matNames.Length; i++)
                    {
                        int index = materials.FindIndex(m => m.name.ToLower() == matNames[i].ToLower());
                        if (index != -1)
                        {
                            if (i == 0)
                            {
                                renderer.sharedMaterial = materials[index];
                                renderer.material = materials[index];
                            }
                            found = true;
                            renderer.sharedMaterials[i] = materials[index];
                            renderer.materials[i] = materials[index];
                            break;
                        }
                    }
                    if (!found)
                        return;
                    Destroy(materialInterface);
                }
            }

            //Set requirements
            Piece piece = prefab.GetComponent<Piece>();

            piece.m_craftingStation = stations.Find(s => s.m_name.ToLower() == pieceInterface.m_craftingStation.ToLower());
            if(piece.m_craftingStation == null)
            {
                Logger.LogInfo("Cant find crafting station item " + pieceInterface.m_craftingStation);
                return;
            }
            piece.m_resources = new Piece.Requirement[pieceInterface.m_resources.Length];

            for (int i = 0; i < piece.m_resources.Length; i++)
            {
                var recipeItem = odb.m_items.Find(item => item.name.ToLower() == pieceInterface.m_resources[i].m_resItem.ToLower());
                if (recipeItem == null)
                    recipeItem = m_customItems.Find(item => item.name.ToLower() == pieceInterface.m_resources[i].m_resItem.ToLower());
                if (recipeItem == null)
                {
                    Logger.LogInfo("Cant find recipe item " + pieceInterface.m_piece);
                    return;
                }
                piece.m_resources[i] = new Piece.Requirement
                {
                    m_resItem = recipeItem.GetComponent<ItemDrop>(),
                    m_amount = pieceInterface.m_resources[i].m_amount,
                    m_amountPerLevel = pieceInterface.m_resources[i].m_amountPerLevel,
                    m_recover = pieceInterface.m_resources[i].m_recover
                };
            }

            //Check for FireplaceInterface
            var fireplaceInterface = prefab.GetComponent<FireplaceInterface>();
            if(fireplaceInterface != null)
            {
                var fireplace = prefab.GetComponent<Fireplace>();
                var fuelItem = odb.m_items.Find(item => item.name.ToLower() == fireplaceInterface.fuelItemName.ToLower());
                if (fuelItem == null)
                    fuelItem = m_customItems.Find(item => item.name.ToLower() == fireplaceInterface.fuelItemName.ToLower());
                if (fuelItem == null)
                {
                    Logger.LogInfo("Cant find fuel item " + fireplaceInterface.fuelItemName);
                    return;
                }
                fireplace.m_fuelItem = fuelItem.GetComponent<ItemDrop>();
            }

            m_customPieces.Add(prefab);

        }
        
        public void UpdateItemHashes()
        {
            this.m_customItemByHash.Clear();
            foreach (GameObject gameObject in this.m_customItems)
                this.m_customItemByHash.Add(gameObject.name.GetStableHashCode(), gameObject);
        }

        public void Clear()
        {
            m_customStatusEffects.Clear();
            m_customItems.Clear();
            m_customRecipes.Clear();
            m_customItemByHash.Clear();
        }
        
       
    }
}
