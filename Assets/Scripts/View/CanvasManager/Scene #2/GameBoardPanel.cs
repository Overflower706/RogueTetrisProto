using UnityEngine;
using UnityEngine.UI;
using OVFL.ECS;
using Minomino;
using System.Collections.Generic;

public class GameBoardPanel : MonoBehaviour
{
    [Header("����(���) ������")]
    [SerializeField] private GameObject GridCellPrefab;

    [Header("���(����) ������")]
    [SerializeField] private GameObject BlockPrefab;

    [Header("�θ� ������Ʈ")]
    [SerializeField] private RectTransform GridParent;
    [SerializeField] private Transform BlockParent;

    private int _width;
    private int _height;

    // (x, y) => gridCell ������Ʈ
    private GameObject[,] _gridCells;

    // (x, y) => ��� ������Ʈ
    private Dictionary<Vector2Int, GameObject> _blockObjects = new();

    public void InitGrid()
    {
        _width = BoardComponent.WIDTH;
        _height = BoardComponent.HEIGHT;

        // ���� �ڽ� ����
        foreach (Transform child in GridParent)
            Destroy(child.gameObject);

        _gridCells = new GameObject[_width, _height];

        // ��� ���� ���� (GridLayoutGroup�� �ڵ� ����)
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                var gridCell = Instantiate(GridCellPrefab, GridParent);
                _gridCells[x, y] = gridCell;
            }
        }
    }

    /// <summary>
    /// ���� ���¸� �޾� �� UI�� ���� (����/����/��� �̵� �� ����)
    /// </summary>
    public void SetBoard(BoardComponent board)
    {
        if (board == null || board.Board == null)
            return;

        var toRemove = new List<Vector2Int>();

        // 1. ��ü ��ǥ ��ȸ
        for (int y = 0; y < BoardComponent.HEIGHT; y++)
        {
            for (int x = 0; x < BoardComponent.WIDTH; x++)
            {
                int blockId = board.Board[x, y];
                var pos = new Vector2Int(x, y);

                if (blockId == 0)
                {
                    // ����� ������ ����
                    if (_blockObjects.TryGetValue(pos, out var obj) && obj != null)
                    {
                        Destroy(obj);
                        toRemove.Add(pos);
                    }
                }
                else // 1~4: ��� ǥ��
                {
                    GameObject blockObj;
                    if (!_blockObjects.TryGetValue(pos, out blockObj) || blockObj == null)
                    {
                        // �� ��� ����
                        blockObj = Instantiate(BlockPrefab, BlockParent);
                        // gridCell�� ���� ��ǥ�� BlockParent�� localPosition���� ��ȯ
                        var gridCellRect = _gridCells[x, y].GetComponent<RectTransform>();
                        var blockRect = blockObj.GetComponent<RectTransform>();

                        Vector2 worldPos = gridCellRect.TransformPoint(gridCellRect.rect.center);
                        Vector2 localPos;
                        RectTransform blockParentRect = BlockParent as RectTransform;
                        if (blockParentRect != null)
                        {
                            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                                blockParentRect,
                                RectTransformUtility.WorldToScreenPoint(null, worldPos),
                                null,
                                out localPos
                            );
                            blockRect.anchoredPosition = localPos;
                        }
                        else
                        {
                            // BlockParent�� RectTransform�� �ƴϸ� fallback
                            blockRect.position = worldPos;
                        }
                        _blockObjects[pos] = blockObj;
                    }
                    // ���� ����
                    var image = blockObj.GetComponent<Image>();
                    image.color = GetBlockColor(blockId);
                    blockObj.SetActive(true);
                }
            }
        }

        // 2. ������ ��� ��ųʸ����� ����
        foreach (var pos in toRemove)
            _blockObjects.Remove(pos);
    }

    // blockId 1~4�� �´� ���� ��ȯ
    private Color GetBlockColor(int blockId)
    {
        switch (blockId)
        {
            case 1: return Color.cyan;
            case 2: return Color.yellow;
            case 3: return Color.magenta;
            case 4: return Color.green;
            default: return Color.white;
        }
    }
}