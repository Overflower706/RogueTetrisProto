using OVFL.ECS;
using UnityEngine;
using System.Collections.Generic;

namespace Minomino
{
    public class TetrominoSystem : ISetupSystem, ITickSystem
    {
        public Context Context { get; set; }

        public void Setup()
        {
            // 7가지 타입
            TetrominoType[] types = {
                TetrominoType.I, TetrominoType.O, TetrominoType.T,
                TetrominoType.S, TetrominoType.Z, TetrominoType.J, TetrominoType.L
            };

            int createdCount = 0;

            // 모든 타입을 가방 값만큼 생성
            for (int i = 0; i < GlobalSettings.Instance.BagSize; i++)
            {
                foreach (var type in types)
                {
                    // 새 Entity 생성
                    var tetrominoEntity = Context.CreateEntity();

                    // TetriminoComponent 추가
                    var tetrominoComponent = tetrominoEntity.AddComponent<TetrominoComponent>();
                    tetrominoComponent.Type = type;
                    tetrominoComponent.Shape = GetBaseShapeForType(type);

                    // 미노 Entity들 생성 및 연결
                    CreateMinoEntities(tetrominoEntity, tetrominoComponent);

                    createdCount++;

                    Debug.Log($"기본 테트리미노 Entity 생성 - Type: {type}, Entity ID: {tetrominoEntity.ID}");
                }
            }


            Debug.Log($"기본 테트리미노 Entity 생성 완료 - 총 {createdCount}개 (7타입 × {GlobalSettings.Instance.BagSize}개 가방)");
        }

        public void Tick()
        {
            var startEntities = Context.GetEntitiesWithComponent<StartGameCommand>();
            if (startEntities.Count > 0)
            {
                // 게임 시작 로직
                Queue<Entity> tetriminoQueue = GenerateSevenBagQueue();

                // TetriminoQueueComponent가 있는 엔티티 찾기
                var queueComponent = Context.GetTetrominoQueue();
                queueComponent.TetrominoQueue = tetriminoQueue;

                Debug.Log($"Tetrimino 생성 완료: {queueComponent.TetrominoQueue.Count}개 대기 중");
            }

            var endEntities = Context.GetEntitiesWithComponent<EndGameCommand>();
            if (endEntities.Count > 0)
            {
                // 게임 종료 로직
                var queueEntity = Context.GetTetrominoQueue();
                queueEntity.TetrominoQueue.Clear();

                var tetriminoEntities = Context.GetEntitiesWithComponent<TetrominoComponent>();
                foreach (var entity in tetriminoEntities)
                {
                    if (entity.HasComponent<BoardTetrominoComponent>())
                    {
                        entity.RemoveComponent<BoardTetrominoComponent>();
                    }
                }

                Debug.Log("게임 종료: Tetrimino 큐를 비우고 모든 BoardTetriminoComponent를 제거했습니다.");
            }

            var generateTetrominoEntities = Context.GetEntitiesWithComponent<GenerateTetrominoCommand>();
            if (generateTetrominoEntities.Count > 0)
            {
                var commandRequests = generateTetrominoEntities[0].GetComponent<GenerateTetrominoCommand>();
                if (commandRequests.PayLoad is TetrominoComponent tetrominoComponent)
                {
                    var tetrominoEntity = Context.CreateEntity();
                    tetrominoEntity.AddComponent(tetrominoComponent);
                    CreateMinoEntities(tetrominoEntity, tetrominoComponent);
                }
            }
        }

        /// <summary>
        /// 테트리미노에 속하는 미노 Entity들을 생성하고 연결
        /// </summary>
        /// <param name="tetriminoEntity">부모 테트리미노 Entity</param>
        /// <param name="tetrominoComponent">테트리미노 컴포넌트</param>
        private void CreateMinoEntities(Entity tetriminoEntity, TetrominoComponent tetrominoComponent)
        {
            // Minos 배열 초기화
            tetrominoComponent.Minos = new int[tetrominoComponent.Shape.Length];

            // Shape 길이만큼 미노 Entity 생성
            for (int i = 0; i < tetrominoComponent.Shape.Length; i++)
            {
                var minoEntity = Context.CreateEntity();
                var minoComponent = minoEntity.AddComponent<MinoComponent>();
                minoComponent.ParentID = tetriminoEntity.ID;
                minoComponent.State = MinoState.Empty;

                // Tetrimino에 속하는 미노 ID 추가
                tetrominoComponent.Minos[i] = minoEntity.ID;
                Debug.Log($"미노 Entity 생성 - Parent ID: {minoComponent.ParentID}, Mino State: {minoComponent.State}, Entity ID: {minoEntity.ID}");
            }
        }

        /// <summary>
        /// 이미 생성된 Tetrimino Entity들을 활용하여 무작위로 섞인 Queue를 생성하여 반환
        /// </summary>
        /// <returns>21가지 조합이 무작위로 섞인 Entity 큐</returns>
        private Queue<Entity> GenerateRandomQueue()
        {
            // 기존에 생성된 TetriminoComponent를 가진 모든 Entity들을 가져옴
            var tetrominoEntities = Context.GetEntitiesWithComponent<TetrominoComponent>();

            // Fisher-Yates 셔플로 무작위로 섞기
            for (int i = tetrominoEntities.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                Entity temp = tetrominoEntities[i];
                tetrominoEntities[i] = tetrominoEntities[randomIndex];
                tetrominoEntities[randomIndex] = temp;
            }

            // 섞인 리스트를 큐로 변환
            Queue<Entity> queue = new Queue<Entity>();
            foreach (var entity in tetrominoEntities)
            {
                queue.Enqueue(entity);
            }

            Debug.Log($"무작위로 섞인 Tetromino Entity 큐 생성 완료: {queue.Count}개");
            return queue;
        }

