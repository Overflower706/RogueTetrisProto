using OVFL.ECS;
using UnityEngine;
using System.Collections.Generic;

namespace Minomino
{
    public class TetriminoSystem : ISetupSystem, ITickSystem
    {
        public Context Context { get; set; }

        public void Setup()
        {
            // 7가지 타입
            TetriminoType[] types = {
                TetriminoType.I, TetriminoType.O, TetriminoType.T,
                TetriminoType.S, TetriminoType.Z, TetriminoType.J, TetriminoType.L
            };

            // 2가지 색상
            TetriminoColor[] colors = {
                TetriminoColor.Red, TetriminoColor.Green
            };

            int createdCount = 0;

            // 모든 타입과 색상 조합으로 Entity 생성
            foreach (var type in types)
            {
                foreach (var color in colors)
                {
                    // 새 Entity 생성
                    var tetriminoEntity = Context.CreateEntity();

                    // TetriminoComponent 추가
                    var tetriminoComponent = tetriminoEntity.AddComponent<TetriminoComponent>();
                    tetriminoComponent.Type = type;
                    tetriminoComponent.Color = color; // 지금은 안 쓰긴 합니다.
                    tetriminoComponent.Shape = GetShapeForType(type);

                    // MinoComponent 추가
                    for (int i = 0; i < tetriminoComponent.Shape.Length; i++)
                    {
                        var minoEntity = Context.CreateEntity();
                        var minoComponent = minoEntity.AddComponent<MinoComponent>();
                        minoComponent.ParentID = tetriminoEntity.ID;
                        minoComponent.State = MinoState.Empty;

                        // Tetrimino에 속하는 미노 ID 추가
                        if (tetriminoComponent.Minos == null)
                        {
                            tetriminoComponent.Minos = new int[tetriminoComponent.Shape.Length];
                        }
                        tetriminoComponent.Minos[i] = minoEntity.ID;
                        Debug.Log($"미노 Entity 생성 - Parent ID: {minoComponent.ParentID}, Mino State: {minoComponent.State}, Entity ID: {minoEntity.ID}");
                    }

                    createdCount++;

                    Debug.Log($"기본 테트리미노 Entity 생성 - Type: {type}, Color: {color}, Entity ID: {tetriminoEntity.ID}");
                }
            }

            var queueEntity = Context.CreateEntity()
                                        .AddComponent<TetriminoQueueComponent>();

            Debug.Log($"기본 테트리미노 Entity 생성 완료 - 총 {createdCount}개 (7타입 × 2색상)");
        }

        public void Tick()
        {
            var startEntities = Context.GetEntitiesWithComponent<StartGameCommand>();
            if (startEntities.Count > 0)
            {
                // 게임 시작 로직
                Queue<Entity> tetriminoQueue = GenerateRandomQueue();

                // TetriminoQueueComponent가 있는 엔티티 찾기 또는 생성
                var queueComponent = GetTetriminoQueue();
                queueComponent.TetriminoQueue = tetriminoQueue;

                Debug.Log($"Tetrimino 생성 완료: {queueComponent.TetriminoQueue.Count}개 대기 중");
            }

            var endEntities = Context.GetEntitiesWithComponent<EndGameCommand>();
            if (endEntities.Count > 0)
            {
                // 게임 종료 로직
                var queueEntity = GetTetriminoQueue();
                queueEntity.TetriminoQueue.Clear();

                var tetriminoEntities = Context.GetEntitiesWithComponent<TetriminoComponent>();
                foreach (var entity in tetriminoEntities)
                {
                    if (entity.HasComponent<BoardTetriminoComponent>())
                    {
                        entity.RemoveComponent<BoardTetriminoComponent>();
                    }
                }

                Debug.Log("게임 종료: Tetrimino 큐를 비우고 모든 BoardTetriminoComponent를 제거했습니다.");
            }

            var state = GetState();
            if (state.CurrentState != GameState.Playing) return;
        }

