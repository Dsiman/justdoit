using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

public class Storeable : Selectable
{
    public int GridRows = 4;
    public int GridColumns = 4;
    
    // Grid storage and item tracking
    private StoredItem[,] inventoryGrid;
    private List<StoredItem> storedItems = new List<StoredItem>();

    protected override void OnAwake()
    {
        base.OnAwake();
        InitializeInventory();
    }

    private void InitializeInventory()
    {
        // Ensure valid grid size
        GridRows = Math.Max(1, GridRows);
        GridColumns = Math.Max(1, GridColumns);
        
        inventoryGrid = new StoredItem[GridRows, GridColumns];
    }

    /// <summary>
    /// Try to add an item to the inventory at specified position with rotation
    /// </summary>
    public bool TryAddItem(StoredItem item, int row, int column, int rotation = 0)
    {
        // Validate position and item fit
        if (!CanPlaceItem(item, row, column, rotation))
            return false;

        // Place item in grid
        PlaceItem(item, row, column, rotation);
        storedItems.Add(item);
        
        // Store placement information
        item.Position = new IntVector2(row, column);
        item.Rotation = rotation;
        
        // Disable physical representation
        if (item.itemToStore != null)
            item.itemToStore.Enabled = false;
        
        return true;
    }

    /// <summary>
    /// Automatically find space for an item with rotation
    /// </summary>
    public bool AutoAddItem(StoredItem item)
    {
        // Try all rotations (0°, 90°, 180°, 270°)
        for (int rotation = 0; rotation < 360; rotation += 90)
        {
            for (int row = 0; row < GridRows; row++)
            {
                for (int col = 0; col < GridColumns; col++)
                {
                    if (CanPlaceItem(item, row, col, rotation))
                    {
                        PlaceItem(item, row, col, rotation);
                        storedItems.Add(item);
                        
                        item.Position = new IntVector2(row, col);
                        item.Rotation = rotation;
                        
                        if (item.itemToStore != null)
                            item.itemToStore.Enabled = false;
                        
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Remove item from inventory
    /// </summary>
    public void RemoveItem(StoredItem item)
    {
        if (!storedItems.Contains(item)) return;

        // Clear grid references using stored position
        var grid = GetRotatedGrid(item.itemShape.Grid, item.Rotation);
        int itemRows = grid.GetLength(0);
        int itemCols = grid.GetLength(1);

        for (int r = 0; r < itemRows; r++)
        {
            for (int c = 0; c < itemCols; c++)
            {
                if (grid[r, c])
                {
                    int gridRow = item.Position.X + r;
                    int gridCol = item.Position.Y + c;
                    
                    if (gridRow < GridRows && gridCol < GridColumns)
                        inventoryGrid[gridRow, gridCol] = null;
                }
            }
        }

        storedItems.Remove(item);
        
        // Re-enable physical object
        if (item.itemToStore != null)
            item.itemToStore.Enabled = true;
    }

    private bool CanPlaceItem(StoredItem item, int startRow, int startCol, int rotation)
    {
        var grid = GetRotatedGrid(item.itemShape.Grid, rotation);
        int itemRows = grid.GetLength(0);
        int itemCols = grid.GetLength(1);

        // Check boundaries
        if (startRow < 0 || startCol < 0 || 
            startRow + itemRows > GridRows || 
            startCol + itemCols > GridColumns)
            return false;

        // Check collision
        for (int r = 0; r < itemRows; r++)
        {
            for (int c = 0; c < itemCols; c++)
            {
                if (grid[r, c])
                {
                    if (inventoryGrid[startRow + r, startCol + c] != null)
                        return false;
                }
            }
        }
        
        return true;
    }

    private void PlaceItem(StoredItem item, int startRow, int startCol, int rotation)
    {
        var grid = GetRotatedGrid(item.itemShape.Grid, rotation);
        int itemRows = grid.GetLength(0);
        int itemCols = grid.GetLength(1);

        for (int r = 0; r < itemRows; r++)
        {
            for (int c = 0; c < itemCols; c++)
            {
                if (grid[r, c])
                {
                    inventoryGrid[startRow + r, startCol + c] = item;
                }
            }
        }
    }

    // Helper method to rotate grid
    public bool[,] GetRotatedGrid(bool[,] original, int rotation)
    {
        int rows = original.GetLength(0);
        int cols = original.GetLength(1);
        bool[,] result = original;

        // Apply rotation (90° increments)
        for (int i = 0; i < rotation / 90; i++)
        {
            result = Rotate90Clockwise(result);
        }
        return result;
    }

    private bool[,] Rotate90Clockwise(bool[,] src)
    {
        int width = src.GetLength(0);
        int height = src.GetLength(1);
        bool[,] dst = new bool[height, width];
        
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                dst[row, col] = src[width - col - 1, row];
            }
        }
        return dst;
    }

    // UI Helper Methods
    public IEnumerable<StoredItem> GetItems() => storedItems;
    public StoredItem GetItemAt(int row, int col) => inventoryGrid[row, col];
    public (int rows, int cols) GetGridSize() => (GridRows, GridColumns);
    
    // Debug visualization
    protected override void OnUpdate()
    {
        base.OnUpdate();
        
        if (_isSelected)
        {
            DrawDebugGrid();
        }
    }
    
    private void DrawDebugGrid()
    {
        var gridPos = WorldPosition;
        
        // Draw grid lines
        for (int r = 0; r <= GridRows; r++)
        {
            DebugOverlay.Line(
                gridPos + new Vector3(0, r, 0),
                gridPos + new Vector3(GridColumns, r, 0),
                Color.White
            );
        }
        
        for (int c = 0; c <= GridColumns; c++)
        {
            DebugOverlay.Line(
                gridPos + new Vector3(c, 0, 0),
                gridPos + new Vector3(c, GridRows, 0),
                Color.White
            );
        }
        
        // Draw items
        foreach (var item in storedItems)
        {
            var grid = GetRotatedGrid(item.itemShape.Grid, item.Rotation);
            for (int r = 0; r < grid.GetLength(0); r++)
            {
                for (int c = 0; c < grid.GetLength(1); c++)
                {
                    if (grid[r, c])
                    {
                        DebugOverlay.Sphere(
                            new Sphere(gridPos + new Vector3(item.Position.Y + c + 0.5f, item.Position.X + r + 0.5f, 0), 0.3f),
                            Color.Green
                        );
                    }
                }
            }
        }
    }
}