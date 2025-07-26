using System;
using System.Collections.Generic;
using UnityEngine;

namespace Minomino
{
    public class GlobalSettings : MonoSingleton<GlobalSettings>
    {
        [Header("보드 초기값")]
        public int SafeWidth = 10;
        public int SafeHeight = 20;
        public int MinoWidth = 120;
        public int MinoHeight = 90;
        [Header("테트로미노가 등장할 버퍼입니다. 최소 2칸은 보장되어야합니다.")]
        public int BoardBufferHeight = 2;
        [Header("이건 Safe 높이 + 버퍼 최종값입니다. 건들지 마십쇼.")]
        public int BoardHeight => SafeHeight + BoardBufferHeight;

        [Header("홀드 초기값")]
        public int HoldSize = 1;

        [Header("가방 초기값")]
        public int BagSize = 2;

        [Header("목표 점수")]
        public int RoundBonus = 10;
        public int StageBonus = 100;

        [Header("보상")]
        public int CurrencyBase = 100;
        public int CurrencyBonusPerDeck = 1;

        [Header("Player Component를 확인하세요")]
        public int BaseBakeCount = 3;
        public int BaseTrashCount = 3;

        [Header("점수")]
        public int CookieScore = 10;
        public int BreadScore = 50;
        public int IcingCookieScore = 50;
        public int ChocoSoraBreadScore = 100;

        [Header("보상 이미지")]
        public Sprite Sprite_Reward_Alert;
        public Sprite Sprite_Reward_Received;

        [Header("특별 미뉴 효과")]
        public int AngelVal = 5;
        public float AngelMul = 0.1f;
        public int DevilVal = 30;
        public float DevilMul = 1f;
        public int SunVal = 100;
        public float SunMul = 2;
        public int MoonVal = 50;
        public float MoonMul = 1f;
        public int StarVal = 10;
        public float StarMul = 0.2f;
    }
}