        /// <summary>
        /// 이미 생성된 Tetrimino Entity들을 활용하여 7-bag 시스템으로 Queue를 생성하여 반환
        /// 7-bag 규칙: 각 가방에는 같은 타입이 중복되지 않으며, 모든 타입이 소진될 때까지 반복
        /// </summary>
        private Queue<Entity> GenerateSevenBagQueue()
        {
            Queue<Entity> queue = new Queue<Entity>();

            // 기존에 생성된 TetriminoComponent를 가진 모든 Entity들을 가져옴
            var tetriminoEntities = Context.GetEntitiesWithComponent<TetrominoComponent>();

            // 타입별로 Entity들을 분류
            var entityByType = new Dictionary<TetrominoType, List<Entity>>();

            foreach (var entity in tetriminoEntities)
            {
                var tetrominoComponent = entity.GetComponent<TetrominoComponent>();
                var type = tetrominoComponent.Type;

                if (!entityByType.ContainsKey(type))
                {
                    entityByType[type] = new List<Entity>();
                }
                entityByType[type].Add(entity);
            }

            // 7가지 타입
            TetrominoType[] types = {
                TetrominoType.I, TetrominoType.O, TetrominoType.T,
                TetrominoType.S, TetrominoType.Z, TetrominoType.J, TetrominoType.L
            };

            // 가방 생성 시작
            List<Entity> currentBag = new List<Entity>();
            List<TetrominoType> availableTypes = new List<TetrominoType>(types);

            while (true)
            {
                // 사용 가능한 타입이 없으면 중단
                if (availableTypes.Count == 0)
                {
                    // 현재 가방에 남은 것이 있으면 큐에 추가
                    if (currentBag.Count > 0)
                    {
                        ShuffleBag(currentBag);
                        foreach (var entity in currentBag)
                        {
                            queue.Enqueue(entity);
                        }
                    }
                    break;
                }

                // 사용 가능한 타입 중에서 무작위로 하나 선택
                int randomIndex = Random.Range(0, availableTypes.Count);
                TetrominoType selectedType = availableTypes[randomIndex];

                // 선택된 타입에서 Entity 하나 가져오기
                if (entityByType.ContainsKey(selectedType) && entityByType[selectedType].Count > 0)
                {
                    Entity selectedEntity = entityByType[selectedType][0];
                    entityByType[selectedType].RemoveAt(0);
                    currentBag.Add(selectedEntity);

                    // 해당 타입의 Entity가 더 없으면 사용 가능한 타입에서 제거
                    if (entityByType[selectedType].Count == 0)
                    {
                        entityByType.Remove(selectedType);
                    }
                }

                // 현재 가방에서 해당 타입 제거 (중복 방지)
                availableTypes.RemoveAt(randomIndex);

                // 가방이 7개가 되거나 사용 가능한 타입이 없으면 가방 완성
                if (currentBag.Count == 7 || availableTypes.Count == 0)
                {
                    // 가방을 섞고 큐에 추가
                    ShuffleBag(currentBag);
                    foreach (var entity in currentBag)
                    {
                        queue.Enqueue(entity);
                    }

                    Debug.Log($"가방 완성: {currentBag.Count}개 테트리미노 추가됨");

                    // 새 가방 시작
                    currentBag.Clear();

                    // 아직 Entity가 남아있는 타입들로 다시 사용 가능한 타입 목록 구성
                    availableTypes.Clear();
                    foreach (var type in types)
                    {
                        if (entityByType.ContainsKey(type) && entityByType[type].Count > 0)
                        {
                            availableTypes.Add(type);
                        }
                    }
                }
            }

            Debug.Log($"7-bag 시스템 Tetromino Entity 큐 생성 완료: 총 {queue.Count}개 Entity");
            return queue;
        }

        /// <summary>
        /// 가방 내 Entity들을 Fisher-Yates 셔플로 무작위로 섞기
        /// </summary>
        /// <param name="bag">섞을 Entity 리스트</param>
        private void ShuffleBag(List<Entity> bag)
        {
            for (int i = bag.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                Entity temp = bag[i];
                bag[i] = bag[randomIndex];
                bag[randomIndex] = temp;
            }
        }

        /// <summary>
        /// TetriminoType에 따른 Shape 배열을 반환
        /// </summary>
        /// <param name="type">테트리미노 타입</param>
        /// <returns>해당 타입의 Shape 배열</returns>
        private static Vector2Int[] GetBaseShapeForType(TetrominoType type)
        {
            switch (type)
            {
                case TetrominoType.I:
                    return new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) };
                case TetrominoType.O:
                    return new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) };
                case TetrominoType.T:
                    return new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) };
                case TetrominoType.S:
                    return new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) };
                case TetrominoType.Z:
                    return new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(1, 0) };
                case TetrominoType.J:
                    return new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0) };
                case TetrominoType.L:
                    return new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1) };
                default:
                    return new Vector2Int[] { new Vector2Int(0, 0) };
            }
        }
    }
}