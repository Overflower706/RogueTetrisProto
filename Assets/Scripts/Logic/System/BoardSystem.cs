using System.Collections.Generic;
using OVFL.ECS;
using Unity.VisualScripting.YamlDotNet.Core;
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

            // 회전 후 충돌 검사
            if (CanMoveTo(tetriminoEntity, boardTetrimino.Position))
            {
                // 회전 가능하면 성공
                boardTetrimino.Rotation = newRotation; // 실제 회전 적용
                Debug.Log($"Tetrimino 회전 성공: {(clockwise ? "시계방향" : "반시계방향")} - 새 회전: {newRotation}");
            }
            else
            {
                // 회전 불가능하면 원래 회전으로 되돌림
                boardTetrimino.Rotation = originalRotation;
                Debug.Log($"Tetrimino 회전 실패: {(clockwise ? "시계방향" : "반시계방향")} - 충돌 또는 경계");
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

            // 줄 완성 검증 및 제거
            var (clearedLines, completedLinesColors) = CheckAndClearCompletedLines();
            if (clearedLines > 0)
            {
                Debug.Log($"완성된 줄 {clearedLines}개 제거됨");

                // ScoreSystem에 줄 클리어 이벤트 전달
                var scoreCommandRequest = GetCommandRequestComponent();
                if (scoreCommandRequest != null)
                {
                    scoreCommandRequest.Requests.Enqueue(new CommandRequest
                    {
                        Type = CommandType.LineClear,
                        PayLoad = (clearedLines, tetriminoEntity.ID, completedLinesColors)
                    });
                    Debug.Log($"줄 클리어 이벤트 전송: {clearedLines}줄, 테트리미노 ID: {tetriminoEntity.ID}");
                }
            }

            // GenerateTetriminoCommand를 통해 새로운 테트리미노 생성 요청
            var commandRequest = GetCommandRequestComponent();
            if (commandRequest != null)
            {
                commandRequest.Requests.Enqueue(new CommandRequest
                {
                    Type = CommandType.GenerateTetrimino,
                    PayLoad = null
                });
                Debug.Log("새로운 테트리미노 생성 요청됨");
            }
        }

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
    }
}