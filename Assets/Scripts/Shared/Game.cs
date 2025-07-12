using UnityEngine;
using System.Collections.Generic;
using Minomino;

[System.Serializable]
public class Game
{
    // 게임 상태
    public GameState CurrentState;
    public int CurrentScore;
    public int TargetScore;
    public float GameTime;

    // 테트리스 보드
    public TetrisBoard Board;

    // 현재 테트리미노
    public Tetrimino CurrentTetrimino;
    public Tetrimino NextTetrimino;

    // 파워업/효과
    public List<ActiveEffect> ActiveEffects;

    // 상점 관련
    public int Currency;
    public Game()
    {
        Board = new TetrisBoard();
        ActiveEffects = new List<ActiveEffect>();
        CurrentState = GameState.Playing;
        TargetScore = 1000; // 기본 목표 점수
        Currency = 0;
    }
}
