using UnityEngine;

[System.Serializable]
public class TetrisBoard
{
    public const int WIDTH = 10;
    public const int HEIGHT = 20;

    public int[,] grid; // 0 = 빈 공간, 1 = 채워진 블록

    public TetrisBoard()
    {
        grid = new int[WIDTH, HEIGHT];
        Clear();
    }

    public void Clear()
    {
        for (int x = 0; x < WIDTH; x++)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                grid[x, y] = 0;
            }
        }
    }

    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < WIDTH &&
               position.y >= 0 && position.y < HEIGHT &&
               grid[position.x, position.y] == 0;
    }

    public void PlaceBlock(Vector2Int position, int blockType)
    {
        if (IsValidPosition(position))
        {
            grid[position.x, position.y] = blockType;
        }
    }

    public bool IsLineFull(int y)
    {
        for (int x = 0; x < WIDTH; x++)
        {
            if (grid[x, y] == 0) return false;
        }
        return true;
    }

    public void ClearLine(int y)
    {
        // 해당 라인을 지우고 위쪽 블록들을 아래로 이동
        for (int moveY = y; moveY < HEIGHT - 1; moveY++)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                grid[x, moveY] = grid[x, moveY + 1];
            }
        }

        // 맨 위 라인 초기화
        for (int x = 0; x < WIDTH; x++)
        {
            grid[x, HEIGHT - 1] = 0;
        }
    }

    public int GetBlock(int x, int y)
    {
        if (x >= 0 && x < WIDTH && y >= 0 && y < HEIGHT)
        {
            return grid[x, y];
        }
        return -1; // 범위 밖은 -1 반환
    }
}
