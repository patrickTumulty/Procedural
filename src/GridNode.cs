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
    }
}

