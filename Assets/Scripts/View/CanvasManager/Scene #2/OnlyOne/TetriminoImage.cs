using UnityEngine;
using UnityEngine.UI;
using OVFL.ECS;
using Minomino;
using TMPro;
using System.Collections.Generic;

public class TetriminoImage : MonoBehaviour
{
    [Header("테트리미노 미리보기 그리드 부모 오브젝트")]
    [SerializeField] private Transform gridParent;

    private GameObject[,] _anchors;
    private GameObject[,] _images;
    private const int GRID_SIZE = 5;
    private bool _isInitialized = false;

    private void Awake()
    {
        Init();
    }

    /// <summary>
    /// 5x5 그리드 초기화 (중복 방지)
    /// </summary>
    public void Init()
    {
        if (_isInitialized) return;
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
    /// 테트리미노 컴포넌트로부터 shape와 color를 받아 미리보기 이미지를 갱신
    /// </summary>
    public void UpdateImage(TetriminoComponent tetrimino)
    {
        if (!_isInitialized) Init();
        ClearDisplay();
        
        if (tetrimino == null || tetrimino.Shape == null || _images == null) 
        {
            Debug.LogWarning("TetriminoImage.UpdateImage: tetrimino, tetrimino.Shape 또는 _images가 null입니다.");
            return;
        }
        
        Vector2Int centerOffset = new Vector2Int(2, 2);
        Color color = GetTetriminoColorStatic(tetrimino.Color);
        
        foreach (var shapePos in tetrimino.Shape)
        {
            Vector2Int gridPos = centerOffset + shapePos;
            if (IsValidGridPosition(gridPos))
            {
                var go = _images[gridPos.x, gridPos.y];
                if (go != null)
                {
                    go.SetActive(true);
                    var img = go.GetComponent<UnityEngine.UI.Image>();
                    if (img != null) img.color = color;
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
    /// 테트리미노 색상 enum을 UnityEngine.Color로 변환
    /// </summary>
    public static Color GetTetriminoColorStatic(TetriminoColor tetriminoColor)
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
    /// 5x5 그리드 내 유효한 위치인지 검사
    /// </summary>
    private bool IsValidGridPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < GRID_SIZE && pos.y >= 0 && pos.y < GRID_SIZE;
    }
}