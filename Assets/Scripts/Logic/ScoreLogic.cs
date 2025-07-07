using UnityEngine;
using System.Collections.Generic;

public class ScoreLogic
{
    private Game game;

    // 기본 점수 설정
    private const int BASE_LINE_SCORE = 100;
    private const int SINGLE_LINE = 1;
    private const int DOUBLE_LINE = 3;
    private const int TRIPLE_LINE = 5;
    private const int TETRIS_LINE = 8;

    public ScoreLogic(Game gameData)
    {
        game = gameData;
    }

    public void ProcessLineClears(int linesCleared, Tetrimino tetrimino)
    {
        if (linesCleared <= 0) return;

        // 기본 점수 계산
        int baseScore = CalculateBaseScore(linesCleared);

        // 테트리미노 효과 적용 (발라트로 라이크 요소)
        float totalMultiplier = CalculateTotalMultiplier(tetrimino);
        int bonusPoints = CalculateBonusPoints(tetrimino, linesCleared);

        // 활성 효과들 적용
        totalMultiplier *= CalculateActiveEffectsMultiplier();
        bonusPoints += CalculateActiveEffectsBonus();

        // 최종 점수 계산
        int finalScore = Mathf.RoundToInt(baseScore * totalMultiplier) + bonusPoints;

        // 점수 적용
        game.CurrentScore += finalScore;

        // 화폐 획득 (점수의 10%)
        game.Currency += Mathf.RoundToInt(finalScore * 0.1f);

        Debug.Log($"라인 클리어! 기본점수: {baseScore}, 배율: {totalMultiplier:F2}, 보너스: {bonusPoints}, 최종: {finalScore}");
    }

    private int CalculateBaseScore(int linesCleared)
    {
        switch (linesCleared)
        {
            case 1: return BASE_LINE_SCORE * SINGLE_LINE;
            case 2: return BASE_LINE_SCORE * DOUBLE_LINE;
            case 3: return BASE_LINE_SCORE * TRIPLE_LINE;
            case 4: return BASE_LINE_SCORE * TETRIS_LINE;
            default: return BASE_LINE_SCORE * linesCleared;
        }
    }

    private float CalculateTotalMultiplier(Tetrimino tetrimino)
    {
        float multiplier = 1.0f;

        if (tetrimino != null && tetrimino.effect != null && tetrimino.effect.isActive)
        {
            switch (tetrimino.effect.effectType)
            {
                case EffectType.ScoreMultiplier:
                    multiplier *= tetrimino.effect.scoreMultiplier;
                    break;
                case EffectType.ComboBonus:
                    // 콤보 시스템은 추후 구현
                    multiplier *= tetrimino.effect.scoreMultiplier;
                    break;
            }
        }

        return multiplier;
    }

    private int CalculateBonusPoints(Tetrimino tetrimino, int linesCleared)
    {
        int bonus = 0;

        if (tetrimino != null && tetrimino.effect != null && tetrimino.effect.isActive)
        {
            switch (tetrimino.effect.effectType)
            {
                case EffectType.BonusPoints:
                    bonus += tetrimino.effect.bonusPoints;
                    break;
                case EffectType.LineClearBonus:
                    bonus += tetrimino.effect.bonusPoints * linesCleared;
                    break;
            }
        }

        return bonus;
    }

    private float CalculateActiveEffectsMultiplier()
    {
        float multiplier = 1.0f;

        foreach (ActiveEffect effect in game.ActiveEffects)
        {
            if (effect.type == EffectType.ScoreMultiplier)
            {
                multiplier *= effect.value;
            }
        }

        return multiplier;
    }

    private int CalculateActiveEffectsBonus()
    {
        int bonus = 0;

        foreach (ActiveEffect effect in game.ActiveEffects)
        {
            if (effect.type == EffectType.BonusPoints)
            {
                bonus += Mathf.RoundToInt(effect.value);
            }
        }

        return bonus;
    }

    // 상점에서 사용할 메서드
    public bool CanAfford(int cost)
    {
        return game.Currency >= cost;
    }

    public bool SpendCurrency(int amount)
    {
        if (CanAfford(amount))
        {
            game.Currency -= amount;
            return true;
        }
        return false;
    }
}
