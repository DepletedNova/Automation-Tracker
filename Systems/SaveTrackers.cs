using Kitchen;
using KitchenLib.Utils;
using KitchenTracker.Components;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace KitchenTracker.Systems
{
    public class SaveTrackers : GameSystemBase
    {
        private static List<CItemTracker> Trackers = new();
        private static List<CPosition> TrackerPositions = new();

        private EntityQuery TrackerQuery;
        protected override void Initialise()
        {
            base.Initialise();
            TrackerQuery = GetEntityQuery(typeof(CItemTracker), typeof(CPosition));
            RequireSingletonForUpdate<SPracticeMode>();
        }

        protected override void OnUpdate()
        {
            using (var trackers = TrackerQuery.ToComponentDataArray<CItemTracker>(Allocator.Temp))
            {
                using var positions = TrackerQuery.ToComponentDataArray<CPosition>(Allocator.Temp);
                Trackers.Clear();
                TrackerPositions.Clear();
                for (int i = 0; i < positions.Length; i++)
                {
                    Trackers.Add(trackers[i]);
                    TrackerPositions.Add(positions[i]);
                }
            }
        }

        public override void AfterLoading(SaveSystemType system_type)
        {
            base.AfterLoading(system_type);

            if (Trackers.IsNullOrEmpty() || TrackerPositions.IsNullOrEmpty())
                return;

            var entities = TrackerQuery.ToEntityArray(Allocator.Temp);
            var positions = TrackerQuery.ToComponentDataArray<CPosition>(Allocator.Temp);
            var trackers = TrackerQuery.ToComponentDataArray<CItemTracker>(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                var index = TrackerPositions.FindIndex(p => (positions[i].Position - p.Position).Chebyshev() < 0.1f);
                if (index == -1)
                    continue;

                var tracker = trackers[index];
                tracker.UpdateAfterCount = Trackers[index].UpdateAfterCount;
                Set(entities[i], tracker);
            }

            entities.Dispose();
            positions.Dispose();
            trackers.Dispose();
            Trackers.Clear();
            TrackerPositions.Clear();
        }
    }
}
