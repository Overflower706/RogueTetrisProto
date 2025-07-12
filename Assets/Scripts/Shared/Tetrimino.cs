using Minomino;
using UnityEngine;

[System.Serializable]
public class Tetrimino
{
    public TetriminoType type;
    public Vector2Int position;
    public int rotation; // 0, 1, 2, 3 (90도씩 회전)
    public Vector2Int[] shape; // 테트리미노의 블록 위치들
    public int color; // 1~4 색상 값

    // 테트리미노 효과 (발라트로 라이크 요소)
    public TetriminoEffect effect;
    public Tetrimino(TetriminoType tetriminoType, int color)
    {
        type = tetriminoType;
        position = new Vector2Int(TetrisBoard.WIDTH / 2, TetrisBoard.HEIGHT - 1);
        rotation = 0;
        shape = GetShapeForType(tetriminoType);
        this.color = color;
        effect = new TetriminoEffect(); // 기본 효과 없음
    }

    private Vector2Int[] GetShapeForType(TetriminoType type)
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
    public Vector2Int[] GetWorldPositions()
    {
        Vector2Int[] worldPositions = new Vector2Int[shape.Length];
        for (int i = 0; i < shape.Length; i++)
        {
            worldPositions[i] = position + RotatePoint(shape[i], rotation);
        }
        return worldPositions;
    }

    private Vector2Int RotatePoint(Vector2Int point, int rotation)
    {
        for (int i = 0; i < rotation; i++)
        {
            int temp = point.x;
            point.x = point.y;
            point.y = -temp;
        }
        return point;
    }
}

[System.Serializable]
public class TetriminoEffect
{
    public EffectType effectType;
    public float scoreMultiplier;
    public int bonusPoints;
    public bool isActive;

    public TetriminoEffect()
    {
        effectType = EffectType.None;
        scoreMultiplier = 1.0f;
        bonusPoints = 0;
        isActive = false;
    }
}

public enum EffectType
{
    None,
    ScoreMultiplier,
    BonusPoints,
    LineClearBonus,
    ComboBonus
}
