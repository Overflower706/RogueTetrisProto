using DG.Tweening;

public interface ICanvasManager
{
    IMiniSceneManager SceneManager { get; }
    public Game GameData { get; }
    void Init(IMiniSceneManager sceneManager);
    Tween Show();
    Tween Hide();
    void Clear();
}