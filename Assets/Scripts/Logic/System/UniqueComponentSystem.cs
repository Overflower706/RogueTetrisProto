using OVFL.ECS;

namespace Minomino
{
    public class UniqueComponentSystem : ISetupSystem
    {
        public Context Context { get; set; }
        public void Setup()
        {
            var gameEntity = Context.CreateEntity();
            gameEntity.AddComponent<GameStateComponent>();
            gameEntity.AddComponent<ScoreComponent>();
            gameEntity.AddComponent<BoardComponent>();

            var playerEntity = Context.CreateEntity();
            playerEntity.AddComponent<PlayerComponent>();
            playerEntity.AddComponent<HoldQueueComponent>();

            var tetriminoQueueEntity = Context.CreateEntity();
            tetriminoQueueEntity.AddComponent<TetrominoQueueComponent>();

            var commandRequestEntity = Context.CreateEntity();
            commandRequestEntity.AddComponent<CommandRequestComponent>();

            var notifyQueueEntity = Context.CreateEntity();
            notifyQueueEntity.AddComponent<NotifyQueueComponent>();
        }
    }
}