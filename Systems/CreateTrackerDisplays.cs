using Kitchen;
using KitchenTracker.Components;
using Unity.Entities;
using UnityEngine;

namespace KitchenTracker.Systems
{
    public class CreateTrackerDisplays : GameSystemBase
    {
        private EntityQuery Query;
        protected override void Initialise()
        {
            base.Initialise();
            Query = GetEntityQuery(typeof(CTrackerDisplay));
            RequireSingletonForUpdate<STrackerEnabled>();
        }

        protected override void OnUpdate()
        {
            if (!Query.IsEmpty)
                return;

            CreateDisplaySide(TrackerPosition.Right);
            CreateDisplaySide(TrackerPosition.Left);
        }

        private void CreateDisplaySide(TrackerPosition side)
        {
            var display = EntityManager.CreateEntity();
            Set(display, new CTrackerDisplay { Side = side });
            Set(display, new CRequiresView { Type = TrackerDisplay, ViewMode = ViewMode.Screen });
            Set<CPosition>(display, new Vector3(side == TrackerPosition.Right ? 1f : 0f, 0f, 0f));
        }
    }
}
