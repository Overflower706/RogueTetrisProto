using UnityEngine;
using OVFL.ECS;

namespace Minomino
{
    public static class ContextExtensions
    {
        /// <summary>
        /// 짱짱 익스텐션
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static T GetUniqueComponent<T>(this Context context) where T : class, IComponent
        {
            var componentEntities = context.GetEntitiesWithComponent<T>();
            if (componentEntities.Count == 0)
            {
                throw new System.Exception($"{typeof(T).Name}가 Context에 없습니다.");
            }
            if (componentEntities.Count > 1)
            {
                throw new System.Exception($"{typeof(T).Name}가 Context에 여러 개 있습니다.");
            }
            return componentEntities[0].GetComponent<T>();
        }

        public static GameStateComponent GetGameState(this Context context)
        {
            return context.GetUniqueComponent<GameStateComponent>();
        }

        public static ScoreComponent GetScore(this Context context)
        {
            return context.GetUniqueComponent<ScoreComponent>();
        }

        public static BoardComponent GetBoard(this Context context)
        {
            return context.GetUniqueComponent<BoardComponent>();
        }

        public static PlayerComponent GetPlayer(this Context context)
        {
            return context.GetUniqueComponent<PlayerComponent>();
        }

        public static TetriminoQueueComponent GetTetriminoQueue(this Context context)
        {
            return context.GetUniqueComponent<TetriminoQueueComponent>();
        }

        public static HoldQueueComponent GetHoldQueue(this Context context)
        {
            return context.GetUniqueComponent<HoldQueueComponent>();
        }

        public static CommandRequestComponent GetCommandRequest(this Context context)
        {
            return context.GetUniqueComponent<CommandRequestComponent>();
        }

        public static NotifyQueueComponent GetNotifyQueue(this Context context)
        {
            return context.GetUniqueComponent<NotifyQueueComponent>();
        }

        /// <summary>
        /// Find an entity by its ID in the context.
        /// </summary>
        public static Entity FindEntityByID(this Context context, int id)
        {
            var entities = context.GetEntities();
            foreach (var entity in entities)
            {
                if (entity.ID == id)
                {
                    return entity;
                }
            }

            Debug.LogWarning($"ID {id}에 해당하는 엔티티를 Context에서 찾을 수 없습니다.");
            return null;
        }
    }
}