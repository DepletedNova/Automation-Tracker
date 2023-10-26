using Kitchen;
using KitchenTracker.Components;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;

namespace KitchenTracker.Systems
{
    public class CreateTrackerBlueprints : NightSystem
    {
        private static readonly int[] RequiredIDs = new int[]
        {
            SpotID,
            BinID
        };

        private EntityQuery Blueprints;
        protected override void Initialise()
        {
            base.Initialise();
            Blueprints = GetEntityQuery(new QueryHelper().All(typeof(CApplianceBlueprint), typeof(CTrackerBlueprint)).None(typeof(CItemTracker)));
            RequireSingletonForUpdate<STrackerEnabled>();
        }

        protected override void OnUpdate()
        {
            using var blueprints = Blueprints.ToComponentDataArray<CApplianceBlueprint>(Allocator.Temp);

            if (blueprints.Length >= RequiredIDs.Length)
                return;

            List<int> ids = RequiredIDs.ToList();
            for (int i = 0; i < blueprints.Length; i++)
            {
                var bp = blueprints[i];
                if (ids.Contains(bp.Appliance))
                    ids.Remove(bp.Appliance);
            }

            var EC = new EntityContext(EntityManager);
            for (int i = 0; i < ids.Count; i++)
                Set<CTrackerBlueprint>(PostHelpers.CreateOpenedLetter(EC, GetPostTiles().ElementAtOrDefault(i), ids[i], force_price: 0));
        }
    }
}
