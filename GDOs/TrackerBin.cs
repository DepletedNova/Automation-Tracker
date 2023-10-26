using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.Utils;
using KitchenTracker.Components;
using System.Collections.Generic;
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
            (Locale.English, LocalisationUtils.CreateApplianceInfo("Tracking Bin", "Use at the end of a production line. Destroys items.", 
                new() {
                    new() { Description = "Interact to adjust UI positioning in the Preparation phase" },
                    new() { Description = "Interact during the day to reset the data" },
                    new() { Title = "Portionable", Description = "Place a portionable item to correctly track portions" },
                    new() { Title = "Tester", Description = "Can only be used in Practice Mode" },
                }, 
                new()))
        };

        public override List<IApplianceProperty> Properties => new()
        {
            new CItemHolder(),
            new CItemTracker() { DestroyItem = true, UpdateAfterCount = 1 },
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
