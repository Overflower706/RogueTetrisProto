using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class MinoSystem : ITickSystem
    {
        public Context Context { get; set; }

        public void Tick()
        {
            var completedLineEntities = Context.GetEntitiesWithComponent<CompletedLineComponent>();
            if (completedLineEntities.Count > 0)
            {
                var board = GetBoard();
                // board에서 해당 height에 해당하는 모든 int 값을 가져온다.
                // 해당 int 값은 Mino Entity의 ID이다.
                var completedLineComponent = completedLineEntities[0].GetComponent<CompletedLineComponent>();
                // 왼쪽에서부터 순서대로 가져와서, MinoComponent의 State를 Linving으로 바꾼다.
                foreach (var line in completedLineComponent.CompletedLine)
                {
                    for (int x = 0; x < BoardComponent.WIDTH; x++)
                    {
                        int entityID = board.Board[x, line];
                        if (entityID != 0)
                        {
                            var minoEntity = FindEntityByID(entityID);
                            var minoComponent = minoEntity.GetComponent<MinoComponent>();
                            minoComponent.State = MinoState.Living;

                            // 이제 notify 보내기만 하면된다.
                            Debug.Log($"높이 {line}의 Mino ID {entityID}의 상태를 Living으로 변경했습니다.");
                        }
                    }
                }
            }
        }

        private BoardComponent GetBoard()
        {
            var boardEntities = Context.GetEntitiesWithComponent<BoardComponent>();
            if (boardEntities.Count == 0)
            {
                Debug.LogWarning("BoardComponent가 있는 엔티티가 없습니다.");
                return null;
            }
            else if (boardEntities.Count > 1)
            {
                Debug.LogWarning("BoardComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return null;
            }

            return boardEntities[0].GetComponent<BoardComponent>();
        }

        private Entity FindEntityByID(int id)
        {
            var entities = Context.GetEntitiesWithComponent<MinoComponent>();
            foreach (var entity in entities)
            {
                var minoComponent = entity.GetComponent<MinoComponent>();
                if (minoComponent.ParentID == id)
                {
                    return entity;
                }
            }
            return null;
        }
    }
}