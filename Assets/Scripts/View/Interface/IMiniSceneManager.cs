using DG.Tweening;

public interface IMiniSceneManager
{
    Game GameData { get; }
    // Dependeny Injection
    void Init(PanelSceneManager panelSceneManager);
    abstract Tween LoadScene();
    abstract Tween UnloadScene();
    abstract void Clear();
}