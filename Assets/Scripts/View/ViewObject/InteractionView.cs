using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Minomino
{
    public class InteractionView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("애니메이션 설정")]
        [SerializeField] private float shakeIntensity = 3f; // 호버 시 흔들림 강도
        [SerializeField] private float shakeDuration = 0.1f; // 호버 시 흔들림 지속시간
        [SerializeField] private float returnDuration = 0.3f; // 원래 위치로 복귀 시간
        [SerializeField] private float rotationIntensity = 5f; // 호버 시 회전 강도
        
        [Header("드래그 회전 설정")]
        [SerializeField] private float maxRotationAngle = 30f; // 최대 기울기 각도
        [SerializeField] private float minSpeedForRotation = 10f; // 회전을 시작하는 최소 속도
        [SerializeField] private float rotationSmoothTime = 0.1f; // 회전 애니메이션 속도
        [SerializeField] private float followDelay = 0.1f; // 마우스 따라가기 딜레이 (초)
        
        [Header("클릭 효과 설정")]
        [SerializeField] private float clickScaleMultiplier = 1.1f; // 클릭 시 스케일 배율 (10% 증가)
        [SerializeField] private float clickScaleDuration = 0.3f; // 클릭 스케일 애니메이션 지속시간

        [Header("기능 제어")]
        [SerializeField] private bool enableHoverAnimation = true;
        [SerializeField] private bool enableDragMovement = true;

        // 초기값 저장 변수
        private Vector3 initialLocalPosition; // 최초 생성 시 위치
        private Vector3 initialLocalRotation; // 최초 생성 시 회전값
        private Vector3 initialLocalScale; // 최초 생성 시 스케일
        private int originalSiblingIndex; // 원래 sibling index 저장
        
        // 상태 관리 변수
        private bool isDragging = false;
        private bool hasPlayedHoverAnimation = false;
        private Camera uiCamera;
        
        // DOTween 애니메이션 변수
        private Tween shakeTween; // 흔들림 애니메이션
        private Tween rotationTween; // 호버 회전 애니메이션
        private Tween positionFollowTween; // 위치 따라가기 트윈
        private Tween rotationFollowTween; // 회전 따라가기 트윈
        private Tween scaleTween; // 스케일 애니메이션 트윈
        
        // 드래그 관련 변수
        private Vector2 lastMousePosition;
        private Vector2 targetPosition; // 목표 위치 (딜레이 효과용)
        private Vector2 mouseVelocity;
        private float targetRotationZ;

        private void Start()
        {
            // 초기값 저장 (애니메이션 복귀 시 사용)
            initialLocalPosition = transform.localPosition;
            initialLocalRotation = transform.localEulerAngles;
            initialLocalScale = transform.localScale;
            originalSiblingIndex = transform.GetSiblingIndex();
            
            // UI 카메라 설정
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                uiCamera = canvas.worldCamera;
                if (uiCamera == null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    uiCamera = Camera.main;
                }
            }
        }

        private void OnDestroy()
        {
            // DOTween 정리
            shakeTween?.Kill();
            rotationTween?.Kill();
            positionFollowTween?.Kill();
            rotationFollowTween?.Kill();
            scaleTween?.Kill();
            transform.DOKill();
        }

        public void SetTransparency(bool isTransparent)
        {
            Image image = GetComponent<Image>();
            if (image == null) return;

            Color color = image.color;
            color.a = isTransparent ? 0f : 1f; // 투명도 설정
            image.color = color;
        }

        public void Refresh(bool isReceived)
        {
            Image image = GetComponent<Image>();
            if (image == null) return;

            if (isReceived)
            {
                image.sprite = GlobalSettings.Instance.Sprite_Reward_Received;
            }
            else
            {
                image.sprite = GlobalSettings.Instance.Sprite_Reward_Alert;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // 드래그 중이면 호버 애니메이션 무시
            if (isDragging) return;
            
            // 호버 애니메이션이 비활성화되어 있으면 return
            if (!enableHoverAnimation) return;
            
            // 이미 재생했으면 return (1회 제한)
            if (hasPlayedHoverAnimation) return;

            // 흔들림 애니메이션 시작 (1회만)
            StartShakeAnimation();
            StartRotationAnimation();
            
            // 1회 재생 플래그 설정
            hasPlayedHoverAnimation = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // 드래그 중이면 호버 종료 애니메이션 무시
            if (isDragging) return;
            
            // 1회 재생 플래그 리셋 (다음 호버 시 다시 재생 가능)
            hasPlayedHoverAnimation = false;
            
            if (!isDragging)
            {
                // 흔들림과 회전 중단하고 원래 위치로 복귀
                StopShakeAnimation();
                StopRotationAnimation();
                ReturnToOriginalPosition();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // 드래그 기능이 비활성화되어 있으면 return
            if (!enableDragMovement) return;
            
            isDragging = true;
            // 흔들림과 회전 중단
            StopShakeAnimation();
            StopRotationAnimation();
            
            // 마우스 위치 초기화
            lastMousePosition = eventData.position;
            mouseVelocity = Vector2.zero;
            targetRotationZ = initialLocalRotation.z;
            
            // 드래그 시작 시 최상단으로 이동
            transform.SetAsLastSibling();
            
            // 드래그 시작 시 기본 기울기
            StartDragRotation();
            
            // 클릭 시 스케일 애니메이션
            StartClickScaleAnimation();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // 드래그 기능이 비활성화되어 있으면 return
            if (!enableDragMovement) return;
            
            isDragging = false;
            
            // DOTween을 사용한 회전 복귀
            rotationFollowTween?.Kill();
            rotationFollowTween = transform.DOLocalRotate(initialLocalRotation, rotationSmoothTime)
                .SetEase(Ease.OutBack);
            
            // 위치 이동 중단
            positionFollowTween?.Kill();
            
            // 원래 sibling index로 복귀
            transform.SetSiblingIndex(originalSiblingIndex);
            
            // 원래 위치로 복귀
            ReturnToOriginalPosition();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            // 새로운 마우스 속도 계산
            mouseVelocity = (eventData.position - lastMousePosition) / Time.deltaTime;
            
            // 회전 계산
            float horizontalVelocity = mouseVelocity.x;
            if (Mathf.Abs(horizontalVelocity) > minSpeedForRotation)
            {
                // 속도에 따른 회전 목표값 계산 (-30도 ~ +30도)
                targetRotationZ = Mathf.Clamp(-horizontalVelocity * 0.1f, -maxRotationAngle, maxRotationAngle);
            }
            else
            {
                targetRotationZ = initialLocalRotation.z;
            }
            
            // DOTween을 사용한 부드러운 회전
            rotationFollowTween?.Kill();
            Vector3 newRotation = initialLocalRotation;
            newRotation.z = targetRotationZ;
            rotationFollowTween = transform.DOLocalRotate(newRotation, rotationSmoothTime)
                .SetEase(Ease.OutQuad);

            // 위치 업데이트 - DOTween으로 부드러운 이동
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform.parent as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out targetPosition);
                
            positionFollowTween?.Kill();
            Vector3 targetPos = new Vector3(targetPosition.x, targetPosition.y, initialLocalPosition.z);
            positionFollowTween = transform.DOLocalMove(targetPos, followDelay)
                .SetEase(Ease.OutQuad);

            lastMousePosition = eventData.position;
        }

        private void StartShakeAnimation()
        {
            StopShakeAnimation();
            
            // DOTween을 사용한 흔들림 애니메이션 (1회만)
            shakeTween = transform.DOShakePosition(shakeDuration, shakeIntensity, 10, 90, false, true);
        }

        private void StopShakeAnimation()
        {
            shakeTween?.Kill();
            shakeTween = null;
        }

        private void StartRotationAnimation()
        {
            StopRotationAnimation();
            
            // DOTween을 사용한 회전 애니메이션 (1회만, 좌우로 살짝 회전) - 최초 생성 시 회전값 기준
            float targetRotation = initialLocalRotation.z + rotationIntensity;
            rotationTween = transform.DORotate(new Vector3(initialLocalRotation.x, initialLocalRotation.y, targetRotation), shakeDuration)
                .SetLoops(2, LoopType.Yoyo); // 2회 반복으로 원래 위치로 돌아오기
        }

        private void StopRotationAnimation()
        {
            rotationTween?.Kill();
            rotationTween = null;
        }

        private void ReturnToOriginalPosition()
        {
            // DOTween을 사용한 부드러운 최초 생성 시 위치와 회전, 스케일로 복귀
            transform.DOLocalMove(initialLocalPosition, returnDuration)
                .SetEase(Ease.OutBack);
            transform.DOLocalRotate(initialLocalRotation, returnDuration)
                .SetEase(Ease.OutBack);
            
            // 스케일도 최초 생성 시 크기로 복귀
            scaleTween?.Kill();
            scaleTween = transform.DOScale(initialLocalScale, returnDuration)
                .SetEase(Ease.OutBack);
        }

        private void StartDragRotation()
        {
            // 드래그 시작 시에는 기본적으로 똑바른 상태로 시작
            StopDragRotation();
        }

        private void StopDragRotation()
        {
            rotationFollowTween?.Kill();
            rotationFollowTween = null;
        }

        private void StartClickScaleAnimation()
        {
            scaleTween?.Kill();
            
            // 순간적으로 더 크게 펀치 효과 후 10% 크기로 설정 (최초 생성 시 스케일 기준)
            Vector3 targetScale = initialLocalScale * clickScaleMultiplier;
            scaleTween = transform.DOPunchScale(Vector3.one * 0.2f, clickScaleDuration * 0.3f, 1, 0f)
                .OnComplete(() => {
                    // 펀치 효과 후 10% 크기로 유지
                    scaleTween = transform.DOScale(targetScale, clickScaleDuration * 0.2f)
                        .SetEase(Ease.OutBack);
                });
        }
    }
}