using OVFL.ECS;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Minomino
{
    public class TetriminoSystem : ITickSystem
    {
        public Context Context { get; set; }

        public void Tick(Context context)
        {
            var startEntities = context.GetEntitiesWithComponent<StartGameCommand>();
            if (startEntities.Count > 0)
            {
                // 게임 시작
                // SevenBag 시스템을 사용하여 Tetrimino 큐 생성
                Queue<TetriminoComponent> tetriminoQueue = GenerateSevenBagQueue();

                // TetriminoQueueComponent가 있는 엔티티 찾기 또는 생성
                var queueEntity = Context.CreateEntity();
                var queueComponent = queueEntity.AddComponent<TetriminoQueueComponent>();
                queueComponent.TetriminoQueue = new Queue<Entity>();

                // Queue 순서대로 CreateEntity().AddComponent해서,
                // Context에서 TetriminoQueueComponent.TetriminoQueue.enqueue(entity);
                foreach (var tetriminoComponent in tetriminoQueue)
                {
                    var tetriminoEntity = context.CreateEntity();
                    tetriminoEntity.AddComponent(tetriminoComponent);
                    queueComponent.TetriminoQueue.Enqueue(tetriminoEntity);
                }

                // 이후 첫 번째 Tetrimino를 꺼내서 CurrentTetrimino Component 부착
                var firstTetrimino = queueComponent.TetriminoQueue.Dequeue();
                var firstTetriminoComponent = firstTetrimino.GetComponent<TetriminoComponent>();
                var currentTetriminoComponent = firstTetrimino.AddComponent<CurrentTetriminoComponent>();
                currentTetriminoComponent.Position = new Vector2Int(BoardComponent.WIDTH / 2 - 1, BoardComponent.HEIGHT - 2);

                Debug.Log($"첫 번째 Tetrimino 생성, Type: {firstTetriminoComponent.Type}, Color: {firstTetriminoComponent.ColorValue}, Position: {currentTetriminoComponent.Position}");
                Debug.Log($"Tetrimino 생성 완료: {queueComponent.TetriminoQueue.Count}개 대기 중");
            }

            var generateCommandEntities = context.GetEntitiesWithComponent<GenerateTetriminoCommand>();
            if (generateCommandEntities.Count > 0)
            {
                // GenerateTetriminoCommand가 있는 경우, Tetrimino를 생성
                if (TryGenerateTetrimino())
                {
                    Debug.Log("새로운 Tetrimino 생성 완료");
                }
                else
                {
                    Debug.LogWarning("Tetrimino 생성 실패: 큐가 비어 있거나 큐 컴포넌트가 없음");
                }
            }
        }

        private bool TryGenerateTetrimino()
        {
            var queueComponent = GetTetriminoQueueComponent();
            if (queueComponent == null)
            {
                Debug.LogWarning("TetriminoQueueComponent가 없습니다. 큐를 생성할 수 없습니다.");
                return false; // 큐가 없으면 생성 실패
            }

            if (queueComponent.TetriminoQueue.Count == 0)
            {
                Debug.LogWarning("Tetrimino 큐가 비어 있습니다. 새로운 Tetrimino를 생성할 수 없습니다.");
                return false; // 큐가 비어 있으면 생성 실패
            }

            // 큐에서 Tetrimino를 꺼내서 현재 테트리미노로 설정
            var nextTetriminoEntity = queueComponent.TetriminoQueue.Dequeue();
            var nextTetriminoComponent = nextTetriminoEntity.GetComponent<TetriminoComponent>();
            var currentTetriminoComponent = nextTetriminoEntity.AddComponent<CurrentTetriminoComponent>();
            currentTetriminoComponent.Position = new Vector2Int(BoardComponent.WIDTH / 2 - 1, BoardComponent.HEIGHT - 2);

            return true;
        }

        private TetriminoQueueComponent GetTetriminoQueueComponent()
        {
            var queueEntities = Context.GetEntitiesWithComponent<TetriminoQueueComponent>();
            if (queueEntities.Count == 0)
            {
                Debug.LogWarning("TetriminoQueueComponent가 있는 엔티티가 없습니다.");
                return null; // 큐가 없으면 null 반환
            }
            else if (queueEntities.Count > 1)
            {
                Debug.LogWarning("TetriminoQueueComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return null; // 여러 엔티티가 있으면 경고 후 null 반환
            }
            return queueEntities[0].GetComponent<TetriminoQueueComponent>();
        }

        /// <summary>
        /// 28가지 Tetrimino Component를 모두 포함하여 무작위로 섞인 Queue를 생성하여 반환
        /// 7가지 타입 × 4가지 색상 = 28가지 조합을 모두 1개씩 포함
        /// </summary>
        /// <returns>28가지 조합이 무작위로 섞인 TetriminoComponent 큐</returns>
        private Queue<TetriminoComponent> GenerateRandomQueue()
        {
            List<TetriminoComponent> allCombinations = new List<TetriminoComponent>();

            // 7가지 타입
            TetriminoType[] types = { TetriminoType.I, TetriminoType.O, TetriminoType.T,
                                    TetriminoType.S, TetriminoType.Z, TetriminoType.J, TetriminoType.L };

            // 4가지 색상 (1~4)
            int[] colors = { 1, 2, 3, 4 };

            // 모든 타입과 색상 조합을 리스트에 추가 (28가지)
            foreach (var type in types)
            {
                foreach (var color in colors)
                {
                    TetriminoComponent tetrimino = new TetriminoComponent
                    {
                        Type = type,
                        Shape = GetShapeForType(type),
                        Rotation = 0,
                        ColorValue = color
                    };

                    allCombinations.Add(tetrimino);
                }
            }

            // Fisher-Yates 셔플로 무작위로 섞기
            for (int i = allCombinations.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                TetriminoComponent temp = allCombinations[i];
                allCombinations[i] = allCombinations[randomIndex];
                allCombinations[randomIndex] = temp;
            }

            // 섞인 리스트를 큐로 변환
            Queue<TetriminoComponent> queue = new Queue<TetriminoComponent>();
            foreach (var tetrimino in allCombinations)
            {
                queue.Enqueue(tetrimino);
            }

            Debug.Log($"28가지 Tetrimino 조합이 무작위로 섞인 큐 생성 완료: {queue.Count}개");
            return queue;
        }

        /// <summary>
        /// 진짜 7-bag 시스템을 도입하여 Tetrimino Component Queue를 생성하여 반환
        /// 색상별로 7-bag을 만든 후, 같은 타입끼리 색상을 무작위로 교환하여 섞음
        /// 7-bag 규칙: 같은 타입이 최대 7번째에 다시 나오는 것이 보장됨
        /// </summary>
        /// <returns>7-bag 시스템으로 생성된 28개의 TetriminoComponent 큐 (색상별 4백)</returns>
        private Queue<TetriminoComponent> GenerateSevenBagQueue()
        {
            Queue<TetriminoComponent> queue = new Queue<TetriminoComponent>();

            // 7가지 타입
            TetriminoType[] types = { TetriminoType.I, TetriminoType.O, TetriminoType.T,
                                    TetriminoType.S, TetriminoType.Z, TetriminoType.J, TetriminoType.L };

            // 4가지 색상별로 7-bag 생성
            List<TetriminoComponent>[] colorBags = new List<TetriminoComponent>[4];

            // 각 색상별로 7타입이 들어있는 백 생성
            for (int colorIndex = 0; colorIndex < 4; colorIndex++)
            {
                colorBags[colorIndex] = new List<TetriminoComponent>();
                int color = colorIndex + 1; // 1~4 색상

                // 7가지 타입을 해당 색상으로 생성
                foreach (var type in types)
                {
                    TetriminoComponent tetrimino = new TetriminoComponent
                    {
                        Type = type,
                        Shape = GetShapeForType(type),
                        Rotation = 0,
                        ColorValue = color
                    };

                    colorBags[colorIndex].Add(tetrimino);
                }

                // 각 색상 백을 무작위로 섞기 (Fisher-Yates 셔플)
                for (int i = colorBags[colorIndex].Count - 1; i > 0; i--)
                {
                    int randomIndex = Random.Range(0, i + 1);
                    TetriminoComponent temp = colorBags[colorIndex][i];
                    colorBags[colorIndex][i] = colorBags[colorIndex][randomIndex];
                    colorBags[colorIndex][randomIndex] = temp;
                }
            }

            // 1번 색상부터 4번 색상까지 순회하면서 같은 타입끼리 색상 교환
            for (int bagIndex = 0; bagIndex < 4; bagIndex++)
            {
                for (int typeIndex = 0; typeIndex < 7; typeIndex++)
                {
                    // 현재 테트리미노
                    TetriminoComponent currentTetrimino = colorBags[bagIndex][typeIndex];

                    // 무작위로 다른 백 선택 (본인 제외)
                    int randomBagIndex;
                    do
                    {
                        randomBagIndex = Random.Range(0, 4);
                    } while (randomBagIndex == bagIndex);

                    // 해당 백에서 같은 타입 찾기
                    for (int i = 0; i < 7; i++)
                    {
                        if (colorBags[randomBagIndex][i].Type == currentTetrimino.Type)
                        {
                            // 같은 타입끼리 색상 교환
                            TetriminoComponent temp = colorBags[bagIndex][typeIndex];
                            colorBags[bagIndex][typeIndex] = colorBags[randomBagIndex][i];
                            colorBags[randomBagIndex][i] = temp;
                            break;
                        }
                    }
                }
            }

            // 모든 백의 테트리미노를 순서대로 큐에 추가
            for (int bagIndex = 0; bagIndex < 4; bagIndex++)
            {
                foreach (var tetrimino in colorBags[bagIndex])
                {
                    queue.Enqueue(tetrimino);
                }
            }

            Debug.Log($"색상별 7-bag 시스템 Tetrimino 큐 생성 완료: 4백, 총 {queue.Count}개 테트리미노 (색상간 무작위 교환 적용)");
            return queue;
        }

        /// <summary>
        /// TetriminoType에 따른 Shape 배열을 반환
        /// </summary>
        /// <param name="type">테트리미노 타입</param>
        /// <returns>해당 타입의 Shape 배열</returns>
        private static Vector2Int[] GetShapeForType(TetriminoType type)
        {
            switch (type)
            {
                case TetriminoType.I:
                    return new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) };
                case TetriminoType.O:
                    return new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) };
                case TetriminoType.T:
                    return new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) };
                case TetriminoType.S:
                    return new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) };
                case TetriminoType.Z:
                    return new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(1, 0) };
                case TetriminoType.J:
                    return new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0) };
                case TetriminoType.L:
                    return new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1) };
                default:
                    return new Vector2Int[] { new Vector2Int(0, 0) };
            }
        }
    }
}