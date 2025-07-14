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

            var playerEntiy = Context.CreateEntity();
            playerEntiy.AddComponent<PlayerComponent>();

            var commandReuqestEntty = Context.CreateEntity();
            commandReuqestEntty.AddComponent<CommandRequestComponent>();
        }
    }
}