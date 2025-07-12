using UnityEngine;
using System.Collections.Generic;
using Minomino;

public class LogicManager : MonoSingleton<LogicManager>
{
    public Game GameData { get; private set; }
    // Logic 모듈들
    private TetrisLogic tetrisLogic;
    private ScoreLogic scoreLogic;
    private EffectLogic effectLogic;
    private ShopLogic shopLogic;

    public void Initialize()
    {
        GameData = new Game();
        tetrisLogic = new TetrisLogic(this, GameData);
        scoreLogic = new ScoreLogic(GameData);
        effectLogic = new EffectLogic(GameData);
        shopLogic = new ShopLogic(GameData, scoreLogic, effectLogic);

        Debug.Log("Hello, smile");
    }

    public void StartGame()
    {
        // 게임 초기 설정
        GameData.CurrentState = GameState.Playing;
        GameData.CurrentScore = 0;
        GameData.TargetScore = 1000; // 초기 목표 점수
        GameData.Currency = 0;
        GameData.GameTime = 0f;

        // 보드 초기화
        GameData.Board.Clear();

        // 첫 번째 테트리미노 생성
        if (tetrisLogic != null)
        {
            tetrisLogic.SpawnNewTetrimino();
        }

        Debug.Log("게임 시작! 목표 점수: " + GameData.TargetScore);
    }

    public void VictoryGame()
    {

    }

    public void LoseGame()
    {

    }

    // View에서 호출할 메서드들
    public void MoveTetrimino(Vector2Int direction)
    {
        if (GameData.CurrentState != GameState.Playing) return;
        tetrisLogic.MoveTetrimino(direction);
    }

    public void RotateTetrimino()
    {
        if (GameData.CurrentState != GameState.Playing) return;
        tetrisLogic.RotateTetrimino();
    }
    public void SoftDrop()
    {
        if (GameData.CurrentState != GameState.Playing) return;
        tetrisLogic.SoftDrop();
    }

    public void DropTetrimino()
    {
        if (GameData.CurrentState != GameState.Playing) return;
        tetrisLogic.HardDrop();
    }
    public void RestartGame()
    {
        // Logic 모듈들 다시 초기화
        tetrisLogic = new TetrisLogic(this, GameData);
        scoreLogic = new ScoreLogic(GameData);
        effectLogic = new EffectLogic(GameData);
        shopLogic = new ShopLogic(GameData, scoreLogic, effectLogic);

        // 게임 재시작
        StartGame();
    }

    // View가 읽을 Game 데이터 접근자
    public Game GetGameData() => GameData;

    // View에서 호출할 Update 메서드 (매 프레임)
    public void UpdateAutoFall(float deltaTime)
    {
        if (GameData != null && GameData.CurrentState == GameState.Playing)
        {
            GameData.GameTime += deltaTime;
            effectLogic.UpdateEffects(deltaTime);

            // 자동 낙하 처리
            tetrisLogic.UpdateAutoFall(deltaTime);
        }
    }
}
