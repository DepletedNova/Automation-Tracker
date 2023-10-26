using Kitchen;
using KitchenTracker.Components;
using System;
using System.Collections.Generic;

namespace KitchenTracker.Systems
{
    public class SwapTrackerUI : ApplianceInteractionSystem
    {
        private CItemTrackerID cID;
        protected override bool IsPossible(ref InteractionData data) =>
            Has<STrackerEnabled>() && Require(data.Target, out cID);

        protected override void Perform(ref InteractionData data)
        {
            var positions = new List<TrackerPosition>((TrackerPosition[])Enum.GetValues(typeof(TrackerPosition)));
            var index = positions.IndexOf(cID.Display);
            index = (index + 1) % positions.Count;
            cID.Display = positions[index];
            Set(data.Target, cID);
        }
    }
}
