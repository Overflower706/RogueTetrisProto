using UnityEngine;
using UnityEngine.UI;
using OVFL.ECS;
using Minomino;
using System.Collections.Generic;

public class GameBoardPanel : MonoBehaviour
{
    public Context Context { get; set; }
    public bool IsInit => _isInit;
    private bool _isInit = false;

    [Header("그리드 (셀) 프리팹")]
    [SerializeField] private GameObject GridCellPrefab;

    [Header("테트리미노 프리팹")]
    [SerializeField] private GameObject TetriminoPrefab;

    [Header("부모 오브젝트")]
    [SerializeField] private RectTransform GridParent;
    [SerializeField] private Transform TetriminoParent;

    [Header("홀드 테트리미노 UI")]
    [SerializeField] private Transform HoldTetriminoParent;
    [SerializeField] private GameObject HoldTetriminoPrefab;

    private int _width;
    private int _height;

    // (x, y) => gridCell 오브젝트
    private GameObject[,] _gridCells;

    // (x, y) => 블록 오브젝트
    private Dictionary<Vector2Int, GameObject> _tetriminoObjects = new();

    // 홀드 UI 관련
    private GameObject[,] _holdAnchors; // 4x4 홀드 앵커 (항상 켜짐)
    private GameObject[,] _holdImages; // 4x4 홀드 이미지 (블록 표시용)
    private const int HOLD_SIZE = 4;

    public void Init(Context context)
    {
        Context = context;
        _isInit = true;
        InitGrid();
        InitHoldArea();
    }

    private void InitGrid()
    {
        _width = BoardComponent.WIDTH;
        _height = BoardComponent.HEIGHT;

        // 기존 자식 삭제
        foreach (Transform child in GridParent)
            Destroy(child.gameObject);

        _gridCells = new GameObject[_width, _height];

        // 모든 셀을 생성 (GridLayoutGroup이 자동 배치)
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
    /// 4x4 홀드 영역 초기화
    /// </summary>
    private void InitHoldArea()
    {
        if (HoldTetriminoParent == null)
        {
            Debug.LogWarning("HoldTetriminoParent가 설정되지 않았습니다.");
            return;
        }

        _holdAnchors = new GameObject[HOLD_SIZE, HOLD_SIZE];
        _holdImages = new GameObject[HOLD_SIZE, HOLD_SIZE];

        // GridLayoutGroup의 자식들(Anchor)을 가져와서 배열에 저장
        int index = 0;
        foreach (Transform anchorTransform in HoldTetriminoParent)
        {
            if (index >= HOLD_SIZE * HOLD_SIZE) break;
            
            int x = index % HOLD_SIZE;
            int y = index / HOLD_SIZE;
            
            GameObject anchor = anchorTransform.gameObject;
            _holdAnchors[x, y] = anchor;
            
            // Anchor의 첫 번째 자식이 Image_Tetrimino
            if (anchor.transform.childCount > 0)
            {
                GameObject imageObj = anchor.transform.GetChild(0).gameObject;
                _holdImages[x, y] = imageObj;
                imageObj.SetActive(false); // 초기에는 비활성화
            }
            
            index++;
        }
    }

    /// <summary>
    /// 보드 상태를 받아 시각 UI를 갱신 (배치/삭제/라인 이동 시 호출)
    /// </summary>
    public void SetBoard(BoardComponent board)
    {
        if (board == null || board.Board == null) return;

        var toRemove = new List<Vector2Int>();
        var (currentTetrimino, currentEntityId) = GetCurrentTetriminoInfo();

        // 기존 블록 UI 처리
        for (int y = 0; y < BoardComponent.HEIGHT; y++)
        {
            for (int x = 0; x < BoardComponent.WIDTH; x++)
            {
                int tetriminoId = board.Board[x, y];
                var pos = new Vector2Int(x, y);

                if (tetriminoId == 0)
                {
                    RemoveTetriminoObject(pos, toRemove);
                }
                else
                {
                    CreateOrUpdateTetriminoObject(pos, tetriminoId, currentTetrimino, currentEntityId);
                }
            }
        }

        // 제거할 오브젝트들 정리
        foreach (var pos in toRemove)
            _tetriminoObjects.Remove(pos);

        // UI 업데이트
        UpdateGhostEffect(board);
        UpdateHoldUI();
    }

    /// <summary>
    /// 홀드 테트리미노 UI 업데이트
    /// </summary>
    private void UpdateHoldUI()
    {
        ClearHoldUI();
        
        var holdTetrimino = GetHoldTetrimino();
        if (holdTetrimino != null)
        {
            DisplayHoldTetrimino(holdTetrimino);
        }
    }

    /// <summary>
    /// 홀드된 테트리미노를 가져오는 헬퍼 메서드
    /// </summary>
    private TetriminoComponent GetHoldTetrimino()
    {
        var holdEntities = Context.GetEntitiesWithComponent<HoldTetriminoComponent>();
        return holdEntities?.Count > 0 ? holdEntities[0].GetComponent<TetriminoComponent>() : null;
    }

    /// <summary>
    /// 모든 홀드 이미지 비활성화
    /// </summary>
    private void ClearHoldUI()
    {
        if (_holdImages == null) return;

        for (int i = 0; i < HOLD_SIZE * HOLD_SIZE; i++)
        {
            int x = i % HOLD_SIZE;
            int y = i / HOLD_SIZE;
            _holdImages[x, y]?.SetActive(false);
        }
    }

    /// <summary>
    /// 홀드된 테트리미노를 4x4 영역에 표시
    /// </summary>
    private void DisplayHoldTetrimino(TetriminoComponent tetrimino)
    {
        if (_holdImages == null || tetrimino?.Shape == null) return;

        Color tetriminoColor = GetTetriminoColor(tetrimino.Color);
        Vector2Int centerOffset = new Vector2Int(1, 1); // 4x4에서 중앙 기준

        foreach (var shapePos in tetrimino.Shape)
        {
            Vector2Int holdPos = centerOffset + shapePos;
            
            if (IsValidHoldPosition(holdPos))
            {
                var holdImage = _holdImages[holdPos.x, holdPos.y];
                if (holdImage != null)
                {
                    holdImage.SetActive(true);
                    var image = holdImage.GetComponent<Image>();
                    if (image != null) image.color = tetriminoColor;
                }
            }
        }
    }

    /// <summary>
    /// 홀드 위치가 유효한지 확인하는 헬퍼 메서드
    /// </summary>
    private bool IsValidHoldPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < HOLD_SIZE && pos.y >= 0 && pos.y < HOLD_SIZE;
    }

