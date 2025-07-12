using UnityEngine;
using System.Collections.Generic;
using Minomino;

public class TetrisLogic
{
    private LogicManager logicManager;
    private Game game;
    private float fallTimer;
    private float fallInterval = 1.0f; // 1초마다 자동 낙하

    public TetrisLogic(LogicManager logicManager, Game gameData)
    {
        this.logicManager = logicManager;
        this.game = gameData;
        fallTimer = 0f;
    }
    public void SpawnNewTetrimino()
    {
        // 다음 테트리미노가 있으면 현재로 이동, 없으면 새로 생성
        if (game.NextTetrimino != null)
        {
            game.CurrentTetrimino = game.NextTetrimino;
        }
        else
        {
            game.CurrentTetrimino = GenerateRandomTetrimino();
        }

        // 테트리미노를 스폰 위치에 배치 (상단 중앙)
        game.CurrentTetrimino.position = new Vector2Int(TetrisBoard.WIDTH / 2 - 1, TetrisBoard.HEIGHT - 2);

        // 새로운 다음 테트리미노 생성
        game.NextTetrimino = GenerateRandomTetrimino();

        // 게임 오버 체크 - 스폰 위치에서 배치할 수 없으면 게임 오버
        if (!CanPlaceTetrimino(game.CurrentTetrimino))
        {
            game.CurrentState = GameState.GameOver;
        }
    }

    public void MoveTetrimino(Vector2Int direction)
    {
        if (game.CurrentTetrimino == null) return;

        Vector2Int newPosition = game.CurrentTetrimino.position + direction;
        Vector2Int oldPosition = game.CurrentTetrimino.position;

        game.CurrentTetrimino.position = newPosition;

        if (!CanPlaceTetrimino(game.CurrentTetrimino))
        {
            // 이동할 수 없으면 원래 위치로 복구
            game.CurrentTetrimino.position = oldPosition;

            // 아래로 이동이 불가능하면 테트리미노 고정
            if (direction.y < 0)
            {
                PlaceTetrimino();
            }
        }
    }

    public void RotateTetrimino()
    {
        if (game.CurrentTetrimino == null) return;

        int oldRotation = game.CurrentTetrimino.rotation;
        game.CurrentTetrimino.rotation = (game.CurrentTetrimino.rotation + 1) % 4;

        if (!CanPlaceTetrimino(game.CurrentTetrimino))
        {
            // 회전할 수 없으면 원래 회전 상태로 복구
            game.CurrentTetrimino.rotation = oldRotation;
        }
    }

    public void HardDrop()
    {
        if (game.CurrentTetrimino == null) return;

        while (CanPlaceTetrimino(game.CurrentTetrimino))
        {
            game.CurrentTetrimino.position += Vector2Int.down;
        }

        // 마지막 유효한 위치로 되돌리기
        game.CurrentTetrimino.position += Vector2Int.up;
        PlaceTetrimino();
    }

    public void SoftDrop()
    {
        if (game.CurrentTetrimino == null) return;

        // 한 칸 아래로 이동 시도
        MoveTetrimino(Vector2Int.down);

        // SoftDrop 사용 시 AutoFall 타이머 초기화
        fallTimer = 0f;
    }

    public void UpdateAutoFall(float deltaTime)
    {
        fallTimer += deltaTime;

        if (fallTimer >= fallInterval)
        {
            SoftDrop(); // MoveTetrimino 대신 SoftDrop 사용
        }
    }
    private Tetrimino GenerateRandomTetrimino()
    {
        TetriminoType[] types = { TetriminoType.I, TetriminoType.O, TetriminoType.T,
                                 TetriminoType.S, TetriminoType.Z, TetriminoType.J, TetriminoType.L };
        TetriminoType randomType = types[Random.Range(0, types.Length)];
        int randomColor = Random.Range(1, 5); // 1~4 중 랜덤 색상
        return new Tetrimino(randomType, randomColor);
    }

    private bool CanPlaceTetrimino(Tetrimino tetrimino)
    {
        Vector2Int[] positions = tetrimino.GetWorldPositions();

        foreach (Vector2Int pos in positions)
        {
            if (!game.Board.IsValidPosition(pos))
            {
                return false;
            }
        }

        return true;
    }
    private void PlaceTetrimino()
    {
        if (game.CurrentTetrimino == null) return;

        Vector2Int[] positions = game.CurrentTetrimino.GetWorldPositions();

        // 보드에 테트리미노 블록 배치 (color 속성 사용)
        foreach (Vector2Int pos in positions)
        {
            game.Board.PlaceBlock(pos, game.CurrentTetrimino.color);
        }

        // 라인 클리어 체크
        CheckLineClears();

        // 새로운 테트리미노 생성
        SpawnNewTetrimino();
    }
    private void CheckLineClears()
    {
        List<int> clearedLines = new List<int>();

        // 아래쪽부터 체크
        for (int y = 0; y < TetrisBoard.HEIGHT; y++)
        {
            if (game.Board.IsLineFull(y))
            {
                clearedLines.Add(y);
            }
        }

        if (clearedLines.Count > 0)
        {
            // 점수 계산 (ScoreLogic에 위임)
            ScoreLogic scoreLogic = new ScoreLogic(game);
            scoreLogic.ProcessLineClears(clearedLines.Count, game.CurrentTetrimino);

            // 라인 클리어 실행 - 위쪽부터 역순으로 클리어 (인덱스 꼬임 방지)
            clearedLines.Sort((a, b) => b.CompareTo(a)); // 내림차순 정렬
            foreach (int line in clearedLines)
            {
                game.Board.ClearLine(line);
            }

            // 승리 조건 체크
            if (game.CurrentScore >= game.TargetScore)
            {
                game.CurrentState = GameState.Victory;
                logicManager.VictoryGame();
            }
        }
    }
}
