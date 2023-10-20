using Kitchen;
using KitchenTracker.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenTracker.Systems
{
    public class TrackerHandler : GenericSystemBase
    {
        public EntityQuery TrackerEntities;

        private static TrackerHandler instance;
        protected override void Initialise()
        {
            instance = this;

            TrackerEntities = GetEntityQuery(new QueryHelper()
                .Any(typeof(CTrackerDisplay), typeof(CAdvancedTrackerDisplay), typeof(CTrackedItem), typeof(CItemTracker), typeof(CTrackerBlueprint)));
        }
        protected override void OnUpdate() { }

        public static bool CanAccessTracker() => instance != null;

        #region Request
        public static void RequestTrackers() 
        {
            if (instance != null)
            {
                instance._requestTracker();
            }
        }
        public void _requestTracker()
        {
            if (Has<STrackingItems>() ||
                !Has<SIsNightTime>() || !Has<SKitchenMarker>() || !Has<SGameplayMarker>())
                return;

            Set(new STrackingItems { Ticker = 60f / UpdatesPerMinute });

            var tracker = EntityManager.CreateEntity();
            Set<CTrackerDisplay>(tracker);
            Set<CPosition>(tracker, new Vector3(1f, 0f, 0f));
            Set(tracker, new CRequiresView { Type = BasicTracker, ViewMode = ViewMode.Screen });
        }
        #endregion

        #region Destroy
        public static void DestroyTrackers()
        {
            if (instance != null)
            {
                instance._destroyTrackers();
            }
        }
        public void _destroyTrackers()
        {
            if (!Has<STrackingItems>() ||
                !Has<SIsNightTime>())
                return;

            Clear<STrackingItems>();

            EntityManager.DestroyEntity(TrackerEntities);
        }
        #endregion

    }
}
