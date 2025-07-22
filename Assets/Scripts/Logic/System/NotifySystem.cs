using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class NotifySystem : ITickSystem, ICleanupSystem
    {
        public Context Context { get; set; }

        public void Tick()
        {
            var notifyQueue = Context.GetNotifyQueue();

            // 한 틱당 하나의 알림만 처리 (순서 보장)
            if (notifyQueue.Notifies.Count > 0)
            {
                var notify = notifyQueue.Notifies.Dequeue();

                // 알림을 엔티티로 생성하여 다른 시스템들이 소비할 수 있도록 함
                var notifyEntity = Context.CreateEntity();
                notifyEntity.AddComponent(new NotifyMarkerComponent());
                AddNotifyComponent(notify, notifyEntity);
            }
        }

        public void Cleanup()
        {
            var notifyEntities = Context.GetEntitiesWithComponent<NotifyMarkerComponent>();
            foreach (var entity in notifyEntities)
            {
                Context.DestroyEntity(entity);
            }
        }

        private void AddNotifyComponent(Notify notify, Entity entity)
        {
            switch (notify.Type)
            {
                default:
                    break;
            }
        }
    }
}