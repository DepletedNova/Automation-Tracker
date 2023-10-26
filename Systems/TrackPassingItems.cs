using Kitchen;
using KitchenData;
using KitchenTracker.Components;
using System.Linq;
using Unity.Collections;
using Unity.Entities;

namespace KitchenTracker.Systems
{
    public class TrackPassingItems : GameSystemBase
    {
        private EntityQuery Trackers;
        protected override void Initialise()
        {
            base.Initialise();
            Trackers = GetEntityQuery(typeof(CItemTracker), typeof(CPosition));
        }

        protected override void OnUpdate()
        {
            using var trackers = Trackers.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < trackers.Length; i++)
            {
                var tracker = trackers[i];
                var cTracker = GetComponent<CItemTracker>(tracker);
                if (cTracker.HeldItem)
                    continue;

                var occupant = GetOccupant(GetComponent<CPosition>(tracker).Position);
                if (occupant == Entity.Null || !Require(occupant, out CItemHolder cHolder) || cHolder.HeldItem == Entity.Null || !Require(cHolder, out CItem cItem))
                {
                    cTracker.HeldItem = false;
                    Set(tracker, cTracker);
                    continue;
                }

                if (Require(cHolder, out CSplittableItem splittable))
                {
                    var count = splittable.TotalCount;
                    if (GameData.Main.TryGet(cItem.ID, out Item item))
                        count += item.SplitDepletedItems.Count(p => p.ID == splittable.SubItem);

                    cTracker.HeldItem = true;
                    cTracker.UpdateAfterCount = count;
                    cTracker.CountForUpdate = 0;
                    cTracker.Average = 0;
                    cTracker.StartTime = 0;
                    Set(tracker, cTracker);
                    continue;
                }

                if (cTracker.DestroyItem)
                    EntityManager.DestroyEntity(cHolder);
                else cTracker.HeldItem = true;
                
                cTracker.Item = cItem.ID;
                cTracker.Components = cItem.Items;
                cTracker.CountForUpdate++;
                cTracker.TotalCount++;
                Set(tracker, cTracker);
            }
        }
    }
}
