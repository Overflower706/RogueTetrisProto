using UnityEngine;
using UnityEngine.UI;
using OVFL.ECS;
using Minomino;
using System.Collections.Generic;

public class GameBoardPanel : MonoBehaviour
{
    [Header("격자(배경) 프리팹")]
    [SerializeField] private GameObject GridCellPrefab;

    [Header("블록(동적) 프리팹")]
    [SerializeField] private GameObject BlockPrefab;

    [Header("부모 오브젝트")]
    [SerializeField] private RectTransform GridParent;
    [SerializeField] private Transform BlockParent;

    private int _width;
    private int _height;

    // (x, y) => gridCell 오브젝트
    private GameObject[,] _gridCells;

    // (x, y) => 블록 오브젝트
    private Dictionary<Vector2Int, GameObject> _blockObjects = new();

    public void InitGrid()
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

        // 1. 전체 좌표 순회
        for (int y = 0; y < BoardComponent.HEIGHT; y++)
        {
            for (int x = 0; x < BoardComponent.WIDTH; x++)
            {
                int blockId = board.Board[x, y];
                var pos = new Vector2Int(x, y);

                if (blockId == 0)
                {
                    // 블록이 있으면 삭제
                    if (_blockObjects.TryGetValue(pos, out var obj) && obj != null)
                    {
                        Destroy(obj);
                        toRemove.Add(pos);
                    }
                }
                else // 1~4: 블록 표시
                {
                    GameObject blockObj;
                    if (!_blockObjects.TryGetValue(pos, out blockObj) || blockObj == null)
                    {
                        // 새 블록 생성
                        blockObj = Instantiate(BlockPrefab, BlockParent);
                        // gridCell의 월드 좌표를 BlockParent의 localPosition으로 변환
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
                            // BlockParent가 RectTransform이 아니면 fallback
                            blockRect.position = worldPos;
                        }
                        _blockObjects[pos] = blockObj;
                    }
                    // 색상 갱신
                    var image = blockObj.GetComponent<Image>();
                    image.color = GetBlockColor(blockId);
                    blockObj.SetActive(true);
                }
            }
        }

        // 2. 삭제된 블록 딕셔너리에서 제거
        foreach (var pos in toRemove)
            _blockObjects.Remove(pos);
    }

    // blockId 1~4에 맞는 색상만 반환
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