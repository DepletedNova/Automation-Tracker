using Kitchen;
using KitchenTracker.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenTracker.Systems
{
    [UpdateBefore(typeof(ViewSystemsGroup))]
    public class UpdateTrackedDays : GameSystemBase
    {
        public EntityQuery TrackedDays;
        protected override void Initialise()
        {
            TrackedDays = GetEntityQuery(typeof(CTrackedDay), typeof(CTrackedItem));

            base.Initialise();
            RequireSingletonForUpdate<STrackingItems>();
            RequireSingletonForUpdate<SPracticeMode>();
            RequireSingletonForUpdate<SDay>();
        }

        protected override void OnUpdate()
        {
            var trackingItems = GetSingleton<STrackingItems>();

            trackingItems.Ticker -= Time.DeltaTime;

            if (trackingItems.Ticker > 0)
            {
                Set(trackingItems);
                return;
            }

            trackingItems.Ticker = 60f / UpdatesPerMinute;
            Set(trackingItems);

            using (var trackedItems = TrackedDays.ToEntityArray(Allocator.Temp))
            {
                var day = GetSingleton<SDay>().Day;
                for (int i = 0; i < trackedItems.Length; i++)
                {
                    var itemEntity = trackedItems[i];
                    var item = GetComponent<CTrackedItem>(itemEntity);
                    var buffer = GetBuffer<CTrackedDay>(itemEntity);

                    if (item.TrackedItems == 0)
                        continue;

                    var currentAverage = item.TrackedItems / Mathf.Max(item.TimeSinceRefresh, 1f) * 60f;

                    bool found = false;
                    for (int i2 = 0; i2 < buffer.Length; i2++)
                    {
                        if (buffer[i2].Day != day)
                            continue;
                        found = true;

                        CTrackedDay tracked = buffer[i2];

                        var spanningAverage = tracked.Average > 0.05f ? Mathf.Lerp(tracked.Average, currentAverage, 0.5f) : currentAverage;
                        tracked.Average = spanningAverage;

                        tracked.ItemCount += item.TrackedItems;
                        buffer[i2] = tracked;

                        break;
                    }

                    if (!found)
                    {
                        CTrackedDay tracked = new()
                        {
                            Day = day
                        };
                        tracked.ItemCount += item.TrackedItems;
                        buffer.Add(tracked);
                    }

                    item.TrackedItems = 0;
                    item.TimeSinceRefresh = 0;
                    Set(itemEntity, item);
                }
            }
            
        }
    }
}
