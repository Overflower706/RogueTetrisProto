using DG.Tweening;

public interface IMiniSceneManager
{
    // Dependeny Injection
    void Init(PanelSceneManager panelSceneManager);
    abstract Tween LoadScene();
    abstract Tween UnloadScene();
    abstract void Clear();
}