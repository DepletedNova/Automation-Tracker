using Kitchen;
using KitchenData;
using KitchenLib.Utils;
using KitchenTracker.Components;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenTracker.Views
{
    public class TrackerDisplayView : UpdatableObjectView<TrackerDisplayView.ViewData>
    {
        private static readonly int Image = Shader.PropertyToID("_Image");

        public GameObject BaseItem;
        public float Spacing;
        public Vector2 Offset;

        private List<TrackerItem> Items = new();

        private ViewData Data = default;

        public override void Initialise()
        {
            base.Initialise();
            BaseItem.SetActive(false);
        }

        protected override void UpdateData(ViewData data)
        {
            if (!data.IsChangedFrom(Data))
                return;

            Data = data;
            var side = Data.Position == TrackerPosition.Left ? 1f : -1f;

            // Check and remove excess data
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                var item = Items[i];
                if (Data.Items.Any(td => td.ID == item.Data.ID))
                    continue;
                Destroy(Items[i].Base);
                Items.RemoveAt(i);
            }

            var increment = PrefManager.Get<float>("Increment");
            var incText = increment == 1 ? "s" :
                increment < 60f ? $"{increment}s" :
                increment == 60f ? "m" :
                $"{increment / 60f}m";

            // Check, add, and update data
            for (int i = 0; i < Data.Items.Count; i++)
            {
                var dataItem = Data.Items[i];
                TrackerItem item = default;
                var index = Items.FindIndex(ti => ti.Data.ID == dataItem.ID);
                if (index != -1)
                    item = Items[index];
                else
                    item = CreateItem(dataItem);

                var imageTransform = item.Image.transform;
                imageTransform.localPosition = new(Mathf.Abs(imageTransform.localPosition.x) * side, 0f);

                if (data.ShowIDs)
                {
                    item.AverageLabel.SetActive(false);
                    item.Average.gameObject.SetActive(false);
                    item.Calculating.SetActive(false);
                    item.Image.gameObject.SetActive(false);

                    item.ID.gameObject.SetActive(true);
                    item.ID.text = dataItem.ID.ToString();
                }
                else
                {
                    item.ID.gameObject.SetActive(false);

                    if (dataItem.Average != 0f)
                    {
                        item.AverageLabel.SetActive(true);
                        item.Average.gameObject.SetActive(true);
                        item.Average.text = (increment * dataItem.Average).ToString("0.00") + '/' + incText;

                        item.Calculating.SetActive(false);
                    } else
                    {
                        item.AverageLabel.SetActive(false);
                        item.Average.gameObject.SetActive(false);
                        item.Calculating.SetActive(true);
                    }

                    if (dataItem.Item != 0)
                    {
                        if (!item.Image.gameObject.activeSelf && GameData.Main.TryGet(dataItem.Item, out Item gdo))
                        {
                            var snapshot = GetSnapshot(gdo.Prefab, dataItem.Item, dataItem.Components);
                            item.Image.material.SetTexture(Image, snapshot);
                            item.Image.gameObject.SetActive(true);
                        }
                    } else item.Image.gameObject.SetActive(false);
                }
            }

            // Sort items
            Items.Sort((x, y) => x.Data.ID > y.Data.ID ? 1 : x.Data.ID > y.Data.ID ? -1 : 0);
            for (int i = 0; i < Items.Count; i++)
                Items[i].Base.transform.localPosition = new(Offset.x * side, Offset.y + Spacing * i);
        }

        private TrackerItem CreateItem(TrackedData data)
        {
            var item = new TrackerItem() { Data = data };
            var side = Data.Position == TrackerPosition.Left ? 1f : -1f;

            var obj = Instantiate(BaseItem);
            obj.transform.SetParent(gameObject.transform, false);
            obj.transform.localPosition = new(Offset.x * side, Offset.y + Spacing * Items.Count);
            obj.SetActive(true);

            item.Base = obj;
            item.Image = obj.GetChild("Image").GetComponent<MeshRenderer>();

            var transform = item.Image.gameObject.transform;
            transform.localPosition = new(Mathf.Abs(transform.localPosition.x) * side, 0, 0);

            item.Calculating = obj.GetChild("Calculating");
            item.AverageLabel = obj.GetChild("Average Label");
            item.Average = obj.GetChild("Average").GetComponent<TextMeshPro>();
            item.ID = obj.GetChild("ID").GetComponent<TextMeshPro>();

            Items.Add(item);

            return item;
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

        private struct TrackerItem
        {
            public GameObject Base;
            public MeshRenderer Image;

            public GameObject Calculating;
            public GameObject AverageLabel;
            public TextMeshPro Average;
            public TextMeshPro ID;

            public TrackedData Data;
        }

        [MessagePackObject]
        public struct TrackedData : IEquatable<TrackedData>
        {
            [Key(1)] public int ID;
            [Key(2)] public float Average;
            [Key(3)] public int Item;
            [Key(4)] public ItemList Components;

            public bool Equals(TrackedData other) =>
                ID == other.ID && Average == other.Average &&
                Item == other.Item && Components.IsEquivalent(other.Components);
        }

        [MessagePackObject]
        public struct ViewData : IViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(1)] public TrackerPosition Position;
            [Key(2)] public bool ShowIDs;
            [Key(3)] public List<TrackedData> Items;

            public bool IsChangedFrom(ViewData check) => 
                Position != check.Position || ShowIDs != check.ShowIDs || !Items.Equals(check.Items);
        }

        private class UpdateView : IncrementalViewSystemBase<ViewData>
        {
            private EntityQuery Trackers;
            private EntityQuery Displays;
            protected override void Initialise()
            {
                base.Initialise();
                Trackers = GetEntityQuery(typeof(CItemTracker), typeof(CItemTrackerID));
                Displays = GetEntityQuery(typeof(CLinkedView), typeof(CTrackerDisplay));
                RequireSingletonForUpdate<STrackerEnabled>();
            }

            protected override void OnUpdate()
            {
                List<TrackedData> Right = new();
                List<TrackedData> Left = new();
                using (var trackers = Trackers.ToComponentDataArray<CItemTracker>(Allocator.Temp))
                {
                    using var IDs = Trackers.ToComponentDataArray<CItemTrackerID>(Allocator.Temp);
                    for (int i = 0; i < trackers.Length; i++)
                    {
                        var tracker = trackers[i];
                        var id = IDs[i];

                        if (id.Display != TrackerPosition.Right && id.Display != TrackerPosition.Left)
                            continue;

                        var data = new TrackedData { ID = id.ID };

                        if (tracker.Average != 0 && tracker.Item != 0)
                        {
                            data.Average = tracker.Average;
                            data.Item = tracker.Item;
                            data.Components = tracker.Components;
                        }

                        (id.Display == TrackerPosition.Right ? Right : Left).Add(data);
                    }
                }

                using var displays = Displays.ToComponentDataArray<CTrackerDisplay>(Allocator.Temp);
                using var views = Displays.ToComponentDataArray<CLinkedView>(Allocator.Temp);
                for (int i = 0; i < displays.Length; i++)
                {
                    var side = displays[i].Side;
                    SendUpdate(views[i], new()
                    {
                        Position = side,
                        ShowIDs = Has<SIsNightTime>(),
                        Items = side == TrackerPosition.Right ? Right : Left
                    });
                }
            }
        }

    }
}
