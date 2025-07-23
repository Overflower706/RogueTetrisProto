using DG.Tweening;
using OVFL.ECS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minomino
{
    public class GameExCanvasManager : MonoBehaviour, ICanvasManager, ITickSystem
    {
        public Context Context { get; set; }

        [Header("담당 캔버스")]
        [SerializeField] private Canvas Canvas_GameEx;

        [Header("관리 패널")]
        [SerializeField] private GameObject Panel_Game;
        [SerializeField] private GameObject Panel_Ground;
        [SerializeField] private ApartmentPanelManager Panel_Apartment;

        [Header("결과 패널")]
        [SerializeField] private GameObject Panel_Result;
        [SerializeField] private GameObject Panel_Backblocker;
        [SerializeField] private TMP_Text Text_Result;
        [SerializeField] private Button Button_Hide;

        public IMiniSceneManager SceneManager { get; private set; }

        public void Init(IMiniSceneManager sceneManager)
        {
            SceneManager = sceneManager;

            Panel_Ground.SetActive(false);
            Panel_Apartment.gameObject.SetActive(false);
            Panel_Result.SetActive(false);
            Panel_Backblocker.SetActive(false);
            Button_Hide.interactable = false;

            Button_Hide.onClick.AddListener(() =>
            {
                Button_Hide.interactable = false;
                PanelSceneManager.Instance.LoadTitleScene();
                Debug.Log("Hide 버튼 클릭 - 타이틀 씬으로 이동");
            });
        }

        public Tween Show()
        {
            // 패널들을 활성화
            Panel_Ground.gameObject.SetActive(true);
            Panel_Apartment.gameObject.SetActive(true);

            // 패널들의 RectTransform 가져오기
            RectTransform groundRect = Panel_Ground.GetComponent<RectTransform>();
            RectTransform apartmentRect = Panel_Apartment.GetComponent<RectTransform>();

            // 원래 위치 저장
            Vector2 groundOriginalPos = groundRect.anchoredPosition;
            Vector2 apartmentOriginalPos = apartmentRect.anchoredPosition;

            // 화면 아래 위치로 초기화 (화면 높이만큼 아래로)
            float screenHeight = Screen.height;
            groundRect.anchoredPosition = new Vector2(groundOriginalPos.x, groundOriginalPos.y - screenHeight);
            apartmentRect.anchoredPosition = new Vector2(apartmentOriginalPos.x, apartmentOriginalPos.y - screenHeight);

            // DOTween Sequence 생성
            Sequence showSequence = DOTween.Sequence();

            // Panel_Ground를 먼저 올리기
            showSequence.Append(
                DOTween.To(() => groundRect.anchoredPosition,
                          x => groundRect.anchoredPosition = x,
                          groundOriginalPos, 0.8f)
                    .SetEase(Ease.OutQuart)
            );

            // Panel_Apartment를 0.3초 뒤에 올리기 (약간의 딜레이와 함께)
            showSequence.Append(
                DOTween.To(() => apartmentRect.anchoredPosition,
                          x => apartmentRect.anchoredPosition = x,
                          apartmentOriginalPos, 0.8f)
                    .SetEase(Ease.OutQuart)
                    .SetDelay(0.3f)
            );

            // 완료 시 콜백 (선택사항)
            showSequence.AppendCallback(() =>
            {
                Debug.Log("GameExCanvasManager Show 애니메이션 완료");
                var commandRequest = Context.GetCommandRequest();
                commandRequest.Requests.Enqueue(new CommandRequest()
                {
                    Type = CommandType.StartGame,
                    PayLoad = null
                });
                Panel_Apartment.Show();
            });

            return showSequence;
        }

        public void Tick()
        {
            var end = Context.GetEntitiesWithComponent<EndGameCommand>();
            if (end.Count > 0)
            {
                var state = Context.GetGameState();

                switch (state.CurrentState)
                {
                    case GameState.Victory:
                        Text_Result.text = "완공!";
                        break;
                    case GameState.GameOver:
                        Text_Result.text = "삐빅\n 위반입니다.\n면허 취소 ㅠㅠ";
                        break;
                    default:
                        Debug.LogWarning("게임 종료 상태가 아닙니다. 현재 상태: " + state.CurrentState);
                        break;
                }

                Panel_Result.SetActive(true);
                Panel_Backblocker.SetActive(true);
                Button_Hide.interactable = true;
                return;
            }
        }

        public Tween Hide()
        {
            Debug.Log("GameExCanvasManager Hide 애니메이션 시작");
            Panel_Result.SetActive(false);
            Panel_Backblocker.SetActive(false);

            // 패널들의 RectTransform 가져오기
            RectTransform groundRect = Panel_Ground.GetComponent<RectTransform>();
            RectTransform apartmentRect = Panel_Apartment.GetComponent<RectTransform>();

            // 현재 위치 저장
            Vector2 groundCurrentPos = groundRect.anchoredPosition;
            Vector2 apartmentCurrentPos = apartmentRect.anchoredPosition;

            // 화면 아래 목표 위치 계산
            float screenHeight = Screen.height;
            Vector2 groundTargetPos = new Vector2(groundCurrentPos.x, groundCurrentPos.y - screenHeight);
            Vector2 apartmentTargetPos = new Vector2(apartmentCurrentPos.x, apartmentCurrentPos.y - screenHeight);

            // DOTween Sequence 생성
            Sequence hideSequence = DOTween.Sequence();

            // Panel_Apartment를 먼저 내리기 (Show와 반대 순서)
            hideSequence.Append(
                DOTween.To(() => apartmentRect.anchoredPosition,
                          x => apartmentRect.anchoredPosition = x,
                          apartmentTargetPos, 0.8f)
                    .SetEase(Ease.InQuart)
            );

            // Panel_Ground를 0.3초 뒤에 내리기
            hideSequence.Append(
                DOTween.To(() => groundRect.anchoredPosition,
                          x => groundRect.anchoredPosition = x,
                          groundTargetPos, 0.8f)
                    .SetEase(Ease.InQuart)
                    .SetDelay(0.3f)
            );

            // 완료 시 콜백 - 패널들 원위치로 복원 후 비활성화
            hideSequence.AppendCallback(() =>
            {
                // 원래 위치로 복원 (다음 Show를 위해)
                groundRect.anchoredPosition = groundCurrentPos;
                apartmentRect.anchoredPosition = apartmentCurrentPos;

                Panel_Apartment.Hide();

                // 패널들 비활성화
                Panel_Ground.gameObject.SetActive(false);
                Panel_Apartment.gameObject.SetActive(false);
                Debug.Log("GameExCanvasManager Hide 애니메이션 완료 - 원위치 복원됨");
            });

            return hideSequence;
        }

        public void Clear()
        {
            SceneManager = null;

            Panel_Ground.SetActive(false);
            Panel_Apartment.gameObject.SetActive(false);
            Panel_Result.SetActive(false);
            Panel_Backblocker.SetActive(false);

            Button_Hide.onClick.RemoveAllListeners();
            Button_Hide.interactable = false;
        }
    }
}