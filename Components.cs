using KitchenData;
using Unity.Entities;

namespace KitchenTracker.Components
{
    // Displays
    public struct CTrackerDisplay : IComponentData
    {
        public TrackerPosition Side;
    }
    public struct CItemTrackerIndicator : IComponentData { }

    // Singletons
    public struct STrackerEnabled : IComponentData
    {
        public int MaxID;
    }

    // Appliance Properties
    public struct CItemTracker : IApplianceProperty
    {
        public bool DestroyItem;

        public bool HeldItem;

        public int Item;
        public ItemList Components;

        public int UpdateAfterCount;
        public int CountForUpdate;

        public float StartTime;

        public float Average;
        public uint TotalCount;
    }
    public struct CItemTrackerID : IComponentData
    {
        public int ID;
        public TrackerPosition Display;
    }
    public struct CPracticeOnly : IApplianceProperty, IComponentData { }
    public struct CTrackerBlueprint : IAttachableProperty, IComponentData { }

    // Enums
    public enum TrackerPosition
    {
        None,
        Personal,
        Right,
        Left
    }
}
