using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class NotifySystem : ITickSystem, ICleanupSystem
    {
        public Context Context { get; set; }

        public void Tick()
        {
            var queueEntities = Context.GetEntitiesWithComponent<NotifyQueueComponent>();
            if (queueEntities.Count == 0)
            {
                Debug.LogWarning("NotifyQueueComponent가 있는 엔티티가 없습니다.");
                return; // 알림 큐가 없으면 아무것도 하지 않음
            }
            else if (queueEntities.Count > 1)
            {
                Debug.LogWarning("NotifyQueueComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return; // 여러 엔티티가 있으면 경고 후 종료
            }

            var notifyQueue = queueEntities[0].GetComponent<NotifyQueueComponent>();

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