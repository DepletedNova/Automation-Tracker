using Kitchen;
using KitchenTracker.Components;
using Unity.Collections;
using Unity.Entities;

namespace KitchenTracker.Systems
{
    public class RemoveExcessTrackingData : StartOfDaySystem
    {
        private EntityQuery Trackers;
        protected override void Initialise()
        {
            Trackers = GetEntityQuery(typeof(CTrackedDay));

            base.Initialise();
            RequireSingletonForUpdate<STrackingItems>();
        }

        protected override void OnUpdate()
        {
            using var trackers = Trackers.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < trackers.Length; i++)
            {
                var tracker = trackers[i];
                var buffer = GetBuffer<CTrackedDay>(tracker);

                if (buffer.Length > 5)
                {
                    var lowestDay = 9999;
                    var lowestIndex = 0;
                    for (int i2 = 0; i2 < buffer.Length; i2++)
                    {
                        if (buffer[i2].Day < lowestDay)
                        {
                            lowestDay = buffer[i2].Day;
                            lowestIndex = i2;
                        }
                    }

                    buffer.RemoveAt(lowestIndex);
                }
            }
        }
    }
}
