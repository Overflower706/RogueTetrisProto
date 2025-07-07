using UnityEngine;

[System.Serializable]
public class ActiveEffect
{
    public EffectType type;
    public float duration;
    public float timeRemaining;
    public float value; // 효과의 강도 (배율, 보너스 점수 등)

    public ActiveEffect(EffectType effectType, float effectDuration, float effectValue)
    {
        type = effectType;
        duration = effectDuration;
        timeRemaining = effectDuration;
        value = effectValue;
    }

    public bool IsExpired()
    {
        return timeRemaining <= 0;
    }

    public void Update(float deltaTime)
    {
        timeRemaining -= deltaTime;
        if (timeRemaining < 0) timeRemaining = 0;
    }
}
