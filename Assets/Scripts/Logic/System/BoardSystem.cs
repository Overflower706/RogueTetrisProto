using System.Collections.Generic;
using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class BoardSystem : ITickSystem
    {
        public Context Context { get; set; }

        public void Tick()
        {
            var startEntities = Context.GetEntitiesWithComponent<StartGameCommand>();
            if (startEntities.Count > 0)
            {
                // 초기 보드 설정 로직
                var board = GetBoard();
                board.Board = new int[BoardComponent.WIDTH, BoardComponent.HEIGHT];
                for (int x = 0; x < BoardComponent.WIDTH; x++)
                {
                    for (int y = 0; y < BoardComponent.HEIGHT; y++)
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
                var board = GetBoard();
                board.Board = new int[BoardComponent.WIDTH, BoardComponent.HEIGHT];
                for (int x = 0; x < BoardComponent.WIDTH; x++)
                {
                    for (int y = 0; y < BoardComponent.HEIGHT; y++)
                    {
                        board.Board[x, y] = 0; // 빈 칸으로 초기화
                    }
                }

                return; // 게임이 종료되면 더 이상 처리하지 않음
            }

            var state = GetState();
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

            var bakeEntities = Context.GetEntitiesWithComponent<BakeCommand>();
            if (bakeEntities.Count > 0)
            {
                var bakeCommand = bakeEntities[0].GetComponent<BakeCommand>();
                var index = bakeCommand.Index;

                ProcessBake(currentEntity, index);
            }

            var trashEntities = Context.GetEntitiesWithComponent<TrashCommand>();
            if (trashEntities.Count > 0)
            {
                var trashCommand = trashEntities[0].GetComponent<TrashCommand>();
                var index = trashCommand.Index;

                ProcessTrashLine(currentEntity, index);
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

        private GameStateComponent GetState()
        {
            var stateEntities = Context.GetEntitiesWithComponent<GameStateComponent>();

            if (stateEntities.Count == 0)
            {
                Debug.LogWarning("GameStateComponent가 있는 엔티티가 없습니다.");
                return null;
            }
            else if (stateEntities.Count > 1)
            {
                Debug.LogWarning("GameStateComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return null;
            }

            return stateEntities[0].GetComponent<GameStateComponent>();
        }

        /// <summary>
        /// CurrentTetriminoComponent를 가진 엔티티 찾기
        /// </summary>
        private Entity GetCurrentTetriminoEntity()
        {
            var tetriminoEntities = Context.GetEntitiesWithComponent<BoardTetriminoComponent>();
            foreach (var entity in tetriminoEntities)
            {
                var tetriminoComponent = entity.GetComponent<BoardTetriminoComponent>();
                if (tetriminoComponent.State == BoardTetriminoState.Current)
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
            var worldPositions = GetTetriminoWorldPositions(tetrimino, newPosition);

            foreach (var position in worldPositions)
            {
                // 보드 경계 체크
                if (!IsValidPosition(position))
                {
                    return false;
                }

                var board = GetBoard();

                // 고정된 블록과 충돌 체크 (0이 아닌 다른 Entity ID가 있으면 충돌)
                // 단, 현재 테트리미노 자신의 ID는 제외 (이동 중이므로)

                if (board.Board[position.x, position.y] > 0 && board.Board[position.x, position.y] != tetrimino.ID)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 특정 위치 기준으로 테트리미노의 실제 월드 위치들 계산
        /// </summary>
        private Vector2Int[] GetTetriminoWorldPositions(Entity tetrimino, Vector2Int position)
        {
            var tetriminoComponent = tetrimino.GetComponent<TetriminoComponent>();
            var boardTetrimino = tetrimino.GetComponent<BoardTetriminoComponent>();

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
            return position.x >= 0 && position.x < BoardComponent.WIDTH &&
                   position.y >= 0 && position.y < BoardComponent.HEIGHT;
        }

        /// <summary>
        /// 실제 이동 처리
        /// </summary>
        private void ProcessMove(Entity tetriminoEntity, bool isRight)
        {
            ClearTetriminoFromBoard(tetriminoEntity);

            var board = GetBoard();

            var tetriminoComponent = tetriminoEntity.GetComponent<TetriminoComponent>();
            var boardTetrimino = tetriminoEntity.GetComponent<BoardTetriminoComponent>();

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

            var board = GetBoard();
            var tetriminoComponent = tetriminoEntity.GetComponent<TetriminoComponent>();
            var boardTetrimino = tetriminoEntity.GetComponent<BoardTetriminoComponent>();

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

            var tetriminoComponent = tetriminoEntity.GetComponent<TetriminoComponent>();
            var boardTetrimino = tetriminoEntity.GetComponent<BoardTetriminoComponent>();

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

            var board = GetBoard();

            var tetriminoComponent = tetriminoEntity.GetComponent<TetriminoComponent>();
            var boardTetrimino = tetriminoEntity.GetComponent<BoardTetriminoComponent>();

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

        private void ProcessBake(Entity tetriminoEntity, int index)
        {
            var player = GetPlayer();

            if (player.BakeCount <= 0)
            {
                Debug.LogWarning($"베이킹 횟수가 부족합니다. 현재 베이킹 횟수: {player.BakeCount}");
                return; // 베이킹 횟수가 부족하면 처리 중단
            }

            player.BakeCount--;

            ClearTetriminoFromBoard(tetriminoEntity);

            var board = GetBoard();
            int[] bakedRow = new int[BoardComponent.WIDTH];

            // 1. 지정된 줄을 0으로 클리어
            for (int x = 0; x < BoardComponent.WIDTH; x++)
            {
                bakedRow[x] = board.Board[x, index];
                board.Board[x, index] = 0;
            }

            Context.CreateEntity().AddComponent(new BakedComponent
            {
                BakedRow = bakedRow
            });

            // 2. 기존 DropLinesDown 메서드를 활용하여 위의 블록들을 아래로 드롭
            var clearedLines = new List<int> { index };
            DropLinesDown(board, clearedLines);

            Debug.Log($"굽기: {index}줄 삭제 및 드롭 완료");
        }

        private void ProcessTrashLine(Entity tetriminoEntity, int index)
        {
            var player = GetPlayer();

            if (player.TrashCount <= 0)
            {
                Debug.LogWarning($"폐기 횟수가 부족합니다. 현재 폐기 횟수: {player.TrashCount}");
                return; // 폐기 횟수가 부족하면 처리 중단
            }

            player.TrashCount--;

            ClearTetriminoFromBoard(tetriminoEntity);

            var board = GetBoard();

            // 1. 지정된 줄을 0으로 클리어
            for (int x = 0; x < BoardComponent.WIDTH; x++)
            {
                board.Board[x, index] = 0;
            }

            // 2. 기존 DropLinesDown 메서드를 활용하여 위의 블록들을 아래로 드롭
            var clearedLines = new List<int> { index };
            DropLinesDown(board, clearedLines);

            Debug.Log($"폐기: {index}줄 삭제 및 드롭 완료");
        }

        /// <summary>
        /// Board에 테트리미노 표시 (Entity ID로 저장)
        /// </summary>
        private void MarkTetriminoToBoard(Entity tetriminoEntity)
        {
            var board = GetBoard();

            var tetriminoComponent = tetriminoEntity.GetComponent<TetriminoComponent>();
            var boardTetrimino = tetriminoEntity.GetComponent<BoardTetriminoComponent>();

            // Shape과 Rotation 기반으로 실제 블록 위치들 계산
            var blockPositions = GetTetriminoWorldPositions(tetriminoEntity, boardTetrimino.Position);

            foreach (var position in blockPositions)
            {
                if (IsValidPosition(position))
                {
                    board.Board[position.x, position.y] = tetriminoEntity.ID; // Entity ID로 테트리미노 표시
                }
            }
        }

        private void ClearTetriminoFromBoard(Entity tetriminoEntity)
        {
            var board = GetBoard();

            var tetriminoComponent = tetriminoEntity.GetComponent<TetriminoComponent>();
            var boardTetrimino = tetriminoEntity.GetComponent<BoardTetriminoComponent>();

            // Shape과 Rotation 기반으로 실제 블록 위치들 계산
            var blockPositions = GetTetriminoWorldPositions(tetriminoEntity, boardTetrimino.Position);

            foreach (var position in blockPositions)
            {
                if (IsValidPosition(position))
                {
                    board.Board[position.x, position.y] = 0; // 빈 칸으로 설정
                }
            }
        }

        /// <summary>
        /// 테트리미노를 고정하고 새로운 테트리미노 생성 요청
        /// </summary>
        private void FixTetrimino(Entity tetriminoEntity)
        {
            // 현재 위치에 테트리미노 표시 (고정)
            tetriminoEntity.RemoveComponent<BoardTetriminoComponent>();
            Debug.Log("Board System : 테트리스 고정할게~");

            // 게임 오버 감지 (테트리미노 고정 직후)
            CheckAndHandleGameOver(tetriminoEntity);

            // 줄 완성 검증 및 제거
            // var (clearedLines, completedLinesColors) = CheckAndClearCompletedLines();
            // if (clearedLines > 0)
            // {
            //     Debug.Log($"완성된 줄 {clearedLines}개 제거됨");

            //     // ScoreSystem에 줄 클리어 이벤트 전달
            //     var scoreCommandRequest = GetCommandRequestComponent();
            //     if (scoreCommandRequest != null)
            //     {
            //         scoreCommandRequest.Requests.Enqueue(new CommandRequest
            //         {
            //             Type = CommandType.LineClear,
            //             PayLoad = (clearedLines, tetriminoEntity.ID, completedLinesColors)
            //         });
            //         Debug.Log($"줄 클리어 이벤트 전송: {clearedLines}줄, 테트리미노 ID: {tetriminoEntity.ID}");
            //     }
            // }

            // GenerateTetriminoCommand를 통해 새로운 테트리미노 생성 요청
            // var commandRequest = GetCommandRequestComponent();
            // if (commandRequest != null)
            // {
            //     commandRequest.Requests.Enqueue(new CommandRequest
            //     {
            //         Type = CommandType.GenerateTetrimino,
            //         PayLoad = null
            //     });
            //     Debug.Log("새로운 테트리미노 생성 요청됨");
            // }
        }

        // Legacy

        /// <summary>
        /// 완성된 줄 검증 및 제거
        /// </summary>
        private (int clearedCount, TetriminoColor[][] completedLinesColors) CheckAndClearCompletedLines()
        {
            var board = GetBoard();

            var completedLines = new List<int>();
            var completedLinesColors = new List<TetriminoColor[]>();

            // 아래부터 위로 검사하여 완성된 줄 찾기
            for (int y = 0; y < BoardComponent.HEIGHT; y++)
            {
                if (IsLineCompleted(board, y))
                {
                    completedLines.Add(y);

                    // 해당 줄의 색상 정보 수집
                    TetriminoColor[] lineColors = new TetriminoColor[BoardComponent.WIDTH];
                    for (int x = 0; x < BoardComponent.WIDTH; x++)
                    {
                        int entityId = board.Board[x, y];
                        if (entityId > 0)
                        {
                            // Entity ID를 통해 테트리미노 색상 찾기 (더 안전한 방법)
                            var tetriminoColor = FindTetriminoColorByEntityId(entityId);
                            lineColors[x] = tetriminoColor;
                        }
                        else
                        {
                            lineColors[x] = TetriminoColor.Red; // 기본값 (실제로는 발생하지 않아야 함)
                        }
                    }
                    completedLinesColors.Add(lineColors);
                }
            }

            // 완성된 줄이 있으면 제거
            if (completedLines.Count > 0)
            {
                ClearCompletedLines(board, completedLines);
                DropLinesDown(board, completedLines);
            }

            return (completedLines.Count, completedLinesColors.ToArray());
        }

        /// <summary>
        /// 특정 줄이 완성되었는지 검사
        /// </summary>
        private bool IsLineCompleted(BoardComponent board, int lineY)
        {
            for (int x = 0; x < BoardComponent.WIDTH; x++)
            {
                if (board.Board[x, lineY] == 0) // 빈 칸이 있으면 완성되지 않음
                {
                    return false;
                }
            }
            return true; // 모든 칸이 채워짐
        }

        /// <summary>
        /// 완성된 줄들을 제거 (0으로 설정)
        /// </summary>
        private void ClearCompletedLines(BoardComponent board, List<int> completedLines)
        {
            foreach (int lineY in completedLines)
            {
                for (int x = 0; x < BoardComponent.WIDTH; x++)
                {
                    board.Board[x, lineY] = 0;
                }
                Debug.Log($"줄 {lineY} 제거됨");
            }
        }

        /// <summary>
        /// 제거된 줄 위의 블록들을 아래로 떨어뜨리기
        /// </summary>
        private void DropLinesDown(BoardComponent board, List<int> clearedLines)
        {
            // 제거되지 않은 줄들을 아래부터 다시 배치
            var remainingLines = new List<int[]>();

            // 아래부터 위로 검사하면서 제거되지 않은 줄들을 수집
            for (int y = 0; y < BoardComponent.HEIGHT; y++)
            {
                if (!clearedLines.Contains(y))
                {
                    // 이 줄은 제거되지 않았으므로 보존
                    int[] line = new int[BoardComponent.WIDTH];
                    for (int x = 0; x < BoardComponent.WIDTH; x++)
                    {
                        line[x] = board.Board[x, y];
                    }
                    remainingLines.Add(line);
                }
            }

            // 보드를 모두 0으로 초기화
            for (int y = 0; y < BoardComponent.HEIGHT; y++)
            {
                for (int x = 0; x < BoardComponent.WIDTH; x++)
                {
                    board.Board[x, y] = 0;
                }
            }

            // 남은 줄들을 아래부터 다시 배치
            for (int i = 0; i < remainingLines.Count; i++)
            {
                for (int x = 0; x < BoardComponent.WIDTH; x++)
                {
                    board.Board[x, i] = remainingLines[i][x];
                }
            }

            Debug.Log($"줄 드롭 완료: {clearedLines.Count}줄 제거됨, {remainingLines.Count}줄 남음");
        }

        private PlayerComponent GetPlayer()
        {
            var playerEntities = Context.GetEntitiesWithComponent<PlayerComponent>();

            if (playerEntities.Count == 0)
            {
                Debug.LogWarning("PlayerComponent가 있는 엔티티가 없습니다.");
                return null;
            }
            else if (playerEntities.Count > 1)
            {
                Debug.LogWarning("PlayerComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return null;
            }

            return playerEntities[0].GetComponent<PlayerComponent>();
        }

        private CommandRequestComponent GetCommandRequestComponent()
        {
            var commandEntities = Context.GetEntitiesWithComponent<CommandRequestComponent>();

            if (commandEntities.Count == 0)
            {
                Debug.LogWarning("CommandRequestComponent가 있는 엔티티가 없습니다.");
                return null;
            }
            else if (commandEntities.Count > 1)
            {
                Debug.LogWarning("CommandRequestComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return null;
            }

            return commandEntities[0].GetComponent<CommandRequestComponent>();
        }

        /// <summary>
        /// Entity ID로 테트리미노 색상 찾기 (안전한 방법)
        /// </summary>
        private TetriminoColor FindTetriminoColorByEntityId(int entityId)
        {
            try
            {
                // 모든 TetriminoComponent를 가진 엔티티들을 순회
                var tetriminoEntities = Context.GetEntitiesWithComponent<TetriminoComponent>();
                foreach (var entity in tetriminoEntities)
                {
                    if (entity.ID == entityId)
                    {
                        var tetriminoComponent = entity.GetComponent<TetriminoComponent>();
                        return tetriminoComponent?.Color ?? TetriminoColor.Red;
                    }
                }

                Debug.LogWarning($"Entity ID {entityId}에 해당하는 TetriminoComponent를 찾을 수 없음");
                return TetriminoColor.Red; // 기본값
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"FindTetriminoColorByEntityId 오류: {ex.Message}");
                return TetriminoColor.Red; // 기본값
            }
        }

        #region Game Over Detection System

        /// <summary>
        /// 게임 오버 상태 감지 및 처리
        /// </summary>
        private void CheckAndHandleGameOver(Entity tetriminoEntity)
        {
            var gameStateComponent = GetState();
            if (gameStateComponent == null || gameStateComponent.CurrentState != GameState.Playing)
            {
                return;
            }

            // 하이브리드 게임 오버 감지
            // 1. 높이 기반 감지 (버퍼 존 포함)
            if (IsGameOverByHeight())
            {
                Debug.Log("게임 오버: 높이 기반 감지 (버퍼 존 초과)");
                TriggerGameOver();
                return;
            }

            // 2. 스폰 위치 충돌 감지
            if (IsGameOverBySpawnCollision())
            {
                Debug.Log("게임 오버: 스폰 위치 충돌 감지");
                TriggerGameOver();
                return;
            }
        }

        /// <summary>
        /// 높이 기반 게임 오버 감지 (버퍼 존 포함)
        /// </summary>
        private bool IsGameOverByHeight()
        {
            var board = GetBoard();

            // 상위 2줄(18, 19줄)에 블록이 있는지 확인 (버퍼 존)
            for (int y = BoardComponent.HEIGHT - 2; y < BoardComponent.HEIGHT; y++)
            {
                for (int x = 0; x < BoardComponent.WIDTH; x++)
                {
                    if (board.Board[x, y] != 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 스폰 위치 충돌 기반 게임 오버 감지
        /// </summary>
        private bool IsGameOverBySpawnCollision()
        {
            var board = GetBoard();

            // 일반적인 테트리미노 스폰 위치 (중앙 상단)
            Vector2Int spawnPosition = new Vector2Int(BoardComponent.WIDTH / 2, BoardComponent.HEIGHT - 1);

            // 스폰 위치와 주변 영역 확인
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 0; dy++)
                {
                    int x = spawnPosition.x + dx;
                    int y = spawnPosition.y + dy;

                    if (x >= 0 && x < BoardComponent.WIDTH && y >= 0 && y < BoardComponent.HEIGHT)
                    {
                        if (board.Board[x, y] != 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 게임 오버 상태 트리거
        /// </summary>
        private void TriggerGameOver()
        {
            var gameStateComponent = GetState();
            if (gameStateComponent != null)
            {
                gameStateComponent.CurrentState = GameState.GameOver;
                Debug.Log("게임 오버 상태로 전환됨");
            }

            // 게임 오버 이벤트 전송
            var commandRequest = GetCommandRequestComponent();
            if (commandRequest != null)
            {
                commandRequest.Requests.Enqueue(new CommandRequest
                {
                    Type = CommandType.EndGame,
                    PayLoad = null
                });
                Debug.Log("게임 오버 이벤트 전송됨");
            }
        }

        #endregion

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

        /// <summary>
        /// GameStateComponent 가져오기
        /// </summary>
        private GameStateComponent GetGameStateComponent()
        {
            var gameStateEntities = Context.GetEntitiesWithComponent<GameStateComponent>();

            if (gameStateEntities.Count == 0)
            {
                Debug.LogWarning("GameStateComponent가 있는 엔티티가 없습니다.");
                return null;
            }
            else if (gameStateEntities.Count > 1)
            {
                Debug.LogWarning("GameStateComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
                return null;
            }

            return gameStateEntities[0].GetComponent<GameStateComponent>();
        }
    }
}