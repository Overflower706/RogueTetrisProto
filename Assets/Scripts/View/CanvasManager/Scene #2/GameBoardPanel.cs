using UnityEngine;
using UnityEngine.UI;
using OVFL.ECS;
using Minomino;
using System.Collections.Generic;

public class GameBoardPanel : MonoBehaviour
{
    public Context Context { get; set; }

    [Header("���� (���) ������")]
    [SerializeField] private GameObject GridCellPrefab;

    [Header("��Ʈ���̳� ������")]
    [SerializeField] private GameObject TetriminoPrefab;

    [Header("�θ� ������Ʈ")]
    [SerializeField] private RectTransform GridParent;
    [SerializeField] private Transform TetriminoParent;

    private int _width;
    private int _height;

    // (x, y) => gridCell ������Ʈ
    private GameObject[,] _gridCells;

    // (x, y) => ��� ������Ʈ
    private Dictionary<Vector2Int, GameObject> _tetriminoObjects = new();

    public void Init(Context context)
    {
        Context = context;
        InitGrid();
    }

    private void InitGrid()
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

        // ���� ��� UI ó��
        for (int y = 0; y < BoardComponent.HEIGHT; y++)
        {
            for (int x = 0; x < BoardComponent.WIDTH; x++)
            {
                int tetriminoId = board.Board[x, y];
                var pos = new Vector2Int(x, y);

                if (tetriminoId == 0)
                {
                    if (_tetriminoObjects.TryGetValue(pos, out var obj) && obj != null)
                    {
                        Destroy(obj);
                        toRemove.Add(pos);
                    }
                }
                else
                {
                    GameObject tetriminoObj;
                    if (!_tetriminoObjects.TryGetValue(pos, out tetriminoObj) || tetriminoObj == null)
                    {
                        tetriminoObj = Instantiate(TetriminoPrefab, TetriminoParent);
                        var gridCellRect = _gridCells[x, y].GetComponent<RectTransform>();
                        var tetriminoRect = tetriminoObj.GetComponent<RectTransform>();

                        Vector2 worldPos = gridCellRect.TransformPoint(gridCellRect.rect.center);
                        Vector2 localPos;
                        RectTransform tetriminoParentRect = TetriminoParent as RectTransform;
                        if (tetriminoParentRect != null)
                        {
                            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                                tetriminoParentRect,
                                RectTransformUtility.WorldToScreenPoint(null, worldPos),
                                null,
                                out localPos
                            );
                            tetriminoRect.anchoredPosition = localPos;
                        }
                        else
                        {
                            tetriminoRect.position = worldPos;
                        }
                        _tetriminoObjects[pos] = tetriminoObj;
                    }
                    var image = tetriminoObj.GetComponent<Image>();
                    var tetriminoEntitie = Context.GetEntities()[tetriminoId].GetComponent<TetriminoComponent>();
                    image.color = GettetriminoColor(tetriminoEntitie.Color);
                    tetriminoObj.SetActive(true);
                }
            }
        }

        foreach (var pos in toRemove)
            _tetriminoObjects.Remove(pos);

        // --- ��Ʈ ��ġ ���� ���� ---
        var ghostPositions = GetHardDropGhostPositions(board);
        SetGhostCellColors(ghostPositions, new Color(0.2f, 0.2f, 0.2f, 1f), new Color(0.8f, 0.8f, 0.8f, 1f));
    }

    /// <summary>
    /// ��Ʈ ��ġ�� darkColor, �������� brightColor�� ��� ���� ���� ����
    /// </summary>
    private void SetGhostCellColors(Vector2Int[] ghostPositions, Color darkColor, Color brightColor)
    {
        // ��Ʈ ��ġ ���� ��ȸ�� ���� HashSet ���
        HashSet<Vector2Int> ghostSet = ghostPositions != null ? new HashSet<Vector2Int>(ghostPositions) : new HashSet<Vector2Int>();

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                var cellImage = _gridCells[x, y].GetComponent<Image>();
                if (ghostSet.Contains(new Vector2Int(x, y)))
                    cellImage.color = darkColor;
                else
                    cellImage.color = brightColor;
            }
        }
    }

    // tetriminoId 1~4�� �´� ���� ��ȯ
    private Color GettetriminoColor(TetriminoColor tetriminoColor)
    {
        switch (tetriminoColor)
        {
            case TetriminoColor.Red: return Color.red;
            case TetriminoColor.Green: return Color.green;
            case TetriminoColor.Blue: return Color.blue;
            case TetriminoColor.Yellow: return Color.yellow;
            default: return Color.white;
        }
    }

    /// <summary>
    /// ���� �������� �ִ� ��Ʈ�ι̳밡 �ϵ��� ���� ���� ���� ��ġ(��� ��ǥ �迭)�� ��ȯ
    /// </summary>
    private Vector2Int[] GetHardDropGhostPositions(BoardComponent board)
    {
        // 1. ���� ��Ʈ�ι̳� ��ƼƼ ã��
        var currentEntities = Context.GetEntitiesWithComponent<CurrentTetriminoComponent>();
        if (currentEntities == null || currentEntities.Count == 0)
            return null;

        var entity = currentEntities[0];
        var tetrimino = entity.GetComponent<TetriminoComponent>();
        var current = entity.GetComponent<CurrentTetriminoComponent>();
        if (tetrimino == null || current == null)
            return null;

        // 2. ȸ�� ����� shape ���ϱ�
        Vector2Int[] rotatedShape = new Vector2Int[tetrimino.Shape.Length];
        for (int i = 0; i < tetrimino.Shape.Length; i++)
            rotatedShape[i] = RotatePoint(tetrimino.Shape[i], tetrimino.Rotation);

        int entityId = entity.ID;

        // 3. �ϵ��� ��ġ ��� (�ڱ� �ڽ��� ����)
        Vector2Int dropPosition = current.Position;
        while (true)
        {
            Vector2Int testPosition = dropPosition + Vector2Int.down;
            bool collision = false;
            foreach (var local in rotatedShape)
            {
                Vector2Int world = testPosition + local;
                if (world.x < 0 || world.x >= BoardComponent.WIDTH || world.y < 0)
                {
                    collision = true;
                    break;
                }
                // �ڱ� �ڽ��� ����
                if (world.y < BoardComponent.HEIGHT)
                {
                    int cellId = board.Board[world.x, world.y];
                    if (cellId != 0 && cellId != entityId)
                    {
                        collision = true;
                        break;
                    }
                }
            }
            if (collision)
                break;
            dropPosition = testPosition;
        }

        // 4. ���� ��ġ ��ȯ
        Vector2Int[] ghostPositions = new Vector2Int[rotatedShape.Length];
        for (int i = 0; i < rotatedShape.Length; i++)
            ghostPositions[i] = dropPosition + rotatedShape[i];
        return ghostPositions;
    }

    // shape�� �� ���� ȸ����Ű�� �Լ� (�ð���� 90����)
    private Vector2Int RotatePoint(Vector2Int point, int rotation)
    {
        for (int i = 0; i < rotation; i++)
        {
            int temp = point.x;
            point.x = -point.y;
            point.y = temp;
        }
        return point;
    }
}