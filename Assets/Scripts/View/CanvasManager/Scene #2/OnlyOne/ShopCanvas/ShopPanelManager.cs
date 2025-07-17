using OVFL.ECS;
using UnityEngine;
using UnityEngine.UI;

namespace Minomino
{
    public class ShopPanelManager : MonoBehaviour, ISystem
    {
        public Context Context { get; set; }

        [SerializeField] private Button Button_Blueprint_1;
        [SerializeField] private Button Button_Blueprint_2;
        [SerializeField] private Button Button_Blueprint_3;

        [SerializeField] private BlueprintView BlueprintView_1;
        [SerializeField] private BlueprintView BlueprintView_2;
        [SerializeField] private BlueprintView BlueprintView_3;


        public void Show()
        {
            var tetrimino_1 = GenerateRandomTetriminoComponent();
            var tetrimino_2 = GenerateRandomTetriminoComponent();
            var tetrimino_3 = GenerateRandomTetriminoComponent();

            BlueprintView_1.Refresh(tetrimino_1);
            BlueprintView_2.Refresh(tetrimino_2);
            BlueprintView_3.Refresh(tetrimino_3);

            Button_Blueprint_1.interactable = true;
            Button_Blueprint_2.interactable = true;
            Button_Blueprint_3.interactable = true;

            RefreshCurrency();

            Button_Blueprint_1.onClick.AddListener(() =>
            {
                Context.CreateEntity()
                .AddComponent(tetrimino_1);
                Button_Blueprint_1.interactable = false;

                var player = GetPlayer();
                player.Currency -= 10;

                RefreshCurrency();

                var tetriminoEntities = Context.GetEntitiesWithComponent<TetriminoComponent>();
                Debug.Log($"선택된 테트리미노: {tetrimino_1.Type}, 현재 개수: {tetriminoEntities.Count}");

            });
            Button_Blueprint_2.onClick.AddListener(() =>
            {
                Context.CreateEntity()
                .AddComponent(tetrimino_2);
                Button_Blueprint_2.interactable = false;

                var player = GetPlayer();
                player.Currency -= 10;

                RefreshCurrency();

                var tetriminoEntities = Context.GetEntitiesWithComponent<TetriminoComponent>();
                Debug.Log($"선택된 테트리미노: {tetrimino_2.Type}, 현재 개수: {tetriminoEntities.Count}");
            });
            Button_Blueprint_3.onClick.AddListener(() =>
            {
                Context.CreateEntity()
                .AddComponent(tetrimino_3);
                Button_Blueprint_3.interactable = false;

                var player = GetPlayer();
                player.Currency -= 10;

                RefreshCurrency();

                var tetriminoEntities = Context.GetEntitiesWithComponent<TetriminoComponent>();
                Debug.Log($"선택된 테트리미노: {tetrimino_3.Type}, 현재 개수: {tetriminoEntities.Count}");
            });

            Debug.Log("ShopPanelManager가 활성화되었습니다.");
        }

        public void Hide()
        {
            Button_Blueprint_1.onClick.RemoveAllListeners();
            Button_Blueprint_2.onClick.RemoveAllListeners();
            Button_Blueprint_3.onClick.RemoveAllListeners();

            BlueprintView_1.Clear();
            BlueprintView_2.Clear();
            BlueprintView_3.Clear();
        }

        private void RefreshCurrency()
        {
            var player = GetPlayer();

            if (player.Currency < 10)
            {
                Button_Blueprint_1.interactable = false;
                Button_Blueprint_2.interactable = false;
                Button_Blueprint_3.interactable = false;
                Debug.LogWarning("플레이어의 통화가 부족하여 버튼을 비활성화합니다.");
            }
        }

        private TetriminoComponent GenerateRandomTetriminoComponent()
        {
            TetriminoType[] types = { TetriminoType.I, TetriminoType.O, TetriminoType.T,
                                    TetriminoType.S, TetriminoType.Z, TetriminoType.J, TetriminoType.L };
            TetriminoColor[] colors = { TetriminoColor.Red, TetriminoColor.Green, TetriminoColor.Blue };

            TetriminoType randomType = types[Random.Range(0, types.Length)];
            TetriminoColor randomColor = colors[Random.Range(0, colors.Length)];

            Vector2Int[] shape = GetShapeForType(randomType);

            var tetrimino = new TetriminoComponent();
            tetrimino.Type = randomType;
            tetrimino.Shape = shape;
            tetrimino.Color = randomColor;
            return tetrimino;
        }

        private static Vector2Int[] GetShapeForType(TetriminoType type)
        {
            switch (type)
            {
                case TetriminoType.I:
                    return new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) };
                case TetriminoType.O:
                    return new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) };
                case TetriminoType.T:
                    return new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) };
                case TetriminoType.S:
                    return new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) };
                case TetriminoType.Z:
                    return new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(1, 0) };
                case TetriminoType.J:
                    return new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0) };
                case TetriminoType.L:
                    return new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1) };
                default:
                    return new Vector2Int[] { new Vector2Int(0, 0) };
            }
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
    }
}