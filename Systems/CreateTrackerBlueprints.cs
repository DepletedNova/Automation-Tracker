using Kitchen;
using KitchenData;
using KitchenTracker.Components;
using System.Linq;
using Unity.Collections;
using Unity.Entities;

namespace KitchenTracker.Systems
{
    public class CreateTrackerBlueprints : NightSystem
    {
        private EntityQuery Blueprints;
        protected override void Initialise()
        {
            base.Initialise();
            Blueprints = GetEntityQuery(new QueryHelper().All(typeof(CTrackerBlueprint), typeof(CApplianceBlueprint)).None(typeof(CItemTracker)));

            RequireSingletonForUpdate<STrackingItems>();
        }

        protected override void OnUpdate()
        {
            using var blueprints = Blueprints.ToComponentDataArray<CApplianceBlueprint>(Allocator.Temp);
            if (blueprints.Length < 2)
            {
                RequiredBlueprint type = blueprints.Length < 1 ? RequiredBlueprint.Both : blueprints.First().Appliance == SpotID ? RequiredBlueprint.Bin : RequiredBlueprint.Spot;

                var EC = new EntityContext(EntityManager);

                Set<CTrackerBlueprint>(PostHelpers.CreateOpenedLetter(EC, GetPostTiles()[0], type == RequiredBlueprint.Both || type == RequiredBlueprint.Bin ? BinID : SpotID));
                if (type == RequiredBlueprint.Both)
                    Set<CTrackerBlueprint>(PostHelpers.CreateOpenedLetter(EC, GetPostTiles()[1], SpotID));
            }
        }

        private enum RequiredBlueprint
        {
            Both,
            Bin,
            Spot
        }
    }
}
