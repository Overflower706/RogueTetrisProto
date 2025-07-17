using UnityEngine;
using OVFL.ECS;
using Minomino;
using TMPro;

public class ScoreBoardPanel : MonoBehaviour
{
    public Context Context { get; set; }

    [Header("점수 UI")]
    [SerializeField] private TMP_Text text_PlayerCurrency;
    [SerializeField] private TextMeshProUGUI targetScoreText;
    [SerializeField] private TextMeshProUGUI currentScoreText;

    private bool _isInitialized = false;

    public void Init(Context context)
    {
        Context = context;
        _isInitialized = true;
        UpdateScoreDisplay();
    }

    private void Update()
    {
        if (!_isInitialized || Context == null) return;

        UpdateScoreDisplay();
    }

    /// <summary>
    /// ECS에서 점수 컴포넌트를 가져와 UI 업데이트
    /// </summary>
    private void UpdateScoreDisplay()
    {
        var scoreComponent = GetScoreComponent();
        if (scoreComponent == null)
        {
            Debug.Log("ScoreComponent가 없습니다.");
            return;
        }

        var player = GetPlayer();

        text_PlayerCurrency.text = player.Currency.ToString("N0");

        // UI 텍스트 업데이트
        if (currentScoreText != null)
            currentScoreText.text = $"{scoreComponent.CurrentScore:N0}";

        if (targetScoreText != null)
            targetScoreText.text = $"{scoreComponent.TargetScore:N0}";

    }

    /// <summary>
    /// ECS Context에서 ScoreComponent를 안전하게 가져오는 헬퍼 메서드
    /// </summary>
    private ScoreComponent GetScoreComponent()
    {
        var scoreEntities = Context.GetEntitiesWithComponent<ScoreComponent>();

        if (scoreEntities == null || scoreEntities.Count == 0)
            return null;

        if (scoreEntities.Count > 1)
        {
            Debug.LogWarning($"ScoreComponent가 {scoreEntities.Count}개 존재합니다. 첫 번째 것을 사용합니다.");
        }

        return scoreEntities[0].GetComponent<ScoreComponent>();
    }

    private PlayerComponent GetPlayer()
    {
        var playerEntities = Context.GetEntitiesWithComponent<PlayerComponent>();
        if (playerEntities.Count == 0)
        {
            Debug.LogWarning("PlayerComponent가 있는 엔티티가 없습니다.");
            return null;
        }
        else if (playerEntities.Count > 1)
        {
            Debug.LogWarning("PlayerComponent가 여러 엔티티에 존재합니다. 하나의 엔티티만 사용해야 합니다.");
            return null;
        }

        return playerEntities[0].GetComponent<PlayerComponent>();
    }

    public void Clear()
    {
        _isInitialized = false;
        Context = null;
    }
}