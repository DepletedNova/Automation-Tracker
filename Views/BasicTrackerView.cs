using Kitchen;
using KitchenData;
using KitchenLib.Utils;
using KitchenTracker.Components;
using KitchenTracker.Utility;
using MessagePack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Serialization;
using UnityEngine;

namespace KitchenTracker.Views
{
    public class BasicTrackerView : UpdatableObjectView<BasicTrackerView.ViewData>
    {
        private static readonly int Image = Shader.PropertyToID("_Image");

        private List<TrackedItem> Items = new();
        private List<TrackerUI> Trackers = new();

        public override void Initialise()
        {
            base.Initialise();
            for (int i = 0; i < gameObject.GetChildCount(); i++)
            {
                TrackerUI tracker = new TrackerUI();
                var child = tracker.Object = gameObject.GetChild(i);

                child.ApplyMaterialToChild("Panel", "UI Panel - Help");
                tracker.Renderer = child.ApplyMaterialToChild("Item", "UI Panel").ApplyMaterialToChild("Image", "Flat Image").GetComponent<MeshRenderer>();

                child.transform.CreateLabel("Average Label", new(0, 0.5f, -0.01f), Quaternion.identity,
                    MaterialUtils.GetExistingMaterial("Alphakind Atlas Material"), FontUtils.GetExistingTMPFont("Large Text"), 0, 1.75f,
                    "Average");
                tracker.Average = child.transform.CreateLabel("Average", new(0, 0.2f, -0.01f), Quaternion.identity,
                    MaterialUtils.GetExistingMaterial("Alphakind Atlas Material"), FontUtils.GetExistingTMPFont("Large Text"), 0, 2.25f,
                    "1.00/m").GetComponent<TextMeshPro>();

                child.transform.CreateLabel("Count Label", new(0, -0.2f, -0.01f), Quaternion.identity,
                    MaterialUtils.GetExistingMaterial("Alphakind Atlas Material"), FontUtils.GetExistingTMPFont("Large Text"), 0, 1.75f,
                    "Item Count");
                tracker.Count = child.transform.CreateLabel("Count", new(0, -0.5f, -0.01f), Quaternion.identity,
                    MaterialUtils.GetExistingMaterial("Alphakind Atlas Material"), FontUtils.GetExistingTMPFont("Large Text"), 0, 2.25f,
                    "512").GetComponent<TextMeshPro>();

                Trackers.Add(tracker);
            }
        }

        protected override void UpdateData(ViewData data)
        {
            if (data.Items.Equals(Items))
                return;

            for (int i = 0; i < Trackers.Count; i++)
            {
                var tracker = Trackers[i];
                if (i >= data.Items.Count || !GameData.Main.TryGet(data.Items[i].Item, out Item item))
                {
                    tracker.Object.SetActive(false);
                    continue;
                }
                var info = data.Items[i];

                var inc = PrefManager.Get<float>("Increment");
                var avg = info.Average * inc;
                string incStr = inc < 1f ? $"{Mathf.Ceil(60f * inc)}sec" : inc == 1 ? "min" : $"{inc}min";

                tracker.Object.SetActive(true);
                tracker.Average.text = $"{avg.ToString("0.00")}/{incStr}";
                tracker.Count.text = info.Count.ToString();

                if (i >= Items.Count || Items[i].Item != info.Item)
                    continue;

                tracker.Renderer.material.SetTexture(Image, GetSnapshot(item.Prefab, info.Item, info.Components));
            }

            Items = data.Items;
        }

        private static readonly int Fade = Shader.PropertyToID("_NightFade");
        private static float NightFade;
        private static Texture2D GetSnapshot(GameObject prefab, int item, ItemList components)
        {
            if (prefab == null)
                return null;

            var gameObject = Instantiate(prefab);
            if (gameObject.TryGetComponent(out IItemSpecificView view))
                view.PerformUpdate(item, components);

            NightFade = Shader.GetGlobalFloat(Fade);
            Shader.SetGlobalFloat(Fade, 0f);

            Quaternion rotation = Quaternion.LookRotation(new Vector3(0f, -0.5f, 0.5f), Vector3.up);
            SnapshotTexture snapshotTexture = Snapshot.RenderPrefabToTexture(128, 128, gameObject, rotation, 0.5f, 0.5f, -10f, 10f, 1f, default);

            Shader.SetGlobalFloat(Fade, NightFade);

            Destroy(gameObject);

            return snapshotTexture.Snapshot;
        }

        public struct TrackerUI
        {
            public GameObject Object;
            public MeshRenderer Renderer;
            public TextMeshPro Average;
            public TextMeshPro Count;
        }


        [MessagePackObject]
        public struct TrackedItem : IEquatable<TrackedItem>
        {
            [Key(1)] public int Item;
            [Key(2)] public ItemList Components;
            [Key(3)] public float Average;
            [Key(4)] public int Count;

            public bool Equals(TrackedItem other) => 
                Item == other.Item && Components.IsEquivalent(other.Components) &&
                Average == other.Average && Count == other.Count;
        }

        [MessagePackObject]
        public struct ViewData : IViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(1)] public List<TrackedItem> Items;

            public bool IsChangedFrom(ViewData check) => !Items.Equals(check.Items);
        }

        private class UpdateView : IncrementalViewSystemBase<ViewData>
        {
            private EntityQuery Displays;
            private EntityQuery Trackers;

            protected override void Initialise()
            {
                base.Initialise();
                Displays = GetEntityQuery(typeof(CTrackerDisplay), typeof(CLinkedView));
                Trackers = GetEntityQuery(typeof(CTrackedItem), typeof(CTrackedDay));

                RequireForUpdate(Displays);
                RequireSingletonForUpdate<SDay>();
                RequireSingletonForUpdate<STrackingItems>();
            }

            protected override void OnUpdate()
            {
                List<TrackedItem> items = new();
                using (var trackers = Trackers.ToEntityArray(Allocator.Temp))
                {
                    var currentDay = GetSingleton<SDay>().Day;
                    for (int i = 0; i < trackers.Length; i++)
                    {
                        var tracker = trackers[i];
                        var trackedDays = GetBuffer<CTrackedDay>(tracker);
                        for (int i2 = 0; i2 < trackedDays.Length; i2++)
                        {
                            var trackedDay = trackedDays[i2];
                            if (trackedDay.Day == currentDay)
                            {
                                var cTracker = GetComponent<CTrackedItem>(tracker);
                                var item = new TrackedItem
                                {
                                    Average = trackedDay.Average,
                                    Count = trackedDay.ItemCount,
                                    Item = cTracker.Item,
                                    Components = cTracker.Components
                                };
                                items.Add(item);
                                break;
                            }
                        }

                    }
                }

                if (items.Count > 1)
                {
                    items.Sort((x, y) => x.Average > y.Average ? -1 : x.Average < y.Average ? 1 : 0);
                }

                using (var views = Displays.ToComponentDataArray<CLinkedView>(Allocator.Temp))
                {
                    for (int i = 0; i < views.Length; i++)
                    {
                        SendUpdate(views[i], new ViewData { Items = items });
                    }
                }
            }
        }
    }
}
