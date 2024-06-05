



using Atomicrops.Core.Loot;
using Atomicrops.Core.SoDb2;
using Atomicrops.Core.Upgrades;
using Atomicrops.Crops;
using Atomicrops.Game.Loot;
using BepInEx;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace AtomicLoader
{
    public class JsonItem
    {
        public string ItemName { get; set; }
        public string ItemImageName { get; set; }
        public string DisplayNameModifier { get; set; }
        public string DescriptionModifier { get; set; }
        public bool ISStackable { get; set; }
        public int maxStack { get; set; }
        public bool CustomCostModifier { get; set; }
        public int CustomCostAmountModifier { get; set; }
        public bool AddSeedsModifier { get; set; }
        public Atomicrops.Crops.CropDef? SeedsToAdd { get; set; }
        public int SeedAmount { get; set; }
        public bool AddfriendsModifier { get; set; }
        public bool FreindsAroundPlayer { get; set; }
        public bool AddturretsModifier { get; set; }
        public bool AddGardenBedModifier { get; set; }
        public bool IsTomorrowLongerModifier { get; set; }
        public bool DontSpawnLastDay { get; set; }
        public string UpgradeParamModifier { get; set; }
        public string AtomicLoaderRarityModifier { get; set; }

        public int? DebugMode { get; set; }
    }

    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }

    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;  // Changed to public

        private void Awake()
        {
            Log = Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }
    }




    [HarmonyPatch(typeof(LootCollection), MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(LootCollectionDef), typeof(LootCollectionIdsEnum), typeof(int), typeof(bool), typeof(bool), typeof(bool) })]
    class LootCollection_Constructor_Patch
    {

        public static Texture2D texture;
        public static Sprite myCustomSprite;

        public static string DllDirectory = Directory.GetCurrentDirectory();

        JsonItem CustomItem = new JsonItem();

        public static void Postfix(LootCollection __instance, LootCollectionDef collectionDef, LootCollectionIdsEnum id, int seed, bool doDlcCheck, bool isCrow, bool isClassic)
        {
            List<JsonItem> CutomItems = new List<JsonItem>();


            string JsonFolderDirectory = $@"{DllDirectory}\BepInEx\plugins\JsonFolder";

            string[] filePaths = Directory.GetFiles($@"{JsonFolderDirectory}", "*.json");
            foreach (var filePath in filePaths)
            {
                string jsonFilePath = $@"{filePath}";



                var reader = new StreamReader(jsonFilePath);

                string json = reader.ReadToEnd();
                JsonItem CustomItem = JsonConvert.DeserializeObject<JsonItem>(json);


                if (id == LootCollectionIdsEnum.Main)
                {


                    UpgradeDef myUpgrade = ScriptableObject.CreateInstance<UpgradeDef>();
                    // creating a new instance of UpgradeDef

                    bool contains = CustomItem.AtomicLoaderRarityModifier.Contains("common", StringComparison.OrdinalIgnoreCase);
                    myUpgrade.name = $"{CustomItem.ItemName}"; // important
                    myUpgrade.UpgradeType = UpgradeDef.UpgradeTypeEnum.Upgrade;
                    myUpgrade.Disabled = false;
                    myUpgrade.MaxStacks = CustomItem.maxStack;
                    myUpgrade.RemoveUpgradesWhenPickedUp = new List<UpgradeDef>();
                    myUpgrade.DoAddDependents = false;
                    myUpgrade.DependentCollection = LootCollectionIdsEnum.Main;
                    myUpgrade.Dependents = new List<UpgradeDef>();
                    myUpgrade.DependentsILoot = new List<SoDb2Item>();
                    myUpgrade.DoInstantApply = false;
                    myUpgrade.InstantApply = null;
                    myUpgrade.InstantApplyAmount = 1;
                    myUpgrade.DoRandomSelectInstantApply = false;
                    myUpgrade.InstantApplyRandomSelect = new List<InstantApplyLootDef>();
                    myUpgrade.DoAddSeeds = CustomItem.AddSeedsModifier;
                    myUpgrade.AddSeeds = CustomItem.SeedsToAdd;
                    myUpgrade.AddSeedsList = new List<CropDef>();
                    myUpgrade.AddSeedsCount = CustomItem.SeedAmount;
                    myUpgrade.AddAloeVeraHeals = 0;
                    myUpgrade.DropOnDamage = false;
                    myUpgrade.DropFx = null;
                    myUpgrade.DropSound = null;
                    myUpgrade.IsTomorrowLongerUpgrade = CustomItem.IsTomorrowLongerModifier;
                    myUpgrade.DoAddFriends = CustomItem.AddfriendsModifier;
                    myUpgrade.AddFriendAroundPlayer = CustomItem.FreindsAroundPlayer;
                    myUpgrade.AddFriends = new List<FriendDef>();
                    myUpgrade.DoAddTurrets = CustomItem.AddturretsModifier;
                    myUpgrade.Turrets = new List<TurretDef>();
                    myUpgrade.RunFunction = UpgradeDef.FunctionEnum.None;
                    myUpgrade.DoAddGardenBed = CustomItem.AddGardenBedModifier;
                    myUpgrade.GardenBeds = new List<GardenBedDef>();
                    myUpgrade.AddPowerSowableSeeds = 0;

                    // Params is a private variable so it must be obtained using reflection and set manually
                    var fieldInfo = typeof(UpgradeDef).GetField("Params", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fieldInfo != null)
                    {
                        // Create a new UpgradeParam
                        UpgradeParam myUpgradeParam = new UpgradeParam();
                        myUpgradeParam.Path = $"Player.{CustomItem.UpgradeParamModifier}";
                        myUpgradeParam.Value = 0.2f;
                        myUpgradeParam.Action = UpgradeParam.Operation.Add;

                        // use reflection to set ValueMin and ValueMax
                        var fieldInfoMin = typeof(UpgradeParam).GetField("ValueMin", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (fieldInfoMin != null)
                        {
                            fieldInfoMin.SetValue(myUpgradeParam, 0.2f);
                        }

                        var fieldInfoMax = typeof(UpgradeParam).GetField("ValueMax", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (fieldInfoMax != null)
                        {
                            fieldInfoMax.SetValue(myUpgradeParam, 0.2f);
                        }


                        // Create a list of UpgradeParam and add myUpgradeParam to it
                        List<UpgradeParam> upgradeParamsList = new List<UpgradeParam>() { myUpgradeParam };

                        fieldInfo.SetValue(myUpgrade, upgradeParamsList);
                    }

                    // LootProperties is a big pain in the ass we have to fill out manually
                    LootDefProperties lootDefProperties = new LootDefProperties();

                    lootDefProperties.Tag = LootDefProperties.TagEnum.None;
                    lootDefProperties.Dlc = AtomicropsDlcManager.Dlcs.None;
                    lootDefProperties.Build = "";
                    lootDefProperties.DisplayName = $"{CustomItem.DisplayNameModifier}"; // important
                    lootDefProperties.Description = $"{CustomItem.DescriptionModifier}"; // important
                    lootDefProperties.DisplayNameLocalizationKey = "";
                    lootDefProperties.DoNameFormatter = false;
                    lootDefProperties.NameFormatterArg1 = "";
                    lootDefProperties.DoLocNameFormatterArg1 = false;
                    lootDefProperties.DescriptionLocalizationKey = "";
                    lootDefProperties.DoDescFormatter = false;
                    lootDefProperties.DescFormatterArg1 = "";
                    lootDefProperties.DoLocDescFormatterArg1 = false;
                    lootDefProperties.DescFormatterArg2 = "";
                    lootDefProperties.DoLocDescFormatterArg2 = false;
                    lootDefProperties.DescFormatterArg3 = "";
                    lootDefProperties.DoLocDescFormatterArg3 = false;
                    lootDefProperties.DoAltDescFormattersForCrow = false;
                    lootDefProperties.AltDescFormattersForCrow = new LootDefProperties.DescFormatters();
                    lootDefProperties.DoDescComposition = false;
                    lootDefProperties.AppendDescComposition = false;
                    lootDefProperties.DescCompJoinUseComma = false;
                    lootDefProperties.LocElementsForDescComposition = new List<LocElement>();
                    contains = CustomItem.AtomicLoaderRarityModifier.Contains("common", StringComparison.OrdinalIgnoreCase);
                    if (contains)
                    {
                        lootDefProperties.Rarity = Atomicrops.Core.Loot.Rarity_.Common; // important
                    }
                    else
                    {
                        contains = CustomItem.AtomicLoaderRarityModifier.Contains("uncommon", StringComparison.OrdinalIgnoreCase);
                        if (contains)
                        {
                            lootDefProperties.Rarity = Atomicrops.Core.Loot.Rarity_.Uncommon; // important
                        }
                        else
                        {
                            contains = CustomItem.AtomicLoaderRarityModifier.Contains("rare", StringComparison.OrdinalIgnoreCase);
                            if (contains)
                            {
                                lootDefProperties.Rarity = Atomicrops.Core.Loot.Rarity_.Rare; // important
                            }
                            else
                            {
                                contains = CustomItem.AtomicLoaderRarityModifier.Contains("legendary", StringComparison.OrdinalIgnoreCase);
                                if (contains)
                                {
                                    lootDefProperties.Rarity = Atomicrops.Core.Loot.Rarity_.Legendary; // important
                                }
                                else { lootDefProperties.Rarity = Atomicrops.Core.Loot.Rarity_.Common; }
                            }
                        }
                    }
                    lootDefProperties.PrimaryBiome = 0;
                    lootDefProperties.LuckMult = 0f;
                    lootDefProperties.UseCustomCost = CustomItem.CustomCostModifier;
                    lootDefProperties.CustomCost = CustomItem.CustomCostAmountModifier;
                    lootDefProperties.DontSpawnOnLastDay = CustomItem.DontSpawnLastDay;
                    lootDefProperties.DoMutuallyExclusive = false;
                    lootDefProperties.MutuallyExclusive = null;
                    lootDefProperties.InventoryIconAnim = null;
                    lootDefProperties.InventoryIconSelected = null;
                    lootDefProperties.InventoryIconSelectedAnim = null;
                    lootDefProperties.InGameLootSprite = null;
                    lootDefProperties.InGameLootClip = null;
                    lootDefProperties.DoAltIconsIfCrow = false;
                    lootDefProperties.IconsIfCrow = new LootDefProperties.Icons();
                    lootDefProperties.RevealClip = null;
                    lootDefProperties.LootSpriteColorMult = UnityEngine.Color.white;
                    lootDefProperties.InGameLootShadowHeightOffset = 0f;
                    lootDefProperties.SetSortOffset = false;
                    lootDefProperties.SortOffset = 0f;
                    lootDefProperties.SizeForShadow = 1f;
                    lootDefProperties.ShowTooltip = true;
                    lootDefProperties.Stack = CustomItem.ISStackable;
                    lootDefProperties.DoHop = true;
                    lootDefProperties.Flash = true;
                    lootDefProperties.NoToolTipRegion = false;
                    lootDefProperties.ToolTipOffset = new Vector2(0, 0.3f);


                    string directory = System.IO.Path.GetDirectoryName($@"{DllDirectory}\BepInEx\plugins\JsonFolder\Images\{CustomItem.ItemImageName}\");
                    string filePathImage = $@"{DllDirectory}\BepInEx\plugins\JsonFolder\Images\{CustomItem.ItemImageName}\{CustomItem.ItemImageName}.png";


                    byte[] imageBytes = System.IO.File.ReadAllBytes(filePathImage);
                    Texture2D texture = new Texture2D(2, 2);  // The initial size doesn't matter since LoadImage will replace it.
                    if (UnityEngine.ImageConversion.LoadImage(texture, imageBytes))  // Directly use LoadImage without prefix
                    {
                        Rect rect = new Rect(0, 0, texture.width, texture.height);
                        Vector2 pivot = new Vector2(0.5f, 0.5f);
                        float pixelsPerUnit = 33.0f;
                        myCustomSprite = Sprite.Create(texture, rect, pivot, pixelsPerUnit);
                    }
                    else
                    {
                        Debug.LogError("Failed to load image.");
                    }

                    
                    
                      
                    
                    


                    // manually setting InventoryIcon and InGameLootSprite
                    lootDefProperties.InventoryIcon = myCustomSprite;
                    lootDefProperties.InGameLootSprite = myCustomSprite;

                    // manually setting pickup sound event from another item
                    lootDefProperties.PickupSoundEvent = SoDb2Utils.GetItem<UpgradeDef>(10).LootProperties.PickupSoundEvent;
                    lootDefProperties.DropSoundEvent = SoDb2Utils.GetItem<UpgradeDef>(10).LootProperties.DropSoundEvent;

                    // setting our new LootDefProperties to myUpgrade
                    myUpgrade.LootProperties = lootDefProperties;

                    // Get the private field _loots using reflection
                    FieldInfo field = typeof(LootCollection).GetField("_loots", BindingFlags.NonPublic | BindingFlags.Instance);
                    Plugin.Log.LogInfo($"{field}");

                    if (field != null)
                    {
                        // Get the current value of _loots
                        List<ILootDef> loots = field.GetValue(__instance) as List<ILootDef>;

                        if (loots != null)
                        {
                            // Use current millisecond combined with the hash code of the modIdentifier as the seed for the Random object
                            string modIdentifier = MyPluginInfo.PLUGIN_GUID;
                            int randomSeed = DateTime.Now.Millisecond + modIdentifier.GetHashCode();
                            System.Random rand = new System.Random(randomSeed);
                            if (CustomItem.DebugMode == null)
                            {
                                // calculate a random index between 0 and the count of items in the list
                                int randomIndex = rand.Next(loots.Count);

                                // insert myUpgrade at a random position
                                loots.Insert(randomIndex, myUpgrade);
                            }
                            else { loots.Insert(0, myUpgrade); }
                            // Set the field's value to the new list
                            field.SetValue(__instance, loots);
                        }
                    }




                }
            }
        }
    }
}










