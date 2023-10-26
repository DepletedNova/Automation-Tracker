using Kitchen;
using KitchenTracker.Components;
using Unity.Collections;
using Unity.Entities;

namespace KitchenTracker.Systems
{
    public class UpdateTrackedAverage : GameSystemBase
    {
        private EntityQuery Trackers;
        protected override void Initialise()
        {
            base.Initialise();
            Trackers = GetEntityQuery(typeof(CItemTracker));
            RequireSingletonForUpdate<SGameTime>();
        }

        protected override void OnUpdate()
        {
            using var trackers = Trackers.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < trackers.Length; i++)
            {
                var tracker = trackers[i];
                var cTracker = GetComponent<CItemTracker>(tracker);
                if (cTracker.CountForUpdate < cTracker.UpdateAfterCount)
                    continue;

                var gameTime = GetSingleton<SGameTime>();

                cTracker.CountForUpdate = 0;

                if (cTracker.StartTime == 0)
                {
                    cTracker.StartTime = gameTime.TotalTime;
                    Set(tracker, cTracker);
                    continue;
                }

                cTracker.Average = (cTracker.TotalCount - cTracker.UpdateAfterCount) / (gameTime.TotalTime - cTracker.StartTime);
                Set(tracker, cTracker);
            }
        }
    }
}
