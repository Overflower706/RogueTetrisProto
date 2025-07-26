using UnityEngine;
using UnityEngine.UI;
using OVFL.ECS;
using Minomino;
using System.Collections.Generic;
using TMPro;

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



    [Header("디버그용 UI")]
    [SerializeField] TMP_Text DebugText;


    private int _width;
    private int _height;

    // (x, y) => gridCell 오브젝트
    private GameObject[,] _gridCells;

    // (x, y) => 블록 오브젝트
    private Dictionary<Vector2Int, GameObject> _tetriminoObjects = new();

    public void Init(Context context)
    {
        Context = context;
        _isInit = true;
        InitGrid();
    }

    private void InitGrid()
    {
        _width = GlobalSettings.Instance.SafeWidth;
        _height = GlobalSettings.Instance.BoardHeight;

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
    /// 보드 상태를 받아 시각 UI를 갱신 (배치/삭제/라인 이동 시 호출)
    /// </summary>
    public void SetBoard(BoardComponent board)
    {
        if (board == null || board.Board == null) return;

        var toRemove = new List<Vector2Int>();
        var (currentTetrimino, currentEntityId) = GetCurrentTetriminoInfo();

        // 기존 블록 UI 처리
        for (int y = 0; y < GlobalSettings.Instance.BoardHeight; y++)
        {
            for (int x = 0; x < GlobalSettings.Instance.SafeWidth; x++)
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
        DebugText.text = BoardLogger.VisualizeBoard(Context, board);
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

    /// <summary>
    /// 현재 활성화된 있는 테트리미노가 하드롭 했을 때의 예상 위치(월드 좌표 배열)를 반환
    /// </summary>
    private Vector2Int[] GetHardDropGhostPositions(BoardComponent board)
    {
        // 1. 현재 테트리미노 엔티티 찾기
        Entity currentEntity = null;
        var boardTetriminoEntities = Context.GetEntitiesWithComponent<BoardTetrominoComponent>();
        foreach (var boardTetriminoEntity in boardTetriminoEntities)
        {
            var boardTetriminoComponent = boardTetriminoEntity.GetComponent<BoardTetrominoComponent>();
            if (boardTetriminoComponent.State == BoardTetrominoState.Current)
            {
                currentEntity = boardTetriminoEntity;
                break;
            }
        }

        if (currentEntity == null)
        {
            Debug.LogWarning("현재 테트리미노 엔티티를 찾을 수 없습니다.");
            return null;
        }

        var tetrimino = currentEntity.GetComponent<TetrominoComponent>();
        var boardTetrimino = currentEntity.GetComponent<BoardTetrominoComponent>();
        if (tetrimino == null || boardTetrimino == null)
            return null;

        // 2. 회전 적용된 shape 구하기
        Vector2Int[] rotatedShape = new Vector2Int[tetrimino.Shape.Length];
        for (int i = 0; i < tetrimino.Shape.Length; i++)
            rotatedShape[i] = RotatePoint(tetrimino.Shape[i], boardTetrimino.Rotation);

        int entityId = currentEntity.ID;

        // 3. 하드롭 위치 계산 (자기 자신은 무시)
        Vector2Int dropPosition = boardTetrimino.Position;
        while (true)
        {
            Vector2Int testPosition = dropPosition + Vector2Int.down;
            bool collision = false;
            foreach (var local in rotatedShape)
            {
                Vector2Int world = testPosition + local;
                if (world.x < 0 || world.x >= GlobalSettings.Instance.SafeWidth || world.y < 0)
                {
                    collision = true;
                    break;
                }
                // 자기 자신은 무시
                if (world.y < GlobalSettings.Instance.BoardHeight)
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
    /// 현재 테트리미노 정보를 가져오는 헬퍼 메서드
    /// </summary>
    private (TetrominoComponent tetrimino, int entityId) GetCurrentTetriminoInfo()
    {
        var currentEntities = Context.GetEntitiesWithComponent<BoardTetrominoComponent>();
        if (currentEntities != null && currentEntities.Count > 0)
        {
            var entity = currentEntities[0];
            return (entity.GetComponent<TetrominoComponent>(), entity.ID);
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
    private void CreateOrUpdateTetriminoObject(Vector2Int pos, int tetriminoId, TetrominoComponent currentTetrimino, int currentEntityId)
    {
        GameObject tetriminoObj = GetOrCreateTetriminoObject(pos);
        var image = tetriminoObj.GetComponent<Image>();
        // 이미지 설정
        Sprite sprite = GetMinoSpriteById(tetriminoId);
        if (sprite != null)
        {
            image.sprite = sprite;
            image.color = Color.white; // 기본 색상으로 설정
        }
        else
        {
            // 스프라이트가 없으면 기본 색상 사용
            image.sprite = null;
            image.color = Color.white;
        }
        tetriminoObj.SetActive(true);
    }    /// <summary>
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
    /// 미노 ID로 스프라이트를 가져오는 헬퍼 메서드
    /// </summary>
    private Sprite GetMinoSpriteById(int minoId)
    {
        var entity = FindEntityById(minoId);
        var sprites = SpriteSettings.Instance.Sprites_Living;
        if (entity == null)
        {
            Debug.LogWarning($"[GetMinoSpriteById] minoId({minoId})에 해당하는 엔티티를 찾을 수 없습니다.");
            return null;
        }
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning("[GetMinoSpriteById] GlobalSettings.Instance.tetriminoSprites에 스프라이트가 없습니다.");
            return null;
        }

        var minoComponent = entity.GetComponent<MinoComponent>();
        if (minoComponent == null)
        {
            Debug.LogWarning($"[GetMinoSpriteById] entityId({entity.ID})에 MinoComponent가 없습니다.");
            return sprites[0];
        }

        int spriteIndex = Mathf.Clamp((int)minoComponent.State, 0, sprites.Length - 1);
        return sprites[spriteIndex];
    }

    /// <summary>
    /// ID로 엔티티를 찾는 헬퍼 메서드
    /// </summary>
    private Entity FindEntityById(int entityId)
    {
        var entities = Context.GetEntities();
        foreach (var entity in entities)
        {
            if (entity.ID == entityId)
            {
                return entity;
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

    /// <summary>
    /// 게임 종료 시 보드를 청소하고 다음 게임을 위해 초기화
    /// </summary>
    public void Clear()
    {
        _isInit = false;
        Context = null;

        // 기존 테트리미노 오브젝트들 모두 제거
        foreach (var kvp in _tetriminoObjects)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value);
            }
        }
        _tetriminoObjects.Clear();

        // 그리드 셀들 제거
        if (_gridCells != null)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (_gridCells[x, y] != null)
                    {
                        Destroy(_gridCells[x, y]);
                    }
                }
            }
            _gridCells = null;
        }

        // GridParent 하위의 모든 자식 오브젝트 제거 (혹시 놓친 것들)
        if (GridParent != null)
        {
            foreach (Transform child in GridParent)
            {
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        // TetriminoParent 하위의 모든 자식 오브젝트 제거
        if (TetriminoParent != null)
        {
            foreach (Transform child in TetriminoParent)
            {
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        // 크기 변수 초기화
        _width = 0;
        _height = 0;

        Debug.Log("GameBoardPanel 청소 완료 - 다음 게임을 위해 준비됨");
    }
}