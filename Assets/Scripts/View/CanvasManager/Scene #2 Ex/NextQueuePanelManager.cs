using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class NextQueuePanelManager : MonoBehaviour, ITickSystem
    {
        public Context Context { get; set; }

        [Header("다음 블루프린트")]
        [SerializeField] private BlueprintView nextBlueprint_1;
        [SerializeField] private BlueprintView nextBlueprint_2;
        [SerializeField] private BlueprintView nextBlueprint_3;

        public void Tick()
        {
            if (!gameObject.activeSelf) return;

            var state = Context.GetGameState();

            if (state.CurrentState != GameState.Playing) return;

            var NextQueueComponent = Context.GetTetrominoQueue();

            if (NextQueueComponent.TetrominoQueue.Count == 0)
            {
                nextBlueprint_1.Clear();
                nextBlueprint_2.Clear();
                nextBlueprint_3.Clear();
                return;
            }

            var queue = NextQueueComponent.TetrominoQueue.ToArray();
            nextBlueprint_1.Refresh(Context, queue.Length > 0 ? queue[0].GetComponent<TetrominoComponent>() : null);
            nextBlueprint_2.Refresh(Context, queue.Length > 1 ? queue[1].GetComponent<TetrominoComponent>() : null);
            nextBlueprint_3.Refresh(Context, queue.Length > 2 ? queue[2].GetComponent<TetrominoComponent>() : null);
        }
    }
}