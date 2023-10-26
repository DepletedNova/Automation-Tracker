using Kitchen;
using KitchenTracker.Components;
using Unity.Entities;

namespace KitchenTracker.Systems
{
    internal class TrackingController : GameSystemBase
    {
        private static TrackingController _instance;

        private EntityQuery Query;
        protected override void Initialise()
        {
            base.Initialise();
            _instance = this;

            Query = GetEntityQuery(new QueryHelper().Any(
                    typeof(CTrackerBlueprint), typeof(CItemTracker),
                    typeof(CTrackerDisplay)
                ));
        }

        protected override void OnUpdate()
        {
            CleanTrackers();
        }

        // Creation & Deletion
        public static void EnableTracking(int _) => _instance._enableTracking();
        private void _enableTracking()
        {
            if (!Has<SGameplayMarker>() || !Has<SKitchenMarker>() || !Has<SIsNightTime>())
                return;

            Set<STrackerEnabled>();
        }

        public static void DisableTracking(int _) => _instance._disableTracking();
        private void _disableTracking() => Clear<STrackerEnabled>();

        // Info
        public static bool IsTracking() => _instance._isTracking();
        private bool _isTracking() => Has<STrackerEnabled>();

        public static bool CanModifyTrackers() => _instance._canModifyTrackers();
        private bool _canModifyTrackers() => Has<SKitchenMarker>() && Has<SIsNightTime>();

        // Clean up
        private void CleanTrackers()
        {
            if (Has<STrackerEnabled>() || Query.IsEmpty)
                return;

            EntityManager.DestroyEntity(Query);
        }
    }
}
