using Kitchen;
using KitchenTracker.Components;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace KitchenTracker.Systems
{
    [UpdateBefore(typeof(PushItems))]
    [UpdateBefore(typeof(GrabItems))]
    public class TrackItems : GameSystemBase
    {
        private Dictionary<int, Entity> ItemTrackers = new();

        private EntityQuery Trackers;
        private EntityQuery TrackedItems;
        protected override void Initialise()
        {
            base.Initialise();
            Trackers = GetEntityQuery(typeof(CItemTracker), typeof(CPosition));
            TrackedItems = GetEntityQuery(typeof(CTrackedItem), typeof(CTrackedDay));

            RequireForUpdate(Trackers);
            RequireSingletonForUpdate<STrackingItems>();
        }

        protected override void OnUpdate()
        {
            using var trackerEntities = Trackers.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < trackerEntities.Length; i++)
            {
                var tracker = trackerEntities[i];
                var cTracker = GetComponent<CItemTracker>(tracker);
                var cPos = GetComponent<CPosition>(tracker);

                var occupant = GetOccupant(cPos.Position);
                if (occupant == Entity.Null || !Require(occupant, out CItemHolder holder) || 
                    holder.HeldItem == Entity.Null || !Require(holder, out CItem cItem))
                {
                    cTracker.FullHolder = false;
                    Set(tracker, cTracker);
                    continue;
                }

                if (cTracker.FullHolder)
                    continue;

                if (cTracker.DestroyItem)
                    EntityManager.DestroyEntity(holder.HeldItem);
                else
                {
                    cTracker.FullHolder = true;
                    Set(tracker, cTracker);
                }

                Entity itemTracker = Entity.Null;
                if (!ItemTrackers.TryGetValue(cItem.ID, out itemTracker))
                {
                    using (var items = TrackedItems.ToEntityArray(Allocator.Temp))
                    {
                        for (int i2 = 0; i2 < items.Length; i2++)
                        {
                            var item = items[i2];
                            var cTrackedItem = GetComponent<CTrackedItem>(item);
                            if (cTrackedItem.Item != cItem.ID || !cTrackedItem.Components.IsEquivalent(cItem.Items))
                                continue;

                            itemTracker = item;
                            break;
                        }
                    }

                    if (itemTracker == Entity.Null)
                    {
                        itemTracker = EntityManager.CreateEntity();
                        Set(itemTracker, new CTrackedItem
                        {
                            Item = cItem.ID,
                            Components = cItem.Items,
                            TrackedItems = 1,
                        });
                        EntityManager.AddBuffer<CTrackedDay>(itemTracker);
                        continue;
                    }
                }

                if (itemTracker == Entity.Null)
                    continue;

                var cTracked = GetComponent<CTrackedItem>(itemTracker);
                cTracked.TrackedItems++;
                Set(itemTracker, cTracked);
            }
        }
    }
}
