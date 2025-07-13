using UnityEngine;
using UnityEngine.UI;
using OVFL.ECS;
using Minomino;
using System.Collections.Generic;

public class GameBoardPanel : MonoBehaviour
{
    public Context Context { get; set; }

    [Header("격자 (배경) 프리팹")]
    [SerializeField] private GameObject GridCellPrefab;

    [Header("테트리미노 프리팹")]
    [SerializeField] private GameObject TetriminoPrefab;

    [Header("부모 오브젝트")]
    [SerializeField] private RectTransform GridParent;
    [SerializeField] private Transform TetriminoParent;

    private int _width;
    private int _height;

    // (x, y) => gridCell 오브젝트
    private GameObject[,] _gridCells;

    // (x, y) => 블록 오브젝트
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

        // 기존 자식 제거
        foreach (Transform child in GridParent)
            Destroy(child.gameObject);

        _gridCells = new GameObject[_width, _height];

        // 배경 격자 생성 (GridLayoutGroup이 자동 정렬)
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
    /// 보드 상태를 받아 블럭 UI를 갱신 (생성/삭제/즉시 이동 및 색상)
    /// </summary>
    public void SetBoard(BoardComponent board)
    {
        if (board == null || board.Board == null)
            return;

        var toRemove = new List<Vector2Int>();

        // 기존 블록 UI 처리
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

        // --- 고스트 위치 색상 적용 ---
        var ghostPositions = GetHardDropGhostPositions(board);
        SetGhostCellColors(ghostPositions, new Color(0.2f, 0.2f, 0.2f, 1f), new Color(0.8f, 0.8f, 0.8f, 1f));
    }

    /// <summary>
    /// 고스트 위치는 darkColor, 나머지는 brightColor로 배경 격자 색상 변경
    /// </summary>
    private void SetGhostCellColors(Vector2Int[] ghostPositions, Color darkColor, Color brightColor)
    {
        // 고스트 위치 빠른 조회를 위해 HashSet 사용
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

    // tetriminoId 1~4에 맞는 색상만 반환
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
    /// 현재 내려오고 있는 테트로미노가 하드드롭 했을 때의 예상 위치(블록 좌표 배열)를 반환
    /// </summary>
    private Vector2Int[] GetHardDropGhostPositions(BoardComponent board)
    {
        // 1. 현재 테트로미노 엔티티 찾기
        var currentEntities = Context.GetEntitiesWithComponent<CurrentTetriminoComponent>();
        if (currentEntities == null || currentEntities.Count == 0)
            return null;

        var entity = currentEntities[0];
        var tetrimino = entity.GetComponent<TetriminoComponent>();
        var current = entity.GetComponent<CurrentTetriminoComponent>();
        if (tetrimino == null || current == null)
            return null;

        // 2. 회전 적용된 shape 구하기
        Vector2Int[] rotatedShape = new Vector2Int[tetrimino.Shape.Length];
        for (int i = 0; i < tetrimino.Shape.Length; i++)
            rotatedShape[i] = RotatePoint(tetrimino.Shape[i], tetrimino.Rotation);

        int entityId = entity.ID;

        // 3. 하드드롭 위치 계산 (자기 자신은 무시)
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
                // 자기 자신은 무시
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

        // 4. 최종 위치 반환
        Vector2Int[] ghostPositions = new Vector2Int[rotatedShape.Length];
        for (int i = 0; i < rotatedShape.Length; i++)
            ghostPositions[i] = dropPosition + rotatedShape[i];
        return ghostPositions;
    }

    // shape의 각 점을 회전시키는 함수 (시계방향 90도씩)
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