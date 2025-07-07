using UnityEngine;
using System.Collections.Generic;

public class ShopLogic
{
    private Game game;
    private ScoreLogic scoreLogic;
    private EffectLogic effectLogic;

    public ShopLogic(Game gameData, ScoreLogic scoreLogic, EffectLogic effectLogic)
    {
        this.game = gameData;
        this.scoreLogic = scoreLogic;
        this.effectLogic = effectLogic;
    }

    public List<ShopItem> GenerateShopItems()
    {
        List<ShopItem> items = new List<ShopItem>();

        // 기본 상점 아이템들 생성
        items.Add(new ShopItem
        {
            id = "score_multiplier",
            name = "점수 배율 증가",
            description = "다음 테트리미노의 점수를 2배로 증가",
            cost = 50,
            effectType = EffectType.ScoreMultiplier,
            effectValue = 2.0f,
            itemType = ShopItemType.TetriminoEffect
        });

        items.Add(new ShopItem
        {
            id = "bonus_points",
            name = "보너스 점수",
            description = "다음 테트리미노로 라인 클리어 시 +200점",
            cost = 30,
            effectType = EffectType.BonusPoints,
            effectValue = 200f,
            itemType = ShopItemType.TetriminoEffect
        });

        items.Add(new ShopItem
        {
            id = "line_clear_bonus",
            name = "라인 클리어 보너스",
            description = "다음 테트리미노로 라인당 +100점 추가",
            cost = 40,
            effectType = EffectType.LineClearBonus,
            effectValue = 100f,
            itemType = ShopItemType.TetriminoEffect
        });

        items.Add(new ShopItem
        {
            id = "global_multiplier",
            name = "전역 점수 배율",
            description = "30초간 모든 점수 1.5배 증가",
            cost = 80,
            effectType = EffectType.ScoreMultiplier,
            effectValue = 1.5f,
            duration = 30f,
            itemType = ShopItemType.GlobalEffect
        });

        items.Add(new ShopItem
        {
            id = "target_reduction",
            name = "목표 점수 감소",
            description = "현재 목표 점수를 20% 감소",
            cost = 100,
            itemType = ShopItemType.Utility
        });

        return items;
    }

    public bool PurchaseItem(ShopItem item)
    {
        if (!scoreLogic.CanAfford(item.cost))
        {
            Debug.Log("구매 실패: 화폐 부족");
            return false;
        }

        // 화폐 차감
        if (!scoreLogic.SpendCurrency(item.cost))
        {
            Debug.Log("구매 실패: 화폐 차감 실패");
            return false;
        }

        // 아이템 효과 적용
        ApplyItemEffect(item);

        Debug.Log($"아이템 구매 성공: {item.name}");
        return true;
    }

    private void ApplyItemEffect(ShopItem item)
    {
        switch (item.itemType)
        {
            case ShopItemType.TetriminoEffect:
                ApplyTetriminoEffect(item);
                break;
            case ShopItemType.GlobalEffect:
                ApplyGlobalEffect(item);
                break;
            case ShopItemType.Utility:
                ApplyUtilityEffect(item);
                break;
        }
    }

    private void ApplyTetriminoEffect(ShopItem item)
    {
        // 다음 테트리미노에 효과 적용
        if (game.NextTetrimino != null)
        {
            effectLogic.ApplyTetriminoEffect(game.NextTetrimino, item.effectType, item.effectValue);
        }
        else
        {
            Debug.LogWarning("다음 테트리미노가 없어서 효과를 적용할 수 없습니다.");
        }
    }

    private void ApplyGlobalEffect(ShopItem item)
    {
        // 전역 효과 적용
        effectLogic.ApplyEffect(item.effectType, item.duration, item.effectValue);
    }

    private void ApplyUtilityEffect(ShopItem item)
    {
        switch (item.id)
        {
            case "target_reduction":
                game.TargetScore = Mathf.RoundToInt(game.TargetScore * 0.8f);
                Debug.Log($"목표 점수가 {game.TargetScore}로 감소했습니다.");
                break;
        }
    }
}

[System.Serializable]
public class ShopItem
{
    public string id;
    public string name;
    public string description;
    public int cost;
    public EffectType effectType;
    public float effectValue;
    public float duration; // 전역 효과용
    public ShopItemType itemType;
}

public enum ShopItemType
{
    TetriminoEffect,  // 다음 테트리미노에 적용되는 효과
    GlobalEffect,     // 일정 시간 지속되는 전역 효과
    Utility          // 기타 유틸리티 효과
}
