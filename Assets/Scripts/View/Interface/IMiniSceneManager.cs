using DG.Tweening;

public interface IMiniSceneManager
{
    // Dependeny Injection
    void Init();
    abstract Tween LoadScene();
    abstract Tween UnloadScene();
    abstract void Clear();
}