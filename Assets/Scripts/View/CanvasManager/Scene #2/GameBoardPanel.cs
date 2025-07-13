using UnityEngine;
using UnityEngine.UI;
using OVFL.ECS;
using Minomino;
using System.Collections.Generic;

public class GameBoardPanel : MonoBehaviour
{
    public Context Context { get; set; }

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
        if (board == null || board.Board == null)
            return;

        var toRemove = new List<Vector2Int>();

        // 현재 테트리미노 정보 미리 가져오기
        TetriminoComponent currentTetrimino = null;
        int currentTetriminoEntityId = 0;
        var currentEntities = Context.GetEntitiesWithComponent<CurrentTetriminoComponent>();
        if (currentEntities != null && currentEntities.Count > 0)
        {
            currentTetrimino = currentEntities[0].GetComponent<TetriminoComponent>();
            currentTetriminoEntityId = currentEntities[0].ID;
        }

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
                    
                    // 현재 테트리미노의 색상 사용 (tetriminoId가 현재 테트리미노의 엔티티 ID와 일치하는 경우)
                    if (currentTetrimino != null && tetriminoId == currentTetriminoEntityId)
                    {
                        image.color = GetTetriminoColor(currentTetrimino.Color);
                    }
                    else
                    {
                        // 배치된 블록의 경우: 모든 엔티티를 검색해서 해당 ID를 찾아 색상 적용
                        var entities = Context.GetEntities();
                        TetriminoComponent placedTetrimino = null;
                        
                        foreach (var entity in entities)
                        {
                            if (entity.ID == tetriminoId)
                            {
                                placedTetrimino = entity.GetComponent<TetriminoComponent>();
                                break;
                            }
                        }
                        
                        if (placedTetrimino != null)
                        {
                            image.color = GetTetriminoColor(placedTetrimino.Color);
                        }
                        else
                        {
                            // 엔티티를 찾을 수 없는 경우 기본 색상 사용
                            image.color = Color.white;
                        }
                    }

