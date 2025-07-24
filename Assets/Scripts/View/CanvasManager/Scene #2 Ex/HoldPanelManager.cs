using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class HoldPanelManager : MonoBehaviour, ITickSystem
    {
        public Context Context { get; set; }

        [Header("홀드 블루프린트")]
        [SerializeField] private BlueprintView holdBlueprint;

        public void Tick()
        {
            if (!gameObject.activeSelf) return;

            var state = Context.GetGameState();

            if (state.CurrentState != GameState.Playing) return;

            var holdQueueComponent = Context.GetHoldQueue();

            if (holdQueueComponent.HoldQueue.Count == 0)
            {
                holdBlueprint.Clear();
                return;
            }

            var hold = holdQueueComponent.HoldQueue.Peek().GetComponent<TetrominoComponent>();
            holdBlueprint.Refresh(Context, hold);
        }
    }
}