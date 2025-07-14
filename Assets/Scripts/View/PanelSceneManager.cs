using DG.Tweening;
using Minomino;
using OVFL.ECS;
using UnityEngine;

public enum SceneType
{
    None,
    Title,
    Game,
    Pack
}

public class PanelSceneManager : MonoSingleton<PanelSceneManager>, ISetupSystem
{
    public Context Context { get; set; }

    private IMiniSceneManager _currentSceneManager;

    [Header("각각의 씬 담당들")]
    [SerializeField] private TitleSceneManager TitleSceneManager;
    [SerializeField] private GameSceneManager GameSceneManager;
    [SerializeField] private PackSceneManager PackSceneManager;

    [Header("현재 씬 시각화")]
    [field: SerializeField] public SceneType CurrentSceneType { get; private set; }

    private SceneType GetCurrentSceneType()
    {
        return _currentSceneManager switch
        {
            _ when (Object)_currentSceneManager == TitleSceneManager => SceneType.Title,
            _ when (Object)_currentSceneManager == GameSceneManager => SceneType.Game,
            _ when (Object)_currentSceneManager == PackSceneManager => SceneType.Pack,
            _ => SceneType.None
        };
    }

    public void Setup()
    {
        TitleSceneManager.Init(this);
        GameSceneManager.Init(this);
        PackSceneManager.Init(this);

        LoadTitleScene();
    }

    public void LoadTitleScene()
    {
        var sequence = DOTween.Sequence();
        if (_currentSceneManager != null)
        {
            // 현재 SceneManager가 있다면 UnloadScene 호출
            sequence.Append(_currentSceneManager.UnloadScene());
        }
        sequence.Append(TitleSceneManager.LoadScene());
        sequence.AppendCallback(() =>
        {
            _currentSceneManager = TitleSceneManager;
            CurrentSceneType = GetCurrentSceneType();
        });
    }

    public void LoadGameScene()
    {
        var sequence = DOTween.Sequence();
        if (_currentSceneManager != null)
        {
            // 현재 SceneManager가 있다면 UnloadScene 호출
            sequence.Append(_currentSceneManager.UnloadScene());
        }
        sequence.Append(GameSceneManager.LoadScene());
        sequence.AppendCallback(() =>
        {
            _currentSceneManager = GameSceneManager;
            CurrentSceneType = GetCurrentSceneType();
        });
    }

    public void LoadPackScene()
    {
        var sequence = DOTween.Sequence();
        if (_currentSceneManager != null)
        {
            // 현재 SceneManager가 있다면 UnloadScene 호출
            sequence.Append(_currentSceneManager.UnloadScene());
        }
        sequence.Append(PackSceneManager.LoadScene());
        sequence.AppendCallback(() =>
        {
            _currentSceneManager = PackSceneManager;
            CurrentSceneType = GetCurrentSceneType();
        });
    }
}