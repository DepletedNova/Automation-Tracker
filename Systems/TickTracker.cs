using Kitchen;
using KitchenTracker.Components;
using Unity.Collections;
using Unity.Entities;

namespace KitchenTracker.Systems
{
    public class TickTracker : DaySystem
    {
        private EntityQuery Trackers;
        protected override void Initialise()
        {
            Trackers = GetEntityQuery(typeof(CTrackedDay), typeof(CTrackedItem));

            base.Initialise();
            RequireSingletonForUpdate<STrackingItems>();
        }

        protected override void OnUpdate()
        {
            using (var trackers = Trackers.ToEntityArray(Allocator.Temp))
            {
                for (int i = 0; i < trackers.Length; i++)
                {
                    var tracker = trackers[i];
                    var cTracked = GetComponent<CTrackedItem>(tracker);
                    cTracked.TimeSinceRefresh += Time.DeltaTime;
                    Set(tracker, cTracked);
                }
            }
        }
    }
}
