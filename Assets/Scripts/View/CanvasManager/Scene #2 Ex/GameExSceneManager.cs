using DG.Tweening;
using UnityEngine;

namespace Minomino
{
    public class GameExSceneManager : MonoBehaviour, IMiniSceneManager
    {
        public void Init()
        {
            gameObject.SetActive(false);
        }

        public void Clear()
        {
            gameObject.SetActive(false);
        }

        public Tween LoadScene()
        {
            gameObject.SetActive(true);
            return default;
        }

        public Tween UnloadScene()
        {
            return default;
        }
    }
}