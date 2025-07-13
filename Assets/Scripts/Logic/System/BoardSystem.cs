using OVFL.ECS;
using UnityEngine;

namespace Minomino
{
    public class BoardSystem : ITickSystem
    {
        public Context Context { get; set; }

        public void Tick(Context context)
        {
            var board = GetBoardComponent(context);
            if (board == null) return;

            var startEntities = context.GetEntitiesWithComponent<StartGameCommand>();
            if (startEntities.Count > 0)
            {
                // 초기 보드 설정 로직
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

            var moveLeftEntities = context.GetEntitiesWithComponent<MoveLeftCommand>();
            if (moveLeftEntities.Count > 0)
            {
                ProcessMoveCommand(Context, CommandType.MoveLeft);
            }

            var moveRightEntities = context.GetEntitiesWithComponent<MoveRightCommand>();
            if (moveRightEntities.Count > 0)
            {
                ProcessMoveCommand(Context, CommandType.MoveRight);
            }

            var softDropEntities = context.GetEntitiesWithComponent<SoftDropCommand>();
            if (softDropEntities.Count > 0)
            {
                ProcessSoftDrop();
            }

            var hardDropEntities = context.GetEntitiesWithComponent<HardDropCommand>();
            if (hardDropEntities.Count > 0)
            {
                ProcessHardDrop();
            }

            var rotateClockwiseEntities = context.GetEntitiesWithComponent<RotateClockwiseCommand>();
            if (rotateClockwiseEntities.Count > 0)
            {
                ProcessRotateCommand(true); // true = 시계방향
            }

            var rotateCounterClockwiseEntities = context.GetEntitiesWithComponent<RotateCounterClockwiseCommand>();
            if (rotateCounterClockwiseEntities.Count > 0)
            {
                ProcessRotateCommand(false); // false = 반시계방향
            }

            // CurrentTetriminoComponent를 찾아서 Board에 표시
            var currentTetrimino = GetCurrentTetrimino();
            if (currentTetrimino != null)
            {
                DisplayTetriminoOnBoard(board, currentTetrimino, context);
            }

            var holdTetriminoEntities = context.GetEntitiesWithComponent<HoldTetriminoCommand>();
            if (holdTetriminoEntities.Count > 0)
            {
                ProcessHoldCommand();
            }
        }

        private BoardComponent GetBoardComponent(Context context)
        {
            var boardEntities = context.GetEntitiesWithComponent<BoardComponent>();

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

        /// <summary>
        /// CurrentTetriminoComponent를 가진 엔티티 찾기
        /// </summary>
        private Entity GetCurrentTetriminoEntity(Context context)
        {
            var currentTetriminoEntities = context.GetEntitiesWithComponent<CurrentTetriminoComponent>();

            if (currentTetriminoEntities.Count == 0)
            {
                return null;
            }

            return currentTetriminoEntities[0];
        }

        /// <summary>
        /// CurrentTetriminoComponent 가져오기
        /// </summary>
        private CurrentTetriminoComponent GetCurrentTetrimino()
        {
            var entity = GetCurrentTetriminoEntity(Context);
            return entity?.GetComponent<CurrentTetriminoComponent>();
        }

        /// <summary>
        /// Board에 테트리미노 표시 (Entity ID로 저장)
        /// </summary>
        private void DisplayTetriminoOnBoard(BoardComponent board, CurrentTetriminoComponent currentTetrimino, Context context)
        {
            var currentEntity = GetCurrentTetriminoEntity(context);
            if (currentEntity == null) return;

            var tetriminoComponent = currentEntity.GetComponent<TetriminoComponent>();
            if (tetriminoComponent == null) return;

            // Shape과 Rotation 기반으로 실제 블록 위치들 계산
            var blockPositions = GetTetriminoWorldPositions(tetriminoComponent, currentTetrimino.Position);

            // Board에 Entity ID 기록
            int entityId = currentEntity.ID;

            foreach (var pos in blockPositions)
            {
                if (IsValidPosition(pos))
                {
                    board.Board[pos.x, pos.y] = entityId;
                }
            }
        }

        /// <summary>
        /// 테트리미노의 실제 월드 위치들 계산
        /// </summary>
        private Vector2Int[] GetTetriminoWorldPositions(TetriminoComponent tetrimino, Vector2Int position)
        {
            Vector2Int[] worldPositions = new Vector2Int[tetrimino.Shape.Length];

            for (int i = 0; i < tetrimino.Shape.Length; i++)
            {
                Vector2Int rotatedShape = RotateShape(tetrimino.Shape[i], tetrimino.Rotation);
                worldPositions[i] = position + rotatedShape;
            }

            return worldPositions;
        }

        /// <summary>
        /// Shape를 회전시키는 메서드
        /// </summary>
        private Vector2Int RotateShape(Vector2Int point, int rotation)
        {
            // 90도씩 시계방향 회전
            for (int i = 0; i < rotation; i++)
            {
                int temp = point.x;
                point.x = -point.y;
                point.y = temp;
            }
            return point;
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
        private void ProcessMoveCommand(Context context, CommandType commandType)
        {
            var board = GetBoardComponent(context);
            var currentTetrimino = GetCurrentTetrimino();
            var currentEntity = GetCurrentTetriminoEntity(context);

            if (board == null || currentTetrimino == null || currentEntity == null)
                return;

            var tetriminoComponent = currentEntity.GetComponent<TetriminoComponent>();
            if (tetriminoComponent == null) return;

            // 현재 위치에서 테트리미노 제거
            ClearTetriminoFromBoard(board, currentTetrimino, tetriminoComponent, currentEntity);

            // 이동 방향 결정
            Vector2Int moveDirection = Vector2Int.zero;
            switch (commandType)
            {
                case CommandType.MoveLeft:
                    moveDirection = Vector2Int.left;
                    break;
                case CommandType.MoveRight:
                    moveDirection = Vector2Int.right;
                    break;
            }

            // 새 위치 계산
            Vector2Int newPosition = currentTetrimino.Position + moveDirection;

            // 충돌 검사
            if (CanMoveTo(board, tetriminoComponent, newPosition, context))
            {
                // 이동 가능하면 위치 업데이트
                currentTetrimino.Position = newPosition;
                Debug.Log($"Tetrimino {commandType} 이동 성공: {newPosition}");
            }
            else
            {
                Debug.Log($"Tetrimino {commandType} 이동 실패: 충돌 또는 경계");
            }

            // 새 위치에 테트리미노 표시
            DisplayTetriminoOnBoard(board, currentTetrimino, context);
        }

        /// <summary>
        /// Board에서 테트리미노 제거
        /// </summary>
        private void ClearTetriminoFromBoard(BoardComponent board, CurrentTetriminoComponent currentTetrimino, TetriminoComponent tetriminoComponent, Entity currentEntity)
        {
            var blockPositions = GetTetriminoWorldPositions(tetriminoComponent, currentTetrimino.Position);
            int entityId = currentEntity.ID;

            foreach (var pos in blockPositions)
            {
                if (IsValidPosition(pos) && board.Board[pos.x, pos.y] == entityId)
                {
                    board.Board[pos.x, pos.y] = 0; // 빈 칸으로 설정
                }
            }
        }

        /// <summary>
        /// 해당 위치로 이동 가능한지 확인
        /// </summary>
        private bool CanMoveTo(BoardComponent board, TetriminoComponent tetrimino, Vector2Int newPosition, Context context)
        {
            var blockPositions = GetTetriminoWorldPositions(tetrimino, newPosition);

            foreach (var pos in blockPositions)
            {
                // 보드 경계 체크
                if (!IsValidPosition(pos))
                {
                    return false;
                }

                // 고정된 블록과 충돌 체크 (0이 아닌 다른 Entity ID가 있으면 충돌)
                // 단, 현재 테트리미노 자신의 ID는 제외 (이동 중이므로)
                var currentEntity = GetCurrentTetriminoEntity(Context);
                int currentEntityId = currentEntity?.ID ?? -1;

                if (board.Board[pos.x, pos.y] > 0 && board.Board[pos.x, pos.y] != currentEntityId)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// SoftDrop 처리 - 테트리미노를 한 칸 아래로 이동
        /// </summary>
        private void ProcessSoftDrop()
        {
            var board = GetBoardComponent(Context);
            var currentTetrimino = GetCurrentTetrimino();
            var currentEntity = GetCurrentTetriminoEntity(Context);

            if (board == null || currentTetrimino == null || currentEntity == null)
                return;

            var tetriminoComponent = currentEntity.GetComponent<TetriminoComponent>();
            if (tetriminoComponent == null) return;

            // 현재 위치에서 테트리미노 제거
            ClearTetriminoFromBoard(board, currentTetrimino, tetriminoComponent, currentEntity);

            // 아래로 한 칸 이동 시도
            Vector2Int newPosition = currentTetrimino.Position + Vector2Int.down;

            if (CanMoveTo(board, tetriminoComponent, newPosition, Context))
            {
                // 이동 가능하면 위치 업데이트
                currentTetrimino.Position = newPosition;
                Debug.Log($"SoftDrop 성공: {newPosition}");
            }
            else
            {
                // 이동 불가능하면 현재 위치에 고정
                Debug.Log("SoftDrop 실패: 바닥 또는 충돌 - 테트리미노 고정");
                FixTetrimino(board, currentTetrimino, tetriminoComponent, currentEntity);
                return; // 고정 후에는 DisplayTetriminoOnBoard 호출하지 않음
            }

            // 새 위치에 테트리미노 표시
            DisplayTetriminoOnBoard(board, currentTetrimino, Context);
        }

        /// <summary>
        /// HardDrop 처리 - 테트리미노를 바닥까지 즉시 이동
        /// </summary>
        private void ProcessHardDrop()
        {
            var board = GetBoardComponent(Context);
            var currentTetrimino = GetCurrentTetrimino();
            var currentEntity = GetCurrentTetriminoEntity(Context);

            if (board == null || currentTetrimino == null || currentEntity == null)
                return;

            var tetriminoComponent = currentEntity.GetComponent<TetriminoComponent>();
            if (tetriminoComponent == null) return;

            // 현재 위치에서 테트리미노 제거
            ClearTetriminoFromBoard(board, currentTetrimino, tetriminoComponent, currentEntity);

            // 가능한 가장 아래까지 이동
            Vector2Int newPosition = currentTetrimino.Position;
            Vector2Int testPosition = newPosition;

            // 아래로 계속 이동하면서 충돌하지 않는 가장 낮은 위치 찾기
            while (true)
            {
                testPosition = newPosition + Vector2Int.down;
                if (CanMoveTo(board, tetriminoComponent, testPosition, Context))
                {
                    newPosition = testPosition;
                }
                else
                {
                    break; // 더 이상 이동할 수 없으면 중단
                }
            }

            // 최종 위치로 이동
            currentTetrimino.Position = newPosition;
            Debug.Log($"HardDrop 성공: {newPosition}");

            // HardDrop 후에는 즉시 테트리미노를 고정하고 새로운 테트리미노 생성
            FixTetrimino(board, currentTetrimino, tetriminoComponent, currentEntity);
        }

        /// <summary>
        /// 테트리미노를 고정하고 새로운 테트리미노 생성 요청
        /// </summary>
        private void FixTetrimino(BoardComponent board, CurrentTetriminoComponent currentTetrimino, TetriminoComponent tetriminoComponent, Entity currentEntity)
        {
            // 현재 위치에 테트리미노 표시 (고정)
            DisplayTetriminoOnBoard(board, currentTetrimino, Context);

            // 현재 테트리미노 Entity에서 CurrentTetriminoComponent 제거
            currentEntity.RemoveComponent<CurrentTetriminoComponent>();

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
        /// 회전 처리 - 테트리미노를 시계방향 또는 반시계방향으로 회전
        /// </summary>
        private void ProcessRotateCommand(bool clockwise)
        {
            var board = GetBoardComponent(Context);
            var currentTetrimino = GetCurrentTetrimino();
            var currentEntity = GetCurrentTetriminoEntity(Context);

            if (board == null || currentTetrimino == null || currentEntity == null)
                return;

            var tetriminoComponent = currentEntity.GetComponent<TetriminoComponent>();
            if (tetriminoComponent == null) return;

            // 현재 위치에서 테트리미노 제거
            ClearTetriminoFromBoard(board, currentTetrimino, tetriminoComponent, currentEntity);

            // 회전 계산
            int originalRotation = tetriminoComponent.Rotation;
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
            tetriminoComponent.Rotation = newRotation;

            // 회전 후 충돌 검사
            if (CanMoveTo(board, tetriminoComponent, currentTetrimino.Position, Context))
            {
                // 회전 가능하면 성공
                Debug.Log($"Tetrimino 회전 성공: {(clockwise ? "시계방향" : "반시계방향")} - 새 회전: {newRotation}");
            }
            else
            {
                // 회전 불가능하면 원래 회전으로 되돌림
                tetriminoComponent.Rotation = originalRotation;
                Debug.Log($"Tetrimino 회전 실패: {(clockwise ? "시계방향" : "반시계방향")} - 충돌 또는 경계");
            }

            // 새 상태로 테트리미노 표시
            DisplayTetriminoOnBoard(board, currentTetrimino, Context);
        }

        /// <summary>
        /// Hold 처리 - 현재 테트리미노를 보드에서 제거
        /// </summary>
        private void ProcessHoldCommand()
        {
            var board = GetBoardComponent(Context);
            var currentTetrimino = GetCurrentTetrimino();
            var currentEntity = GetCurrentTetriminoEntity(Context);

            if (board == null || currentTetrimino == null || currentEntity == null)
                return;

            var tetriminoComponent = currentEntity.GetComponent<TetriminoComponent>();
            if (tetriminoComponent == null) return;

            // 보드에서 현재 테트리미노 제거
            ClearTetriminoFromBoard(board, currentTetrimino, tetriminoComponent, currentEntity);

            Debug.Log("Hold: 보드에서 현재 테트리미노 제거 완료");
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
    }
}