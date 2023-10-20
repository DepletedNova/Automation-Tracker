global using static KitchenTracker.Main;
using Kitchen;
using KitchenLib;
using KitchenLib.Utils;
using KitchenLib.Views;
using KitchenMods;
using KitchenTracker.GDOs;
using KitchenTracker.Systems;
using KitchenTracker.Views;
using PreferenceSystem;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static PreferenceSystem.PreferenceSystemManager;

namespace KitchenTracker
{
    public class Main : BaseMod
    {
        public const string NAME = "Automation Tracker";
        public const string GUID = "nova.production-tracker";
        public const string VERSION = "1.0.1";

        public Main() : base(GUID, NAME, "Zoey Davis", VERSION, ">=1.0.0", Assembly.GetExecutingAssembly()) { }

        private static AssetBundle Bundle;
        public static PreferenceSystemManager PrefManager;

        #region References
        public static CustomViewType BasicTracker { get; private set; }
        public static CustomViewType AdvancedTracker { get; private set; }

        public const float UpdatesPerMinute = 6f;

        public static int BinID { get; private set; }
        public static int SpotID { get; private set; }
        #endregion

        protected override void OnPostActivate(Mod mod)
        {
            Bundle = mod.GetPacks<AssetBundleModPack>().SelectMany(e => e.AssetBundles).First();

            SetupMenu();

            SetupBasicTracker();

            BinID = AddGameDataObject<TrackerBin>().ID;
            SpotID = AddGameDataObject<TrackerSpot>().ID;
        }

        #region Views
        private void SetupBasicTracker()
        {
            BasicTracker = AddViewType("Basic Tracker", () =>
            {
                var prefab = GetPrefab("Basic Tracker");
                var view = prefab.TryAddComponent<BasicTrackerView>();
                return prefab;
            });
        }

        private void SetupAdvancedTracker()
        {

        }
        #endregion

        #region Menu
        private void SetupMenu()
        {
            PrefManager = new(GUID, "Automation Tracker");

            PrefManager
                .AddLabel("Time Increment")
                .AddOption("Increment", 1f,
                new float[] { 0.5f, 1f, 2f, 3f },
                new string[] { "30 Seconds", "1 Minute", "2 Minutes", "3 Minutes" })
                /*.AddLabel("Save JSON")
                .AddOption("JSON", false, new bool[] { false, true }, new string[] { "Disabled", "Enabled" })*/
                .AddSpacer()
                //.AddButton("Open Panel", _ => { })
                .AddConditionalBlocker(() => !TrackerHandler.CanAccessTracker())
                .AddButton("Create Tracker", _ => TrackerHandler.RequestTrackers())
                .AddButton("Delete Tracker", _ => TrackerHandler.DestroyTrackers());

            PrefManager.RegisterMenu(MenuType.PauseMenu);
        }
        #endregion

        #region Generic Util
        public static GameObject GetPrefab(string name) => Bundle.LoadAsset<GameObject>(name);
        #endregion
    }
}
