using Kitchen;
using KitchenTracker.Components;
using Unity.Collections;
using Unity.Entities;

namespace KitchenTracker.Systems
{
    public class AddTrackerIDs : GameSystemBase
    {
        private EntityQuery WithoutIDs;
        protected override void Initialise()
        {
            base.Initialise();
            WithoutIDs = GetEntityQuery(new QueryHelper().All(typeof(CItemTracker)).None(typeof(CItemTrackerID)));
            RequireSingletonForUpdate<STrackerEnabled>();
        }

        protected override void OnUpdate()
        {
            var sTracker = GetSingleton<STrackerEnabled>();
            using (var entities = WithoutIDs.ToEntityArray(Allocator.Temp))
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    sTracker.MaxID++;
                    var entity = entities[i];
                    Set(entity, new CItemTrackerID
                    {
                        ID = sTracker.MaxID,
                        Display = TrackerPosition.Personal
                    });
                }
            }
            Set(sTracker);
        }
    }
}
