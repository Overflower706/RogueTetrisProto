using OVFL.ECS;

namespace Minomino
{
    public class DataSystem : ISetupSystem
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

            var commandRequestEntity = Context.CreateEntity();
            commandRequestEntity.AddComponent<CommandRequestComponent>();
        }
    }
}