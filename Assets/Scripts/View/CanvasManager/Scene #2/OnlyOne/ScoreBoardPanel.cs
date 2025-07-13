using UnityEngine;
using UnityEngine.UI;
using OVFL.ECS;
using Minomino;
using TMPro;

public class ScoreBoardPanel : MonoBehaviour
{
    public Context Context { get; set; }

    [Header("점수 UI")]
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

    public void Clear()
    {
        _isInitialized = false;
        Context = null;
    }
}