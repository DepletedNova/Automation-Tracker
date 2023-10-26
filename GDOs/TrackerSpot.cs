using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.Utils;
using KitchenTracker.Components;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenTracker.GDOs
{
    public class TrackerSpot : CustomAppliance
    {
        public override string UniqueNameID => "TrackerSpot";
        public override GameObject Prefab => GetPrefab("Tracker Spot");
        public override PriceTier PriceTier => PriceTier.Free;
        public override OccupancyLayer Layer => OccupancyLayer.Floor;
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, LocalisationUtils.CreateApplianceInfo("Tracking Spot", "Place under an appliance to track items passing over it", 
                new() {
                    new() { Description = "Interact to adjust UI positioning in the Preparation phase" },
                    new() { Description = "Interact during the day to reset the data" },
                    new() { Title = "Portionable", Description = "Can be tuned to correctly track portionable items whenever a portionable item is placed over it" },
                }, 
                new()))
        };

        public override List<IApplianceProperty> Properties => new()
        {
            new CItemTracker() { UpdateAfterCount = 1 },
            new CFixedRotation(),
        };

        public override void SetupPrefab(GameObject prefab)
        {
            prefab.ApplyMaterialToChild("Spot", "Glowing Blue Soft");
        }
    }
}
