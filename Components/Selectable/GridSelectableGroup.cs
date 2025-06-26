using UnityEngine;
using Ham.GameControl.Player;
using System;

namespace Ham.Components.Selectable
{
public class GridSelectableGroup : MonoBehaviour
{
    [Header("Grid Setup")]
    public int width = 3;
    public int height = 3;

    public Selectable[,] grid; // Assigned externally
    private int currentX = 0;
    private int currentY = 0;

    void Start()
    {
        Focus(currentX, currentY);
    }

    public void MoveFocus(Vector2 dir)
    {
        int dx = (int)Mathf.Sign(dir.x);
        int dy = (int)Mathf.Sign(dir.y);

        int newX = Mathf.Clamp(currentX + dx, 0, width - 1);
        int newY = Mathf.Clamp(currentY + dy, 0, height - 1);

        if (grid[newX, newY] != null)
            Focus(newX, newY);
    }

    public void Confirm(PlayerController player)
    {
        grid[currentX, currentY]?.Select(player);
    }

    public void Focus(int x, int y)
    {
        currentX = x;
        currentY = y;
    }

    public void SetGrid(Selectable[,] newGrid)
    {
        this.grid = newGrid;
        this.width = newGrid.GetLength(0);
        this.height = newGrid.GetLength(1);
    }
}
}

