using DG.Tweening;

public interface ICanvasManager
{
    IMiniSceneManager SceneManager { get; }
    void Init(IMiniSceneManager sceneManager);
    Tween Show();
    Tween Hide();
    void Clear();
}