        private GameStateComponent GetState()
        {
            var stateEntities = Context.GetEntitiesWithComponent<GameStateComponent>();
            if (stateEntities.Count == 0)
            {
                Debug.LogWarning("GameStateComponent가 있는 엔티티가 없습니다.");
                return null; // 상태가 없으면 null 반환
            }
            else if (stateEntities.Count > 1)
            {
                Debug.LogWarning("GameStateComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return null; // 여러 엔티티가 있으면 경고 후 null 반환
            }
            return stateEntities[0].GetComponent<GameStateComponent>();
        }

        private TetriminoQueueComponent GetTetriminoQueue()
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
        /// 이미 생성된 Tetrimino Entity들을 활용하여 무작위로 섞인 Queue를 생성하여 반환
        /// 7가지 타입 × 3가지 색상 = 21가지 조합을 모두 1개씩 포함
        /// </summary>
        /// <returns>21가지 조합이 무작위로 섞인 Entity 큐</returns>
        private Queue<Entity> GenerateRandomQueue()
        {
            // 기존에 생성된 TetriminoComponent를 가진 모든 Entity들을 가져옴
            var tetriminoEntities = Context.GetEntitiesWithComponent<TetriminoComponent>();

            // Fisher-Yates 셔플로 무작위로 섞기
            for (int i = tetriminoEntities.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                Entity temp = tetriminoEntities[i];
                tetriminoEntities[i] = tetriminoEntities[randomIndex];
                tetriminoEntities[randomIndex] = temp;
            }

            // 섞인 리스트를 큐로 변환
            Queue<Entity> queue = new Queue<Entity>();
            foreach (var entity in tetriminoEntities)
            {
                queue.Enqueue(entity);
            }

            Debug.Log($"무작위로 섞인 Tetrimino Entity 큐 생성 완료: {queue.Count}개");
            return queue;
        }

        /// <summary>
        /// 이미 생성된 Tetrimino Entity들을 활용하여 7-bag 시스템으로 Queue를 생성하여 반환
        /// 색상별로 7-bag을 만든 후, 같은 타입끼리 색상을 무작위로 교환하여 섞음
        /// 7-bag 규칙: 같은 타입이 최대 7번째에 다시 나오는 것이 보장됨
        /// </summary>
        /// <returns>7-bag 시스템으로 생성된 21개의 Entity 큐 (색상별 3백)</returns>
        private Queue<Entity> GenerateSevenBagQueue()
        {
            Queue<Entity> queue = new Queue<Entity>();

            // 기존에 생성된 TetriminoComponent를 가진 모든 Entity들을 가져옴
            var tetriminoEntities = Context.GetEntitiesWithComponent<TetriminoComponent>();

            // 타입별, 색상별로 Entity들을 분류
            var entityByTypeAndColor = new Dictionary<(TetriminoType, TetriminoColor), Entity>();

            foreach (var entity in tetriminoEntities)
            {
                var tetriminoComponent = entity.GetComponent<TetriminoComponent>();
                var key = (tetriminoComponent.Type, tetriminoComponent.Color);
                entityByTypeAndColor[key] = entity;
            }

            // 7가지 타입
            TetriminoType[] types = { TetriminoType.I, TetriminoType.O, TetriminoType.T,
                                    TetriminoType.S, TetriminoType.Z, TetriminoType.J, TetriminoType.L };

            // 2가지 색상별로 7-bag 생성
            List<Entity>[] colorBags = new List<Entity>[2];
            TetriminoColor[] colors = { TetriminoColor.Red, TetriminoColor.Green };

            // 각 색상별로 7타입이 들어있는 백 생성
            for (int colorIndex = 0; colorIndex < 2; colorIndex++)
            {
                colorBags[colorIndex] = new List<Entity>();
                TetriminoColor color = colors[colorIndex];

                // 7가지 타입을 해당 색상으로 찾아서 백에 추가
                foreach (var type in types)
                {
                    var key = (type, color);
                    if (entityByTypeAndColor.ContainsKey(key))
                    {
                        colorBags[colorIndex].Add(entityByTypeAndColor[key]);
                    }
                }

                // 각 색상 백을 무작위로 섞기 (Fisher-Yates 셔플)
                for (int i = colorBags[colorIndex].Count - 1; i > 0; i--)
                {
                    int randomIndex = Random.Range(0, i + 1);
                    Entity temp = colorBags[colorIndex][i];
                    colorBags[colorIndex][i] = colorBags[colorIndex][randomIndex];
                    colorBags[colorIndex][randomIndex] = temp;
                }
            }

            // 1번 색상부터 2번 색상까지 순회하면서 같은 타입끼리 색상 교환
            for (int bagIndex = 0; bagIndex < 2; bagIndex++)
            {
                for (int typeIndex = 0; typeIndex < colorBags[bagIndex].Count; typeIndex++)
                {
                    // 현재 Entity의 타입 확인
                    var currentEntity = colorBags[bagIndex][typeIndex];
                    var currentType = currentEntity.GetComponent<TetriminoComponent>().Type;

                    // 무작위로 다른 백 선택 (본인 제외)
                    int randomBagIndex;
                    do
                    {
                        randomBagIndex = Random.Range(0, 2);
                    } while (randomBagIndex == bagIndex);

                    // 해당 백에서 같은 타입 찾기
                    for (int i = 0; i < colorBags[randomBagIndex].Count; i++)
                    {
                        var otherEntity = colorBags[randomBagIndex][i];
                        var otherType = otherEntity.GetComponent<TetriminoComponent>().Type;

                        if (otherType == currentType)
                        {
                            // 같은 타입끼리 색상 교환
                            Entity temp = colorBags[bagIndex][typeIndex];
                            colorBags[bagIndex][typeIndex] = colorBags[randomBagIndex][i];
                            colorBags[randomBagIndex][i] = temp;
                            break;
                        }
                    }
                }
            }

            // 모든 백의 Entity들을 순서대로 큐에 추가
            for (int bagIndex = 0; bagIndex < 2; bagIndex++)
            {
                foreach (var entity in colorBags[bagIndex])
                {
                    queue.Enqueue(entity);
                }
            }

            Debug.Log($"색상별 7-bag 시스템 Tetrimino Entity 큐 생성 완료: 2백, 총 {queue.Count}개 Entity (색상간 무작위 교환 적용)");
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