using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.Utils;
using KitchenTracker.Components;
using KitchenTracker.Utility;
using KitchenTracker.Views;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace KitchenTracker.GDOs
{
    public class TrackerBin : CustomAppliance
    {
        public override string UniqueNameID => "TrackerBin";
        public override GameObject Prefab => GetPrefab("Tracker Bin");
        public override PriceTier PriceTier => PriceTier.Free;
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, LocalisationUtils.CreateApplianceInfo("Benchmarker", "End of the line!", 
                new() { 
                    new() { Title = "Line Ender", Description = "Place at the end of a production line to track the items put in" },
                    new() { Title = "Practice", Description = "Can only be used in Practice Mode to test maximum benchmarks" } 
                }, 
                new()))
        };

        public override List<IApplianceProperty> Properties => new()
        {
            new CItemHolder(),
            new CItemTracker() { DestroyItem = true },
            new CPracticeOnly(),
            new CFixedRotation(),
        };

        public override void SetupPrefab(GameObject prefab)
        {
            prefab.TryAddComponent<HoldPointContainer>().HoldPoint = prefab.transform.Find("HoldPoint");

            prefab.ApplyMaterialToChild("Removal", "Glowing Blue Soft");
            prefab.ApplyMaterialToChild("Spot", "Glowing Blue Soft");
        }
    }
}
