using Kitchen;
using KitchenTracker.Components;
using Unity.Entities;

namespace KitchenTracker.Systems
{
    public class DestroyPracticeEntities : StartOfDaySystem
    {
        private EntityQuery Appliances;
        protected override void Initialise()
        {
            Appliances = GetEntityQuery(typeof(CPracticeOnly));

            base.Initialise();
        }

        protected override void OnUpdate()
        {
            if (!HasSingleton<SPracticeMode>())
                EntityManager.DestroyEntity(Appliances);
        }
    }
}
