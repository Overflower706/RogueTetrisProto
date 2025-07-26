using UnityEngine;
using Minomino;
using OVFL.ECS;

public class TetriminoImage : MonoBehaviour
{
    [Header("테트리미노 미리보기 그리드 부모 오브젝트")]
    [SerializeField] private Transform gridParent;

    public Context Context { get; set; }

    private GameObject[,] _anchors;
    private GameObject[,] _images;
    private const int GRID_SIZE = 5;
    private bool _isInitialized = false;


    /// <summary>
    /// 5x5 그리드 초기화 (중복 방지)
    /// </summary>
    public void Init(Context context)
    {
        if (_isInitialized) return;

        Context = context;
        _anchors = new GameObject[GRID_SIZE, GRID_SIZE];
        _images = new GameObject[GRID_SIZE, GRID_SIZE];

        int index = 0;
        foreach (Transform anchorTransform in gridParent)
        {
            int x = index % GRID_SIZE;
            int y = index / GRID_SIZE;
            GameObject anchor = anchorTransform.gameObject;
            _anchors[x, y] = anchor;
            if (anchorTransform.childCount > 0)
            {
                GameObject image = anchorTransform.GetChild(0).gameObject;
                _images[x, y] = image;
                image.SetActive(false);
            }
            index++;
        }
        _isInitialized = true;
    }

    /// <summary>
    /// 테트리미노 컴포넌트로부터 mino ID들을 받아 MinoComponent의 state에 맞는 스프라이트로 미리보기 이미지를 갱신
    /// </summary>
    public void UpdateImage(TetrominoComponent tetrimino)
    {
        if (!_isInitialized) return;
        ClearDisplay();

        if (tetrimino == null || tetrimino.Shape == null || _images == null || Context == null)
        {
            Debug.LogWarning("TetriminoImage.UpdateImage: tetrimino, tetrimino.Shape, _images 또는 Context가 null입니다.");
            return;
        }

        if (tetrimino.Minos == null || tetrimino.Minos.Length == 0)
        {
            Debug.LogWarning("TetriminoImage.UpdateImage: tetrimino.Minos가 null이거나 비어있습니다.");
            return;
        }

        Vector2Int centerOffset = new Vector2Int(2, 2);

        for (int i = 0; i < tetrimino.Shape.Length && i < tetrimino.Minos.Length; i++)
        {
            Vector2Int shapePos = tetrimino.Shape[i];
            int minoID = tetrimino.Minos[i];

            Vector2Int gridPos = centerOffset + shapePos;
            if (IsValidGridPosition(gridPos))
            {
                var go = _images[gridPos.x, gridPos.y];
                if (go != null)
                {
                    go.SetActive(true);
                    var img = go.GetComponent<UnityEngine.UI.Image>();
                    if (img != null)
                    {
                        Sprite sprite = GetMinoSpriteFromID(minoID);
                        if (sprite != null)
                        {
                            img.sprite = sprite;
                            img.color = Color.white; // 기본 색상으로 설정
                        }
                        else
                        {
                            // 스프라이트가 없으면 기본 처리
                            img.sprite = null;
                            img.color = Color.white;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 모든 미리보기 이미지를 비활성화
    /// </summary>
    public void ClearDisplay()
    {
        if (!_isInitialized) return;
        for (int y = 0; y < GRID_SIZE; y++)
            for (int x = 0; x < GRID_SIZE; x++)
                _images[x, y]?.SetActive(false);
    }

    /// <summary>
    /// Mino ID로부터 MinoComponent를 찾아 state에 맞는 스프라이트를 반환
    /// </summary>
    private Sprite GetMinoSpriteFromID(int minoID)
    {
        if (Context == null) return null;

        // Context에서 해당 ID의 엔티티를 찾기
        var entities = Context.GetEntities();
        if (entities == null || minoID >= entities.Count || minoID < 0) return null;

        var entity = entities[minoID];
        if (entity == null) return null;

        var minoComponent = entity.GetComponent<MinoComponent>();
        if (minoComponent == null) return null;

        // MinoComponent의 state에 맞는 스프라이트 반환
        return GetSpriteByMinoState(minoComponent.State);
    }

    /// <summary>
    /// MinoComponent의 state에 맞는 스프라이트를 GlobalSettings에서 가져오기
    /// </summary>
    private Sprite GetSpriteByMinoState(MinoState state)
    {
        switch (state)
        {
            case MinoState.Living:
                return SpriteSettings.Instance.Sprites_Living[0];
            case MinoState.Empty:
                return SpriteSettings.Instance.Sprites_Empty[0];
            case MinoState.None:
                Debug.LogWarning("MinoState is None, returning default sprite.");
                return SpriteSettings.Instance.Sprites_Empty[0];
            default:
                Debug.LogWarning($"Unknown MinoState: {state}, returning default Living sprite.");
                return SpriteSettings.Instance.Sprites_Living[0];
        }
    }

    /// <summary>
    /// 5x5 그리드 내 유효한 위치인지 검사
    /// </summary>
    private bool IsValidGridPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < GRID_SIZE && pos.y >= 0 && pos.y < GRID_SIZE;
    }
}