                    tetriminoObj.SetActive(true);
                }
            }
        }

        foreach (var pos in toRemove)
            _tetriminoObjects.Remove(pos);

        // --- 고스트 위치 시각 효과 ---
        var ghostPositions = GetHardDropGhostPositions(board);
        SetGhostCellColors(ghostPositions, new Color(0.2f, 0.2f, 0.2f, 1f), new Color(0.8f, 0.8f, 0.8f, 1f));

        // --- 현재 테트리미노 및 홀드 정보 로그 ---
        LogCurrentAndHoldTetrimino();

        // --- 홀드 테트리미노 UI 업데이트 ---
        UpdateHoldUI();
    }

    /// <summary>
    /// 홀드 테트리미노 UI 업데이트
    /// </summary>
    private void UpdateHoldUI()
    {
        // 모든 홀드 셀 비활성화
        ClearHoldUI();

        // 홀드된 테트리미노 찾기
        var holdEntities = Context.GetEntitiesWithComponent<HoldTetriminoComponent>();
        if (holdEntities == null || holdEntities.Count == 0)
            return;

        var holdEntity = holdEntities[0];
        var holdTetrimino = holdEntity.GetComponent<TetriminoComponent>();
        if (holdTetrimino == null)
            return;

        // 홀드된 테트리미노를 중앙의 4x4 영역에 표시
        DisplayHoldTetrimino(holdTetrimino);
    }

    /// <summary>
    /// 모든 홀드 이미지 비활성화
    /// </summary>
    private void ClearHoldUI()
    {
        if (_holdImages == null) return;

        for (int y = 0; y < HOLD_SIZE; y++)
        {
            for (int x = 0; x < HOLD_SIZE; x++)
            {
                if (_holdImages[x, y] != null)
                    _holdImages[x, y].SetActive(false);
            }
        }
    }

    /// <summary>
    /// 홀드된 테트리미노를 4x4 영역에 표시
    /// </summary>
    private void DisplayHoldTetrimino(TetriminoComponent tetrimino)
    {
        if (_holdImages == null || tetrimino == null || tetrimino.Shape == null)
            return;

        // 테트리미노 색상 가져오기
        Color tetriminoColor = GetTetriminoColor(tetrimino.Color);

        // 4x4 영역의 중앙에 테트리미노 배치 (회전은 0으로 고정)
        Vector2Int centerOffset = new Vector2Int(1, 1); // 4x4에서 중앙은 (1,1) 기준

        foreach (var shapePos in tetrimino.Shape)
        {
            // 홀드 영역에서는 회전을 적용하지 않음 (기본 모양만 표시)
            Vector2Int holdPos = centerOffset + shapePos;

            // 4x4 범위 안에 있는지 확인
            if (holdPos.x >= 0 && holdPos.x < HOLD_SIZE && holdPos.y >= 0 && holdPos.y < HOLD_SIZE)
            {
                var holdImage = _holdImages[holdPos.x, holdPos.y];
                if (holdImage != null)
                {
                    holdImage.SetActive(true);
                    var image = holdImage.GetComponent<Image>();
                    if (image != null)
                        image.color = tetriminoColor;
                }
            }
        }
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
    /// 현재 테트리미노와 홀드된 테트리미노 정보를 로그로 출력
    /// </summary>
    private void LogCurrentAndHoldTetrimino()
    {
        if (Context == null) return;

        System.Text.StringBuilder logBuilder = new System.Text.StringBuilder();
        logBuilder.AppendLine("=== 테트리미노 상태 정보 ===");

        // 현재 테트리미노 정보
        var currentEntities = Context.GetEntitiesWithComponent<CurrentTetriminoComponent>();
        if (currentEntities != null && currentEntities.Count > 0)
        {
            var currentEntity = currentEntities[0];
            var currentTetrimino = currentEntity.GetComponent<TetriminoComponent>();
            var currentPosition = currentEntity.GetComponent<CurrentTetriminoComponent>();

            if (currentTetrimino != null && currentPosition != null)
            {
                logBuilder.AppendLine($"[현재 테트리미노]");
                logBuilder.AppendLine($"  - 타입: {currentTetrimino.Type}");
                logBuilder.AppendLine($"  - 색상: {currentTetrimino.Color}");
                logBuilder.AppendLine($"  - 위치: ({currentPosition.Position.x}, {currentPosition.Position.y})");
                logBuilder.AppendLine($"  - 회전: {currentTetrimino.Rotation}");
                logBuilder.AppendLine($"  - 엔티티 ID: {currentEntity.ID}");
                
                // 현재 테트리미노의 실제 블록 위치들
                logBuilder.AppendLine($"  - 블록 위치들:");
                if (currentTetrimino.Shape != null)
                {
                    for (int i = 0; i < currentTetrimino.Shape.Length; i++)
                    {
                        Vector2Int rotatedPos = RotatePoint(currentTetrimino.Shape[i], currentTetrimino.Rotation);
                        Vector2Int worldPos = currentPosition.Position + rotatedPos;
                        logBuilder.AppendLine($"    [{i}] Local({currentTetrimino.Shape[i].x}, {currentTetrimino.Shape[i].y}) → World({worldPos.x}, {worldPos.y})");
                    }
                }
            }
        }
        else
        {
            logBuilder.AppendLine("[현재 테트리미노] 없음");
        }

        // 홀드된 테트리미노 정보
        var holdEntities = Context.GetEntitiesWithComponent<HoldTetriminoComponent>();
        if (holdEntities != null && holdEntities.Count > 0)
        {
            var holdEntity = holdEntities[0];
            var holdTetrimino = holdEntity.GetComponent<TetriminoComponent>();

            if (holdTetrimino != null)
            {
                logBuilder.AppendLine($"[홀드 테트리미노]");
                logBuilder.AppendLine($"  - 타입: {holdTetrimino.Type}");
                logBuilder.AppendLine($"  - 색상: {holdTetrimino.Color}");
                logBuilder.AppendLine($"  - 엔티티 ID: {holdEntity.ID}");
                logBuilder.AppendLine($"  - 기본 형태:");
                
                if (holdTetrimino.Shape != null)
                {
                    for (int i = 0; i < holdTetrimino.Shape.Length; i++)
                    {
                        logBuilder.AppendLine($"    [{i}] ({holdTetrimino.Shape[i].x}, {holdTetrimino.Shape[i].y})");
                    }
                }
            }
        }
        else
        {
            logBuilder.AppendLine("[홀드 테트리미노] 없음");
        }

        logBuilder.AppendLine("================================");
        Debug.Log(logBuilder.ToString());
    }
}