using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minomino
{
    public class MinoView : MonoBehaviour
    {
        [Header("미노 이미지")]
        [SerializeField] private Image Image_Mino;
        [SerializeField] private Image Image_Wall;
        [SerializeField] private Image Image_Background;

        [Header("일반 미뉴들")]
        [SerializeField] private Image Image_Big_Minue;
        [SerializeField] private Image Image_Midum_Minue;
        [SerializeField] private Image Image_Small_Minue;

        [Header("특수 미뉴")]
        [SerializeField] private Image Image_Special_Minue;
        [SerializeField] private GameObject Object_Special_Type;
        [SerializeField] private TMP_Text Text_Special_Type;
        [SerializeField] private GameObject Object_Special_Size;
        [SerializeField] private TMP_Text Text_Special_Size;

        [Header("설명")]
        [SerializeField] private GameObject Object_Description;
        [SerializeField] private TMP_Text Text_Description;

        private MinoComponent _minoComponent;

        public void Refresh(MinoComponent minoComponent = null)
        {
            if (minoComponent == null)
            {
                _minoComponent = null;
                Image_Big_Minue.gameObject.SetActive(false);
                Image_Midum_Minue.gameObject.SetActive(false);
                Image_Small_Minue.gameObject.SetActive(false);
                Image_Special_Minue.gameObject.SetActive(false);
                Object_Special_Type.SetActive(false);
                Object_Special_Size.SetActive(false);
                Object_Description.SetActive(false);

                Image_Mino.color = Color.clear;
                return;
            }

            // 새로운 엔티티가 설정되면 업데이트
            _minoComponent = minoComponent;
            Image_Mino.color = Color.white;
            Image_Background.color = SpriteSettings.Instance.WhiteCreamColor;

            SetSpriteByMinue(_minoComponent);
        }

        private void SetSpriteByMinue(MinoComponent minoComponent)
        {
            // 미노 배경색
            switch (minoComponent.State)
            {
                case MinoState.None:
                    Debug.LogWarning($"None state MinoComponent");
                    break;
                case MinoState.Empty:
                    Image_Background.color = SpriteSettings.Instance.DarkColor;
                    break;
                case MinoState.Living:
                    Image_Background.color = SpriteSettings.Instance.WhiteCreamColor;
                    break;
                default:
                    Debug.LogWarning($"Unknown MinoState: {minoComponent.State}");
                    break;
            }

            // 미노 벽색
            switch (minoComponent.MinoColor)
            {
                case MinoColor.None:
                    Debug.LogWarning($"None color MinoComponent");
                    break;
                case MinoColor.Bright:
                    Image_Wall.color = SpriteSettings.Instance.ButterColor;
                    break;
                case MinoColor.Beige:
                    Image_Wall.color = SpriteSettings.Instance.BeigeColor;
                    break;
                case MinoColor.Dark:
                    Image_Wall.color = SpriteSettings.Instance.DarkColor;
                    break;
                default:
                    Debug.LogWarning($"Unknown MinoColor: {minoComponent.MinoColor}");
                    break;
            }

            // 미뉴 지정
            switch (minoComponent.MinueType)
            {
                case MinueType.None:
                    Debug.LogWarning($"None type MinoComponent");
                    break;
                case MinueType.Small:
                    SetActiveImages(true, false, false);
                    Text_Description.text = "오늘 저녁 뭐 먹지~";
                    break;
                case MinueType.Medium:
                    SetActiveImages(false, true, false);
                    Text_Description.text = "반가워요!";
                    break;
                case MinueType.Big:
                    SetActiveImages(false, false, true);
                    Text_Description.text = "좋은 밤이야";
                    break;
                // 특수 미뉴들
                default:
                    // 특수 미뉴 타입 파싱
                    SetActiveImages(false, false, false);
                    int spriteIdx = -1;
                    string typeText = string.Empty;
                    string sizeText = string.Empty;
                    bool showType = false;
                    bool showSize = false;

                    // 타입/배수 구분
                    string minueName = minoComponent.MinueType.ToString();
                    if (minueName.StartsWith("Val"))
                    {
                        showType = true;
                        typeText = "벨";
                    }
                    else if (minueName.StartsWith("Mul"))
                    {
                        showType = true;
                        typeText = "배수";
                    }

                    // 특수 미뉴 종류
                    if (minueName.Contains("Angel")) spriteIdx = 0;
                    else if (minueName.Contains("Devil")) spriteIdx = 1;
                    else if (minueName.Contains("Sun")) spriteIdx = 2;
                    else if (minueName.Contains("Moon")) spriteIdx = 3;
                    else if (minueName.Contains("Star")) spriteIdx = 4;

                    // 사이즈
                    if (minueName.Contains("Small")) { showSize = true; sizeText = "작은"; }
                    else if (minueName.Contains("Midium")) { showSize = true; sizeText = "중간"; }
                    else if (minueName.Contains("Big")) { showSize = true; sizeText = "큰"; }

                    if (spriteIdx >= 0)
                        Image_Special_Minue.sprite = SpriteSettings.Instance.Sprites_Special_Minue[spriteIdx];

                    Object_Special_Type.SetActive(showType);
                    Object_Special_Size.SetActive(showSize);
                    Text_Special_Type.text = showType ? typeText : string.Empty;
                    Text_Special_Size.text = showSize ? sizeText : string.Empty;

                    // 설명 텍스트 (특수 미뉴별 효과)
                    switch (minoComponent.MinueType)
                    {
                        case MinueType.ValAngelMineu:
                            Text_Description.text = $"같은 줄에 미뉴의 수 만큼 벨 + {GlobalSettings.Instance.AngelVal}";
                            break;
                        case MinueType.MulAngelMineu:
                            Text_Description.text = $"같은 줄에 미뉴의 수 만큼 배수 + {GlobalSettings.Instance.AngelMul}";
                            break;
                        case MinueType.ValDevilMineu:
                            Text_Description.text = $"같은 줄에 3종의 미뉴가 모두 있다면 벨 + {GlobalSettings.Instance.DevilVal}";
                            break;
                        case MinueType.MulDevilMineu:
                            Text_Description.text = $"같은 줄에 3종의 미뉴가 모두 있다면 배수 + {GlobalSettings.Instance.DevilMul}";
                            break;
                        case MinueType.ValSmallStarMineu:
                            Text_Description.text = $"같은 줄에 작은 미뉴의 수 만큼 벨 + {GlobalSettings.Instance.StarVal}";
                            break;
                        case MinueType.ValMidiumStarMineu:
                            Text_Description.text = $"같은 줄에 중간 미뉴의 수 만큼 벨 + {GlobalSettings.Instance.StarVal}";
                            break;
                        case MinueType.ValBigStarMineu:
                            Text_Description.text = $"같은 줄에 큰 미뉴의 수 만큼 벨 + {GlobalSettings.Instance.StarVal}";
                            break;
                        case MinueType.MulSmallStarMineu:
                            Text_Description.text = $"같은 줄에 작은 미뉴의 수 만큼 배수 + {GlobalSettings.Instance.StarMul}";
                            break;
                        case MinueType.MulMidiumStarMineu:
                            Text_Description.text = $"같은 줄에 중간 미뉴의 수 만큼 배수 + {GlobalSettings.Instance.StarMul}";
                            break;
                        case MinueType.MulBigStarMineu:
                            Text_Description.text = $"같은 줄에 큰 미뉴의 수 만큼 배수 + {GlobalSettings.Instance.StarMul}";
                            break;
                        case MinueType.ValSmallMoonMineu:
                            Text_Description.text = $"같은 줄에 작은 미뉴의 수가 4명 이상일 때 벨 + {GlobalSettings.Instance.MoonVal}";
                            break;
                        case MinueType.ValMidiumMoonMineu:
                            Text_Description.text = $"같은 줄에 중간 미뉴의 수가 4명 이상일 때 벨 + {GlobalSettings.Instance.MoonVal}";
                            break;
                        case MinueType.ValBigMoonMineu:
                            Text_Description.text = $"같은 줄에 큰 미뉴의 수가 4명 이상일 때 벨 + {GlobalSettings.Instance.MoonVal}";
                            break;
                        case MinueType.MulSmallMoonMineu:
                            Text_Description.text = $"같은 줄에 작은 미뉴의 수가 4명 이상일 때 배수 + {GlobalSettings.Instance.MoonMul}";
                            break;
                        case MinueType.MulMidiumMoonMineu:
                            Text_Description.text = $"같은 줄에 중간 미뉴의 수가 4명 이상일 때 배수 + {GlobalSettings.Instance.MoonMul}";
                            break;
                        case MinueType.MulBigMoonMineu:
                            Text_Description.text = $"같은 줄에 큰 미뉴의 수가 4명 이상일 때 배수 + {GlobalSettings.Instance.MoonMul}";
                            break;
                        case MinueType.ValSmallSunMineu:
                            Text_Description.text = $"같은 줄에 작은 미뉴만 있다면 벨 + {GlobalSettings.Instance.SunVal}";
                            break;
                        case MinueType.ValMidiumSunMineu:
                            Text_Description.text = $"같은 줄에 중간 미뉴만 있다면 벨 + {GlobalSettings.Instance.SunVal}";
                            break;
                        case MinueType.ValBigSunMineu:
                            Text_Description.text = $"같은 줄에 큰 미뉴만 있다면 벨 + {GlobalSettings.Instance.SunVal}";
                            break;
                        case MinueType.MulSmallSunMineu:
                            Text_Description.text = $"같은 줄에 작은 미뉴만 있다면 배수 + {GlobalSettings.Instance.SunMul}";
                            break;
                        case MinueType.MulMidiumSunMineu:
                            Text_Description.text = $"같은 줄에 중간 미뉴만 있다면 배수 + {GlobalSettings.Instance.SunMul}";
                            break;
                        case MinueType.MulBigSunMineu:
                            Text_Description.text = $"같은 줄에 큰 미뉴만 있다면 배수 + {GlobalSettings.Instance.SunMul}";
                            break;
                        default:
                            Text_Description.text = string.Empty;
                            break;
                    }
                    break;
            }
        }

        private void SetActiveImages(bool isSmall, bool isMedium, bool isBig)
        {

            // 특수 미뉴는 별도로 처리
            bool isSpecial = !isSmall && !isMedium && !isBig;

            if (isSpecial)
            {
                Image_Small_Minue.gameObject.SetActive(false);
                Image_Midum_Minue.gameObject.SetActive(false);
                Image_Big_Minue.gameObject.SetActive(false);
                Image_Special_Minue.gameObject.SetActive(true);
                Object_Special_Type.SetActive(true);
                Object_Special_Size.SetActive(true);
            }
            else
            {
                Image_Small_Minue.gameObject.SetActive(isSmall);
                Image_Midum_Minue.gameObject.SetActive(isMedium);
                Image_Big_Minue.gameObject.SetActive(isBig);
                Image_Special_Minue.gameObject.SetActive(false);
                Object_Special_Type.SetActive(false);
                Object_Special_Size.SetActive(false);
            }
        }

        public void OnPointerEnter()
        {
            Object_Description.SetActive(true);
        }

        public void OnPointerExit()
        {
            Object_Description.SetActive(false);
        }
    }
}