    /// <summary>
    /// 고스트 위치는 darkColor, 나머지는 brightColor로 셀의 배경 색상 설정
    /// </summary>
    private void SetGhostCellColors(Vector2Int[] ghostPositions, Color darkColor, Color brightColor)
    {
        // 고스트 위치 빠른 검회를 위한 HashSet 생성
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

    // tetriminoId 1~4에 맞는 색상 반환
    private Color GetTetriminoColor(TetriminoColor tetriminoColor)
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
    /// 현재 활성화된 있는 테트리미노가 하드롭 했을 때의 예상 위치(월드 좌표 배열)를 반환
    /// </summary>
    private Vector2Int[] GetHardDropGhostPositions(BoardComponent board)
    {
        // 1. 현재 테트리미노 엔티티 찾기
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

        // 3. 하드롭 위치 계산 (자기 자신은 무시)
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

    // shape의 각 점을 회전시키는 함수 (반시계방향 90도씩)
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


    /// <summary>
    /// 현재 테트리미노와 위치 정보를 함께 가져오는 헬퍼 메서드
    /// </summary>
    private (TetriminoComponent tetrimino, CurrentTetriminoComponent position) GetCurrentTetriminoWithPosition()
    {
        var currentEntities = Context.GetEntitiesWithComponent<CurrentTetriminoComponent>();
        if (currentEntities?.Count > 0)
        {
            var entity = currentEntities[0];
            return (entity.GetComponent<TetriminoComponent>(), entity.GetComponent<CurrentTetriminoComponent>());
        }
        return (null, null);
    }

    /// <summary>
    /// 현재 테트리미노 정보를 가져오는 헬퍼 메서드
    /// </summary>
    private (TetriminoComponent tetrimino, int entityId) GetCurrentTetriminoInfo()
    {
        var currentEntities = Context.GetEntitiesWithComponent<CurrentTetriminoComponent>();
        if (currentEntities != null && currentEntities.Count > 0)
        {
            var entity = currentEntities[0];
            return (entity.GetComponent<TetriminoComponent>(), entity.ID);
        }
        return (null, 0);
    }

    /// <summary>
    /// 테트리미노 오브젝트 제거 헬퍼 메서드
    /// </summary>
    private void RemoveTetriminoObject(Vector2Int pos, List<Vector2Int> toRemove)
    {
        if (_tetriminoObjects.TryGetValue(pos, out var obj) && obj != null)
        {
            Destroy(obj);
            toRemove.Add(pos);
        }
    }

    /// <summary>
    /// 테트리미노 오브젝트 생성/업데이트 헬퍼 메서드
    /// </summary>
    private void CreateOrUpdateTetriminoObject(Vector2Int pos, int tetriminoId, TetriminoComponent currentTetrimino, int currentEntityId)
    {
        GameObject tetriminoObj = GetOrCreateTetriminoObject(pos);
        var image = tetriminoObj.GetComponent<Image>();
        
        // 색상 설정
        Color color = GetTetriminoColorById(tetriminoId, currentTetrimino, currentEntityId);
        image.color = color;
        
        tetriminoObj.SetActive(true);
    }

    /// <summary>
    /// 테트리미노 오브젝트를 가져오거나 생성하는 헬퍼 메서드
    /// </summary>
    private GameObject GetOrCreateTetriminoObject(Vector2Int pos)
    {
        if (_tetriminoObjects.TryGetValue(pos, out var tetriminoObj) && tetriminoObj != null)
        {
            return tetriminoObj;
        }

        // 새 오브젝트 생성
        tetriminoObj = Instantiate(TetriminoPrefab, TetriminoParent);
        var gridCellRect = _gridCells[pos.x, pos.y].GetComponent<RectTransform>();
        var tetriminoRect = tetriminoObj.GetComponent<RectTransform>();

        // 위치 설정
        SetTetriminoPosition(gridCellRect, tetriminoRect);
        _tetriminoObjects[pos] = tetriminoObj;
        
        return tetriminoObj;
    }

    /// <summary>
    /// 테트리미노 위치 설정 헬퍼 메서드
    /// </summary>
    private void SetTetriminoPosition(RectTransform gridCellRect, RectTransform tetriminoRect)
    {
        Vector2 worldPos = gridCellRect.TransformPoint(gridCellRect.rect.center);
        RectTransform tetriminoParentRect = TetriminoParent as RectTransform;
        
        if (tetriminoParentRect != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                tetriminoParentRect,
                RectTransformUtility.WorldToScreenPoint(null, worldPos),
                null,
                out Vector2 localPos
            );
            tetriminoRect.anchoredPosition = localPos;
        }
        else
        {
            tetriminoRect.position = worldPos;
        }
    }

    /// <summary>
    /// 테트리미노 ID로 색상을 가져오는 헬퍼 메서드
    /// </summary>
    private Color GetTetriminoColorById(int tetriminoId, TetriminoComponent currentTetrimino, int currentEntityId)
    {
        // 현재 테트리미노인 경우
        if (currentTetrimino != null && tetriminoId == currentEntityId)
        {
            return GetTetriminoColor(currentTetrimino.Color);
        }

        // 배치된 블록인 경우
        var placedTetrimino = FindEntityById(tetriminoId);
        return placedTetrimino != null 
            ? GetTetriminoColor(placedTetrimino.Color) 
            : Color.white;
    }

    /// <summary>
    /// ID로 엔티티를 찾는 헬퍼 메서드
    /// </summary>
    private TetriminoComponent FindEntityById(int entityId)
    {
        var entities = Context.GetEntities();
        foreach (var entity in entities)
        {
            if (entity.ID == entityId)
            {
                return entity.GetComponent<TetriminoComponent>();
            }
        }
        return null;
    }

    /// <summary>
    /// 고스트 효과 업데이트 헬퍼 메서드
    /// </summary>
    private void UpdateGhostEffect(BoardComponent board)
    {
        var ghostPositions = GetHardDropGhostPositions(board);
        SetGhostCellColors(ghostPositions, new Color(0.2f, 0.2f, 0.2f, 1f), new Color(0.8f, 0.8f, 0.8f, 1f));
    }
}