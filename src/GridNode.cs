
namespace Grid
{
    public class GridNode
    {
        public int X { get; }
        public int Y { get; }
        public GridNode?[] AdjascentNodes { get; }

        public GridNode(int x, int y)
        {
            X = x;
            Y = y;
            AdjascentNodes = new GridNode?[] { null, null, null, null, };
        }

        public string GetAdjascentNodesString()
        {
            return $"[U{AdjascentNodes[0],8}, L{AdjascentNodes[1],8}, D{AdjascentNodes[2],8}, R{AdjascentNodes[3],8}]";
        }

        public override string ToString()
        {
            return $"({X,2}, {Y,2})";
        }

        public bool GreaterThan(Direction direction, GridNode node)
        {
            return direction switch
            {
                Direction.Up => Y < node.Y,
                Direction.Left => X < node.X,
                Direction.Down => Y > node.Y,
                Direction.Right => X > node.X,
                _ => false,
            };
        }

        public bool LessThan(Direction direction, GridNode node)
        {
            return direction switch
            {
                Direction.Up => Y > node.Y,
                Direction.Left => X > node.X,
                Direction.Down => Y < node.Y,
                Direction.Right => X < node.X,
                _ => false,
            };
        }

        public bool CoordinatesMatch(GridNode node)
        {
            return X == node.X && Y == node.Y;
        }
    }
}

