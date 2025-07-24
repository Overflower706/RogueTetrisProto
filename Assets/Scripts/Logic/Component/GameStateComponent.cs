using OVFL.ECS;

namespace Minomino
{
    public class GameStateComponent : IComponent
    {
        public GameState CurrentState;
        public float GameTime;
    }


    public enum GameState
    {
        None,
        Initial,
        Playing,
        GameOver,
        Victory,
        Reward
    }
}