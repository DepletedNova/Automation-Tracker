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
    public class TrackerSpot : CustomAppliance
    {
        public override string UniqueNameID => "TrackerSpot";
        public override GameObject Prefab => GetPrefab("Tracker Spot");
        public override PriceTier PriceTier => PriceTier.Free;
        public override OccupancyLayer Layer => OccupancyLayer.Floor;
        public override List<(Locale, ApplianceInfo)> InfoList => new()
        {
            (Locale.English, LocalisationUtils.CreateApplianceInfo("Passer", "", 
                new() { 
                    new() { Title = "Benchmark", Description = "Placed under appliances to track items passing over them" } 
                }, 
                new()))
        };

        public override List<IApplianceProperty> Properties => new()
        {
            new CItemTracker(),
            new CFixedRotation(),
        };

        public override void SetupPrefab(GameObject prefab)
        {

            prefab.ApplyMaterialToChild("Spot", "Glowing Blue Soft");
        }
    }
}
