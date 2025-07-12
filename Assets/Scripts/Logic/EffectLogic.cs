using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EffectLogic
{
    private Game game;

    public EffectLogic(Game gameData)
    {
        this.game = gameData;
    }

    public void UpdateEffects(float deltaTime)
    {
        // 모든 활성 효과들 업데이트
        foreach (ActiveEffect effect in game.ActiveEffects)
        {
            effect.Update(deltaTime);
        }

        // 만료된 효과들 제거
        game.ActiveEffects.RemoveAll(effect => effect.IsExpired());
    }

    public void ApplyEffect(EffectType type, float duration, float value)
    {
        ActiveEffect newEffect = new ActiveEffect(type, duration, value);

        // 같은 타입의 효과가 이미 있는지 확인
        ActiveEffect existingEffect = game.ActiveEffects.FirstOrDefault(e => e.type == type);

        if (existingEffect != null)
        {
            // 기존 효과가 있으면 교체 또는 스택
            switch (type)
            {
                case EffectType.ScoreMultiplier:
                    // 배율 효과는 스택 (곱하기)
                    existingEffect.value *= value;
                    existingEffect.timeRemaining = Mathf.Max(existingEffect.timeRemaining, duration);
                    break;
                case EffectType.BonusPoints:
                    // 보너스 점수는 스택 (더하기)
                    existingEffect.value += value;
                    existingEffect.timeRemaining = Mathf.Max(existingEffect.timeRemaining, duration);
                    break;
                default:
                    // 기본적으로는 교체
                    existingEffect.value = value;
                    existingEffect.timeRemaining = duration;
                    break;
            }
        }
        else
        {
            // 새로운 효과 추가
            game.ActiveEffects.Add(newEffect);
        }

        Debug.Log($"효과 적용: {type}, 값: {value}, 지속시간: {duration}초");
    }

    public void ApplyTetriminoEffect(Tetrimino tetrimino, EffectType effectType, float value, float duration = 0)
    {
        if (tetrimino == null) return;

        // 테트리미노에 효과 적용
        tetrimino.effect.effectType = effectType;
        tetrimino.effect.isActive = true;

        switch (effectType)
        {
            case EffectType.ScoreMultiplier:
                tetrimino.effect.scoreMultiplier = value;
                break;
            case EffectType.BonusPoints:
                tetrimino.effect.bonusPoints = Mathf.RoundToInt(value);
                break;
            case EffectType.LineClearBonus:
                tetrimino.effect.bonusPoints = Mathf.RoundToInt(value);
                break;
            case EffectType.ComboBonus:
                tetrimino.effect.scoreMultiplier = value;
                break;
        }

        // 지속시간이 있는 경우 전역 효과로도 적용
        if (duration > 0)
        {
            ApplyEffect(effectType, duration, value);
        }

        Debug.Log($"테트리미노 효과 적용: {tetrimino.type}에 {effectType} 효과");
    }

    public void ClearAllEffects()
    {
        game.ActiveEffects.Clear();
        Debug.Log("모든 효과 제거");
    }

    public bool HasActiveEffect(EffectType type)
    {
        return game.ActiveEffects.Any(effect => effect.type == type);
    }

    public float GetEffectValue(EffectType type)
    {
        ActiveEffect effect = game.ActiveEffects.FirstOrDefault(e => e.type == type);
        return effect?.value ?? 0f;
    }

    public float GetEffectTimeRemaining(EffectType type)
    {
        ActiveEffect effect = game.ActiveEffects.FirstOrDefault(e => e.type == type);
        return effect?.timeRemaining ?? 0f;
    }

    // 상점에서 사용할 효과 미리보기
    public string GetEffectDescription(EffectType type, float value)
    {
        switch (type)
        {
            case EffectType.ScoreMultiplier:
                return $"점수 {value:F1}배 증가";
            case EffectType.BonusPoints:
                return $"보너스 점수 +{value:F0}";
            case EffectType.LineClearBonus:
                return $"라인당 보너스 +{value:F0}";
            case EffectType.ComboBonus:
                return $"콤보 보너스 {value:F1}배";
            default:
                return "알 수 없는 효과";
        }
    }
}
