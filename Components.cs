using KitchenData;
using Unity.Entities;

namespace KitchenTracker.Components
{
    public struct CTrackerDisplay : IComponentData { }
    public struct CAdvancedTrackerDisplay : IComponentData { }

    public struct STrackingItems : IComponentData
    {
        public float Ticker;
    }

    public struct CTrackedItem : IComponentData
    {
        public int Item;
        public ItemList Components;
        public float TimeSinceRefresh;
        public int TrackedItems;
    }

    [InternalBufferCapacity(8)]
    public struct CTrackedDay : IBufferElementData
    {
        public int Day;
        public int ItemCount;
        public float Average;
    }

    public struct CItemTracker : IApplianceProperty, IComponentData
    {
        public bool FullHolder;
        public bool DestroyItem;
    }

    public struct CPracticeOnly : IApplianceProperty, IComponentData { }

    public struct CTrackerBlueprint : IAttachableProperty, IComponentData { }
}
