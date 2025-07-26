using UnityEngine;
using UnityEngine.UI;

namespace Minomino
{
    public class ShadowView : MonoBehaviour
    {
        [Header("그림자 설정")]
        [SerializeField] private Vector2 shadowOffset = new Vector2(0, -10);
        [SerializeField] private float shadowScale = 1.1f; // 10% 더 크게
        [SerializeField] private Color shadowColor = new Color(0, 0, 0, 0.5f);

        // 참조 변수
        private RectTransform shadowRect;
        private RectTransform targetRect;
        private Image shadowImage;
        private Image targetImage;
        private GameObject shadowObject;

        private void Start()
        {
            // 자기 자신의 컴포넌트 가져오기
            targetRect = GetComponent<RectTransform>();
            targetImage = GetComponent<Image>();

            if (targetRect == null || targetImage == null)
            {
                Debug.LogError("ShadowView: This object must have RectTransform and Image components!");
                return;
            }

            CreateShadow();
        }

        private void CreateShadow()
        {
            // 그림자 오브젝트 생성
            shadowObject = new GameObject("Shadow");
            shadowObject.transform.SetParent(transform.parent, false);

            // RectTransform 설정
            shadowRect = shadowObject.AddComponent<RectTransform>();

            // Image 컴포넌트 설정
            shadowImage = shadowObject.AddComponent<Image>();

            // 초기 설정 복사
            CopyInitialSettings();

            // 그림자를 원본 뒤로 보내기
            shadowObject.transform.SetSiblingIndex(transform.GetSiblingIndex());
            transform.SetSiblingIndex(shadowObject.transform.GetSiblingIndex() + 1);
        }

        private void CopyInitialSettings()
        {
            // 앵커 및 피벗 설정 복사
            shadowRect.anchorMin = targetRect.anchorMin;
            shadowRect.anchorMax = targetRect.anchorMax;
            shadowRect.pivot = targetRect.pivot;

            // 크기 설정 (10% 더 크게)
            shadowRect.sizeDelta = new Vector2(
                targetRect.sizeDelta.x * shadowScale, 
                targetRect.sizeDelta.y * shadowScale
            );

            // 초기 위치 설정
            Vector2 targetPosition = targetRect.anchoredPosition;
            shadowRect.anchoredPosition = targetPosition + shadowOffset;

            // 이미지 설정
            if (targetImage != null)
            {
                shadowImage.sprite = targetImage.sprite;
                shadowImage.color = shadowColor;
            }
        }

        private void Update()
        {
            if (targetRect == null || shadowRect == null) return;

            // 위치 따라가기 (Z축 고정)
            UpdatePosition();

            // 회전 따라가기 (Z축 고정)
            UpdateRotation();

            // 스프라이트 동기화
            UpdateSprite();
        }

        private void UpdatePosition()
        {
            Vector2 targetPosition = targetRect.anchoredPosition;
            Vector2 shadowPosition = targetPosition + shadowOffset;
            
            // 그림자 위치 업데이트
            shadowRect.anchoredPosition = shadowPosition;
        }

        private void UpdateRotation()
        {
            Vector3 targetRotation = targetRect.localEulerAngles;
            
            // 모든 축의 회전값을 따라가기
            shadowRect.localEulerAngles = targetRotation;
        }

        private void UpdateSprite()
        {
            if (shadowImage != null && targetImage != null)
            {
                // 스프라이트가 변경되었을 때만 업데이트
                if (shadowImage.sprite != targetImage.sprite)
                {
                    shadowImage.sprite = targetImage.sprite;
                }
            }
        }

        // 공개 메서드들
        public void SetShadowOffset(Vector2 offset)
        {
            shadowOffset = offset;
        }

        public void SetShadowColor(Color color)
        {
            shadowColor = color;
            if (shadowImage != null)
            {
                shadowImage.color = shadowColor;
            }
        }

        private void OnDisable()
        {
            // 오브젝트가 비활성화될 때 그림자도 숨기기
            if (shadowObject != null)
            {
                shadowObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            // 오브젝트가 활성화될 때 그림자도 다시 보이기
            if (shadowObject != null)
            {
                shadowObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            // 그림자 오브젝트 정리 (안전한 방법)
            if (shadowObject != null)
            {
                // 이미 파괴 중인지 확인
                if (shadowObject.gameObject != null)
                {
                    Destroy(shadowObject);
                }
                shadowObject = null;
            }
        }
    }
}
