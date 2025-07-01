using Sandbox;

public enum ItemShapeType
{
    Square_1x1,
    Square_2x2,
    Square_3x3,
    Cross,
    CaneL,
    CaneR,
    Custom
}

public class ItemShape
{
    public ItemShapeType ShapeType { get; set; } = ItemShapeType.Square_1x1;

    // 2D grid footprint: true = occupied, false = empty
    public bool[,] Grid { get; set; }

    public ItemShape(ItemShapeType type)
    {
        ShapeType = type;
        Grid = GenerateShape(type);
    }

    private bool[,] GenerateShape(ItemShapeType type)
    {
        return type switch
        {
            ItemShapeType.Square_1x1 => new bool[,] { { true } },
            ItemShapeType.Square_2x2 => new bool[,] { { true, true }, { true, true } },
            ItemShapeType.Square_3x3 => new bool[,] { { true, true, true }, { true, true, true }, { true, true, true } },
            ItemShapeType.Cross => new bool[,] {
                { false, true, false },
                { true,  true, true  },
                { false, true, false }
            },
            ItemShapeType.CaneL => new bool[,] {
                { true, false },
                { true, false },
                { true, true  }
            },
            ItemShapeType.CaneR => new bool[,] {
                { false, true },
                { false, true },
                { true,  true }
            },
            _ => new bool[,] { { true } }
        };
    }
}

// Helper struct for grid positions
public struct IntVector2
{
    public int X;
    public int Y;
    
    public IntVector2(int x, int y)
    {
        X = x;
        Y = y;
    }
}