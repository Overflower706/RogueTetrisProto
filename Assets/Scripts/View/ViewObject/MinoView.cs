using OVFL.ECS;
using UnityEngine;
using UnityEngine.UI;

namespace Minomino
{
    public class MinoView : MonoBehaviour
    {
        [SerializeField] private Image Image_Background;
        private Entity _minoEntity;
        private MinoComponent _minoComponent;

        public void Refresh(Entity minoEntity = null)
        {
            if (minoEntity == null)
            {
                _minoEntity = null;
                Image_Background.sprite = null;
                Image_Background.color = new Color(0, 0, 0, 0); // 투명하게 설정
                return;
            }

            // 새로운 엔티티가 설정되면 업데이트
            _minoEntity = minoEntity;
            Image_Background.color = new Color(1, 1, 1, 1); // 불투명하게 설정
            GetSpriteByState(_minoEntity.GetComponent<MinoComponent>().State);
        }

        private void GetSpriteByState(MinoState state)
        {
            switch (state)
            {
                case MinoState.None:
                    Debug.LogWarning($"None state MinoComponent");
                    break;
                case MinoState.Empty:
                    // int emptyIndex = Random.Range(0, GlobalSettings.Instance.Sprites_Empty.Length);
                    Image_Background.sprite = GlobalSettings.Instance.Sprites_Empty[0];
                    break;
                case MinoState.Living:
                    // int livingIndex = Random.Range(0, GlobalSettings.Instance.Sprites_Living.Length);
                    Image_Background.sprite = GlobalSettings.Instance.Sprites_Living[0];
                    break;
                default:
                    Debug.LogWarning($"Unknown MinoState: {state}");
                    break;
            }
        }
    }
}