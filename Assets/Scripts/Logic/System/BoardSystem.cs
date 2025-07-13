using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class BoardSystem : ITickSystem
    {
        public Context Context { get; set; }

        public void Tick(Context context)
        {
            var board = GetBoardComponent();

            var commandEntities = context.GetEntitiesWithComponent<StartGameCommand>();
            if (commandEntities.Count > 0)
            {
                // 초기 보드 설정 로직
                board.Board = new int[10, 20]; // 예시로 10x20 보드
                for (int x = 0; x < 10; x++)
                {
                    for (int y = 0; y < 20; y++)
                    {
                        board.Board[x, y] = 0; // 빈 칸을 0으로 초기화
                    }
                }
                Debug.Log("게임 시작, 보드 초기화 완료");
            }
        }

        private BoardComponent GetBoardComponent()
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
    }
}