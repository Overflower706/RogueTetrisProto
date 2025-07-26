using System.Collections.Generic;
using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class BoardSystem : ITickSystem, ICleanupSystem
    {
        public Context Context { get; set; }

        public void Tick()
        {
            var startEntities = Context.GetEntitiesWithComponent<StartGameCommand>();
            if (startEntities.Count > 0)
            {
                // 초기 보드 설정 로직
                var board = Context.GetBoard();
                board.Board = new int[GlobalSettings.Instance.SafeWidth, GlobalSettings.Instance.BoardHeight];
                for (int x = 0; x < GlobalSettings.Instance.SafeWidth; x++)
                {
                    for (int y = 0; y < GlobalSettings.Instance.BoardHeight; y++)
                    {
                        board.Board[x, y] = 0; // 빈 칸을 0으로 초기화
                    }
                }

                Debug.Log("게임 시작, 보드 초기화 완료");
            }

            var endEntities = Context.GetEntitiesWithComponent<EndGameCommand>();
            if (endEntities.Count > 0)
            {
                // 게임 종료 로직
                Debug.Log("게임 종료, 보드 초기화");
                var board = Context.GetBoard();
                board.Board = new int[GlobalSettings.Instance.SafeWidth, GlobalSettings.Instance.BoardHeight];
                for (int x = 0; x < GlobalSettings.Instance.SafeWidth; x++)
                {
                    for (int y = 0; y < GlobalSettings.Instance.BoardHeight; y++)
                    {
                        board.Board[x, y] = 0; // 빈 칸으로 초기화
                    }
                }

                return; // 게임이 종료되면 더 이상 처리하지 않음
            }

            var state = Context.GetGameState();
            if (state.CurrentState != GameState.Playing) return;

            var currentEntity = GetCurrentTetriminoEntity();

            if (currentEntity == null)
            {
                Debug.LogWarning("현재 테트리미노가 없습니다. 뭔가 잘못됨");
                return; // 현재 테트리미노가 없으면 더 이상 처리하지 않음. 사실 없으면 안되긴 하는데.
            }

            MarkTetriminoToBoard(currentEntity);

            var moveLeftEntities = Context.GetEntitiesWithComponent<MoveLeftCommand>();
            if (moveLeftEntities.Count > 0)
            {
                ProcessMove(currentEntity, false);
            }

            var moveRightEntities = Context.GetEntitiesWithComponent<MoveRightCommand>();
            if (moveRightEntities.Count > 0)
            {
                ProcessMove(currentEntity, true);
            }

            var rotateClockwiseEntities = Context.GetEntitiesWithComponent<RotateClockwiseCommand>();
            if (rotateClockwiseEntities.Count > 0)
            {
                ProcessRotate(currentEntity, true); // true = 시계방향
            }

            var rotateCounterClockwiseEntities = Context.GetEntitiesWithComponent<RotateCounterClockwiseCommand>();
            if (rotateCounterClockwiseEntities.Count > 0)
            {
                ProcessRotate(currentEntity, false); // false = 반시계방향
            }

            var softDropEntities = Context.GetEntitiesWithComponent<SoftDropCommand>();
            if (softDropEntities.Count > 0)
            {
                ProcessSoftDrop(currentEntity);
            }

            var hardDropEntities = Context.GetEntitiesWithComponent<HardDropCommand>();
            if (hardDropEntities.Count > 0)
            {
                ProcessHardDrop(currentEntity);
            }

            var holdEntities = Context.GetEntitiesWithComponent<HoldTetriminoCommand>();
            if (holdEntities.Count > 0)
            {
                ClearTetriminoFromBoard(currentEntity);
            }
        }

        public void Cleanup()
        {
            var completedLines = Context.GetEntitiesWithComponent<CompletedLineComponent>();
            foreach (var entity in completedLines)
            {
                Context.DestroyEntity(entity);
            }
        }

        /// <summary>
        /// CurrentTetriminoComponent를 가진 엔티티 찾기
        /// </summary>
        private Entity GetCurrentTetriminoEntity()
        {
            var tetriminoEntities = Context.GetEntitiesWithComponent<BoardTetrominoComponent>();
            foreach (var entity in tetriminoEntities)
            {
                var tetriminoComponent = entity.GetComponent<BoardTetrominoComponent>();
                if (tetriminoComponent.State == BoardTetrominoState.Current)
                {
                    return entity; // 현재 상태의 테트리미노 엔티티 반환
                }
            }

            return null;
        }

        /// <summary>
        /// 해당 위치로 이동 가능한지 확인
        /// </summary>
        private bool CanMoveTo(Entity tetrimino, Vector2Int newPosition)
        {
            var tetriminoComponent = tetrimino.GetComponent<TetrominoComponent>();
            var worldPositions = GetTetriminoWorldPositions(tetrimino, newPosition);

            for (int i = 0; i < worldPositions.Length && i < tetriminoComponent.Minos.Length; i++)
            {
                var position = worldPositions[i];
                var minoEntityId = tetriminoComponent.Minos[i];

                // 보드 경계 체크
                if (!IsValidPosition(position))
                {
                    return false;
                }

                var board = Context.GetBoard();

                // 고정된 블록과 충돌 체크 (0이 아닌 다른 Entity ID가 있으면 충돌)
                // 단, 현재 테트리미노의 미노 ID들은 제외 (이동 중이므로)
                int currentCellId = board.Board[position.x, position.y];
                if (currentCellId > 0 && !IsCurrentTetriminoMino(tetrimino, currentCellId))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 주어진 Entity ID가 현재 테트리미노의 미노인지 확인
        /// </summary>
        private bool IsCurrentTetriminoMino(Entity tetrimino, int entityId)
        {
            var tetriminoComponent = tetrimino.GetComponent<TetrominoComponent>();
            foreach (int minoId in tetriminoComponent.Minos)
            {
                if (minoId == entityId)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 특정 위치 기준으로 테트리미노의 실제 월드 위치들 계산
        /// </summary>
        private Vector2Int[] GetTetriminoWorldPositions(Entity tetrimino, Vector2Int position)
        {
            var tetriminoComponent = tetrimino.GetComponent<TetrominoComponent>();
            var boardTetrimino = tetrimino.GetComponent<BoardTetrominoComponent>();

            Vector2Int[] worldPositions = new Vector2Int[tetriminoComponent.Shape.Length];

            for (int i = 0; i < tetriminoComponent.Shape.Length; i++)
            {
                Vector2Int rotatedShape = RotateShape(tetriminoComponent.Shape[i], boardTetrimino.Rotation);
                worldPositions[i] = position + rotatedShape;
            }

            return worldPositions;
        }

        /// <summary>
        /// Shape를 회전시키는 메서드
        /// </summary>
        private Vector2Int RotateShape(Vector2Int shape, int rotation)
        {
            // 90도씩 시계방향 회전
            for (int i = 0; i < rotation; i++)
            {
                int temp = shape.x;
                shape.x = -shape.y;
                shape.y = temp;
            }
            return shape;
        }

        /// <summary>
        /// 보드 범위 내 유효한 위치인지 확인
        /// </summary>
        private bool IsValidPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < GlobalSettings.Instance.SafeWidth &&
                   position.y >= 0 && position.y < GlobalSettings.Instance.BoardHeight;
        }

        /// <summary>
        /// 실제 이동 처리
        /// </summary>
        private void ProcessMove(Entity tetriminoEntity, bool isRight)
        {
            ClearTetriminoFromBoard(tetriminoEntity);

            var board = Context.GetBoard();

            var tetriminoComponent = tetriminoEntity.GetComponent<TetrominoComponent>();
            var boardTetrimino = tetriminoEntity.GetComponent<BoardTetrominoComponent>();

            // 이동 방향 결정
            Vector2Int moveDirection = Vector2Int.zero;
            if (isRight)
            {
                moveDirection = Vector2Int.right;

            }
            else
            {
                moveDirection = Vector2Int.left;
            }

            // 새 위치 계산
            Vector2Int newPosition = boardTetrimino.Position + moveDirection;

            // 충돌 검사
            if (CanMoveTo(tetriminoEntity, newPosition))
            {
                // 이동 가능하면 위치 업데이트
                boardTetrimino.Position = newPosition;
                Debug.Log($"Tetrimino isRight : {isRight} 이동 성공: {newPosition}");
            }
            else
            {
                Debug.Log($"Tetrimino isRight : {isRight} 이동 실패: 충돌 또는 경계");
            }

            MarkTetriminoToBoard(tetriminoEntity);
        }


        /// <summary>
        /// 회전 처리 - 테트리미노를 시계방향 또는 반시계방향으로 회전
        /// Wall Kick 기능 포함: 회전이 불가능할 때 위치를 조정하여 회전 시도
        /// </summary>
        private void ProcessRotate(Entity tetriminoEntity, bool clockwise)
        {
            // 현재 위치에서 테트리미노 제거
            ClearTetriminoFromBoard(tetriminoEntity);

            var board = Context.GetBoard();
            var tetriminoComponent = tetriminoEntity.GetComponent<TetrominoComponent>();
            var boardTetrimino = tetriminoEntity.GetComponent<BoardTetrominoComponent>();

            // 회전 계산
            int originalRotation = boardTetrimino.Rotation;
            int newRotation;

            if (clockwise)
            {
                newRotation = (originalRotation + 1) % 4; // 시계방향 회전
            }
            else
            {
                newRotation = (originalRotation - 1 + 4) % 4; // 반시계방향 회전
            }

            // 임시로 회전 적용
            boardTetrimino.Rotation = newRotation;

            // Wall Kick 시도 - 여러 위치에서 회전 가능성 검사
            Vector2Int originalPosition = boardTetrimino.Position;
            Vector2Int[] wallKickOffsets = GetWallKickOffsets(tetriminoComponent.Shape, originalRotation, newRotation, clockwise);

            bool rotationSuccessful = false;
            Vector2Int finalPosition = originalPosition;

            // 각 Wall Kick 오프셋에 대해 회전 가능성 검사
            foreach (var offset in wallKickOffsets)
            {
                Vector2Int testPosition = originalPosition + offset;

                if (CanMoveTo(tetriminoEntity, testPosition))
                {
                    // 회전 가능한 위치 발견
                    finalPosition = testPosition;
                    rotationSuccessful = true;
                    break;
                }
            }

            if (rotationSuccessful)
            {
                // 회전 성공 - 새로운 위치와 회전 적용
                boardTetrimino.Position = finalPosition;
                boardTetrimino.Rotation = newRotation;

                string kickInfo = finalPosition != originalPosition ?
                    $" (Wall Kick: {finalPosition - originalPosition})" : "";
                Debug.Log($"Tetrimino 회전 성공: {(clockwise ? "시계방향" : "반시계방향")} - 새 회전: {newRotation}{kickInfo}");
            }
            else
            {
                // 회전 불가능 - 원래 회전으로 되돌림
                boardTetrimino.Rotation = originalRotation;
                Debug.Log($"Tetrimino 회전 실패: {(clockwise ? "시계방향" : "반시계방향")} - 모든 Wall Kick 시도 실패");
            }

            // 테트리미노 표시
            MarkTetriminoToBoard(tetriminoEntity);
        }

        /// <summary>
        /// SoftDrop 처리 - 테트리미노를 한 칸 아래로 이동
        /// </summary>
        private void ProcessSoftDrop(Entity tetriminoEntity)
        {
            // 현재 위치에서 테트리미노 제거
            ClearTetriminoFromBoard(tetriminoEntity);

            var tetriminoComponent = tetriminoEntity.GetComponent<TetrominoComponent>();
            var boardTetrimino = tetriminoEntity.GetComponent<BoardTetrominoComponent>();

            // 아래로 한 칸 이동 시도
            Vector2Int newPosition = boardTetrimino.Position + Vector2Int.down;

            if (CanMoveTo(tetriminoEntity, newPosition))
            {
                // 이동 가능하면 위치 업데이트
                boardTetrimino.Position = newPosition;
                Debug.Log($"SoftDrop 성공: {newPosition}");
                MarkTetriminoToBoard(tetriminoEntity);
            }
            else
            {
                // 이동 불가능하면 현재 위치에 고정
                Debug.Log("SoftDrop 실패: 바닥 또는 충돌 - 테트리미노 고정");
                MarkTetriminoToBoard(tetriminoEntity);
                FixTetrimino(tetriminoEntity);
                return;
            }
        }

        /// <summary>
        /// HardDrop 처리 - 테트리미노를 바닥까지 즉시 이동
        /// </summary>
        private void ProcessHardDrop(Entity tetriminoEntity)
        {
            ClearTetriminoFromBoard(tetriminoEntity);

            var board = Context.GetBoard();

            var tetriminoComponent = tetriminoEntity.GetComponent<TetrominoComponent>();
            var boardTetrimino = tetriminoEntity.GetComponent<BoardTetrominoComponent>();

            // 가능한 가장 아래까지 이동
            Vector2Int newPosition = boardTetrimino.Position;
            Vector2Int testPosition = newPosition;

            // 아래로 계속 이동하면서 충돌하지 않는 가장 낮은 위치 찾기
            while (true)
            {
                testPosition = newPosition + Vector2Int.down;
                if (CanMoveTo(tetriminoEntity, testPosition))
                {
                    newPosition = testPosition;
                }
                else
                {
                    break; // 더 이상 이동할 수 없으면 중단
                }
            }

            // 최종 위치로 이동
            boardTetrimino.Position = newPosition;
            Debug.Log($"HardDrop 성공: {newPosition}");

            // HardDrop 후에는 즉시 테트리미노를 고정하고 새로운 테트리미노 
            MarkTetriminoToBoard(tetriminoEntity);
            FixTetrimino(tetriminoEntity);
        }

        /// <summary>
        /// Board에 테트리미노 표시 (개별 미노 Entity ID로 저장)
        /// </summary>
        private void MarkTetriminoToBoard(Entity tetriminoEntity)
        {
            var board = Context.GetBoard();

            var tetriminoComponent = tetriminoEntity.GetComponent<TetrominoComponent>();
            var boardTetrimino = tetriminoEntity.GetComponent<BoardTetrominoComponent>();

            // Shape과 Rotation 기반으로 실제 블록 위치들 계산
            var blockPositions = GetTetriminoWorldPositions(tetriminoEntity, boardTetrimino.Position);

            // Shape 순서대로 Minos와 매칭하여 개별 미노 Entity ID 할당
            if (blockPositions.Length == 0)
            {
                Debug.LogWarning("테트리미노의 블록 위치가 비어 있습니다. 테트리미노가 올바르게 설정되었는지 확인하세요.");
                return; // 블록 위치가 비어 있으면 처리 중단
            }
            if (tetriminoComponent.Minos == null)
            {
                Debug.LogWarning("테트리미노의 Minos가 null입니다. 테트리미노가 올바르게 설정되었는지 확인하세요.");
                return; // Minos가 null이면 처리 중단
            }
            for (int i = 0; i < blockPositions.Length && i < tetriminoComponent.Minos.Length; i++)
            {
                var position = blockPositions[i];
                var minoEntityId = tetriminoComponent.Minos[i];

                if (IsValidPosition(position) && minoEntityId > 0)
                {
                    board.Board[position.x, position.y] = minoEntityId; // 개별 미노 Entity ID로 테트리미노 표시
                }
            }
        }

        private void ClearTetriminoFromBoard(Entity tetriminoEntity)
        {
            var board = Context.GetBoard();

            var tetriminoComponent = tetriminoEntity.GetComponent<TetrominoComponent>();
            var boardTetrimino = tetriminoEntity.GetComponent<BoardTetrominoComponent>();

            // Shape과 Rotation 기반으로 실제 블록 위치들 계산
            var blockPositions = GetTetriminoWorldPositions(tetriminoEntity, boardTetrimino.Position);

            // Shape 순서대로 Minos와 매칭하여 해당 미노만 제거
            for (int i = 0; i < blockPositions.Length && i < tetriminoComponent.Minos.Length; i++)
            {
                var position = blockPositions[i];
                var minoEntityId = tetriminoComponent.Minos[i];

                if (IsValidPosition(position) && minoEntityId > 0)
                {
                    // 해당 위치에 있는 Entity ID가 현재 미노 ID와 일치하면 제거
                    if (board.Board[position.x, position.y] == minoEntityId)
                    {
                        board.Board[position.x, position.y] = 0; // 빈 칸으로 설정
                    }
                }
            }
        }

        /// <summary>
        /// 테트리미노를 고정하고 새로운 테트리미노 생성 요청
        /// </summary>
        private void FixTetrimino(Entity tetriminoEntity)
        {
            // 현재 위치에 테트리미노 표시 (고정)
            tetriminoEntity.RemoveComponent<BoardTetrominoComponent>();
            Debug.Log("Board System : 테트리스 고정할게~");

            CheckCompletedLines();
        }

        private void CheckCompletedLines()
        {
            var completedLines = new List<int>();

            for (int y = 0; y < GlobalSettings.Instance.SafeHeight; y++)
            {
                if (IsLineCompleted(y))
                {
                    completedLines.Add(y);
                }
            }

            if (completedLines.Count > 0)
            {
                // MinoSystem 측에서 수정할 수 있도록 Component 발행
                var completedLineComponent = Context.CreateEntity().AddComponent<CompletedLineComponent>();
                completedLineComponent.CompletedLine = completedLines;
            }
        }


        /// <summary>
        /// 특정 줄이 완성되었는지 검사
        /// </summary>
        private bool IsLineCompleted(int lineY)
        {
            var board = Context.GetBoard();

            for (int x = 0; x < GlobalSettings.Instance.SafeWidth; x++)
            {
                if (board.Board[x, lineY] == 0) // 빈 칸이 있으면 완성되지 않음
                {
                    return false;
                }

                var entity = Context.FindEntityByID(board.Board[x, lineY]);
                var minoComponent = entity.GetComponent<MinoComponent>();
                if (minoComponent.State != MinoState.Empty)
                {
                    return false;
                }
            }
            return true; // 모든 칸이 채워짐
        }

        // ========================================
        // Wall Kick System (Shape-Based Rotation)
        // ========================================

        /// <summary>
        /// Shape 기반 Wall Kick 오프셋 배열 생성
        /// </summary>
        private Vector2Int[] GetWallKickOffsets(Vector2Int[] shape, int fromRotation, int toRotation, bool clockwise)
        {
            // Shape의 특성을 분석하여 적절한 Wall Kick 패턴 결정
            var shapeAnalysis = AnalyzeShape(shape);

            switch (shapeAnalysis.ShapeType)
            {
                case ShapeType.Square:
                    return GetSquareWallKickOffsets();
                case ShapeType.Line:
                    return GetLineWallKickOffsets(fromRotation, toRotation, clockwise);
                case ShapeType.Complex:
                    return GetComplexWallKickOffsets(shapeAnalysis, fromRotation, toRotation, clockwise);
                default:
                    return GetStandardWallKickOffsets(fromRotation, toRotation, clockwise);
            }
        }

        /// <summary>
        /// Shape 분석 결과를 담는 구조체
        /// </summary>
        private struct ShapeAnalysis
        {
            public ShapeType ShapeType;
            public Vector2Int BoundingBox;
            public Vector2Int Center;
            public int BlockCount;
            public bool IsSymmetric;
        }

        /// <summary>
        /// Shape 타입 열거형
        /// </summary>
        private enum ShapeType
        {
            Square,    // 정사각형 (2x2)
            Line,      // 직선형 (1xN 또는 Nx1)
            Complex,   // 복잡한 형태
            Standard   // 일반적인 테트리미노 형태
        }

        /// <summary>
        /// Shape을 분석하여 특성을 파악
        /// </summary>
        private ShapeAnalysis AnalyzeShape(Vector2Int[] shape)
        {
            var analysis = new ShapeAnalysis();
            analysis.BlockCount = shape.Length;

            // 바운딩 박스 계산
            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;

            foreach (var block in shape)
            {
                minX = Mathf.Min(minX, block.x);
                maxX = Mathf.Max(maxX, block.x);
                minY = Mathf.Min(minY, block.y);
                maxY = Mathf.Max(maxY, block.y);
            }

            analysis.BoundingBox = new Vector2Int(maxX - minX + 1, maxY - minY + 1);
            analysis.Center = new Vector2Int((minX + maxX) / 2, (minY + maxY) / 2);

            // Shape 타입 결정
            if (analysis.BoundingBox.x == analysis.BoundingBox.y && analysis.BlockCount == 4)
            {
                analysis.ShapeType = ShapeType.Square;
            }
            else if (analysis.BoundingBox.x == 1 || analysis.BoundingBox.y == 1)
            {
                analysis.ShapeType = ShapeType.Line;
            }
            else if (analysis.BlockCount <= 4)
            {
                analysis.ShapeType = ShapeType.Standard;
            }
            else
            {
                analysis.ShapeType = ShapeType.Complex;
            }

            // 대칭성 검사
            analysis.IsSymmetric = CheckSymmetry(shape);

            return analysis;
        }

        /// <summary>
        /// Shape의 대칭성 검사
        /// </summary>
        private bool CheckSymmetry(Vector2Int[] shape)
        {
            // 간단한 대칭성 검사 (완전한 구현은 복잡하므로 기본적인 체크만)
            // 정사각형이나 십자형 등의 대칭적인 형태인지 확인
            return shape.Length == 4; // 임시로 4개 블록이면 대칭적이라고 가정
        }

        /// <summary>
        /// 정사각형 형태의 Wall Kick (회전 불필요)
        /// </summary>
        private Vector2Int[] GetSquareWallKickOffsets()
        {
            return new Vector2Int[] { Vector2Int.zero };
        }

        /// <summary>
        /// 직선 형태의 Wall Kick
        /// </summary>
        private Vector2Int[] GetLineWallKickOffsets(int fromRotation, int toRotation, bool clockwise)
        {
            // 직선형은 더 넓은 범위의 Wall Kick 필요
            List<Vector2Int> offsets = new List<Vector2Int>
            {
                Vector2Int.zero,        // 원래 위치
                Vector2Int.left,        // 좌측으로 1칸
                Vector2Int.right,       // 우측으로 1칸
                new Vector2Int(-2, 0),  // 좌측으로 2칸
                new Vector2Int(2, 0),   // 우측으로 2칸
                Vector2Int.down,        // 아래로 1칸
                new Vector2Int(0, -2),  // 아래로 2칸
                new Vector2Int(-1, -1), // 좌측 아래
                new Vector2Int(1, -1),  // 우측 아래
            };

            // 수평 ↔ 수직 회전 시 특별한 처리
            bool isHorizontalToVertical = IsHorizontalToVerticalRotation(fromRotation, toRotation);
            bool isVerticalToHorizontal = IsVerticalToHorizontalRotation(fromRotation, toRotation);

            if (isHorizontalToVertical)
            {
                // 수평에서 수직으로 회전 시 좌우 이동을 더 우선시
                offsets.Insert(1, new Vector2Int(-1, 0));
                offsets.Insert(2, new Vector2Int(1, 0));
            }
            else if (isVerticalToHorizontal)
            {
                // 수직에서 수평으로 회전 시 위아래 이동을 더 우선시
                offsets.Insert(1, new Vector2Int(0, 1));
                offsets.Insert(2, new Vector2Int(0, -1));
            }

            return offsets.ToArray();
        }

        /// <summary>
        /// 복잡한 형태의 Wall Kick
        /// </summary>
        private Vector2Int[] GetComplexWallKickOffsets(ShapeAnalysis analysis, int fromRotation, int toRotation, bool clockwise)
        {
            // 복잡한 형태는 더 많은 Wall Kick 시도
            List<Vector2Int> offsets = new List<Vector2Int>
            {
                Vector2Int.zero,        // 원래 위치
                Vector2Int.left,        // 좌측으로 1칸
                Vector2Int.right,       // 우측으로 1칸
                Vector2Int.down,        // 아래로 1칸
                new Vector2Int(-2, 0),  // 좌측으로 2칸
                new Vector2Int(2, 0),   // 우측으로 2칸
                new Vector2Int(0, -2),  // 아래로 2칸
                new Vector2Int(-1, -1), // 좌측 아래
                new Vector2Int(1, -1),  // 우측 아래
                new Vector2Int(-2, -1), // 좌측 아래 2칸
                new Vector2Int(2, -1),  // 우측 아래 2칸
            };

            // 바운딩 박스 크기에 따른 추가 오프셋
            int maxSize = Mathf.Max(analysis.BoundingBox.x, analysis.BoundingBox.y);
            if (maxSize > 3)
            {
                // 큰 형태일 경우 더 넓은 범위의 Wall Kick
                offsets.Add(new Vector2Int(-3, 0));
                offsets.Add(new Vector2Int(3, 0));
                offsets.Add(new Vector2Int(0, -3));
            }

            return offsets.ToArray();
        }

        /// <summary>
        /// 일반적인 테트리미노 형태의 Wall Kick
        /// </summary>
        private Vector2Int[] GetStandardWallKickOffsets(int fromRotation, int toRotation, bool clockwise)
        {
            // 기본 우선순위: 원래 위치 → 좌측 → 우측 → 아래 → 좌측 아래 → 우측 아래
            List<Vector2Int> offsets = new List<Vector2Int>
            {
                Vector2Int.zero,        // 원래 위치
                Vector2Int.left,        // 좌측으로 1칸
                Vector2Int.right,       // 우측으로 1칸
                Vector2Int.down,        // 아래로 1칸
                new Vector2Int(-1, -1), // 좌측 아래
                new Vector2Int(1, -1),  // 우측 아래
            };

            // 회전 방향에 따른 추가 오프셋
            if (clockwise)
            {
                // 시계방향 회전 시 우측 우선
                offsets.Insert(2, new Vector2Int(2, 0));  // 우측으로 2칸
                offsets.Insert(4, new Vector2Int(-2, 0)); // 좌측으로 2칸
            }
            else
            {
                // 반시계방향 회전 시 좌측 우선
                offsets.Insert(2, new Vector2Int(-2, 0)); // 좌측으로 2칸
                offsets.Insert(4, new Vector2Int(2, 0));  // 우측으로 2칸
            }

            return offsets.ToArray();
        }

        /// <summary>
        /// 수평에서 수직으로 회전하는지 확인
        /// </summary>
        private bool IsHorizontalToVerticalRotation(int fromRotation, int toRotation)
        {
            // 0도(수평) → 90도(수직) 또는 180도(수평) → 270도(수직)
            return (fromRotation == 0 && toRotation == 1) ||
                   (fromRotation == 2 && toRotation == 3);
        }

        /// <summary>
        /// 수직에서 수평으로 회전하는지 확인
        /// </summary>
        private bool IsVerticalToHorizontalRotation(int fromRotation, int toRotation)
        {
            // 90도(수직) → 180도(수평) 또는 270도(수직) → 0도(수평)
            return (fromRotation == 1 && toRotation == 2) ||
                   (fromRotation == 3 && toRotation == 0);
        }
    }
}