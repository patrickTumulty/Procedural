
namespace Procedural
{
    public enum Axis
    {
        X,
        Y
    }

    public class AxisUtils
    {
        public static Axis InvertAxis(Axis axis)
        {
            return axis == Axis.X ? Axis.Y : Axis.X;
        }
    }

    public enum Direction : byte
    {
        Up = 0,
        Left = 1,
        Down = 2,
        Right = 3,
    }

    public class DirectionUtils
    {
        public static bool IsVertical(Direction direction)
        {
            return direction is Direction.Up or Direction.Down;
        }

        public static Axis GetAxis(Direction direction)
        {
            return IsVertical(direction) ? Axis.Y : Axis.X;
        }

        public static Direction InvertDirection(Direction direction)
        {
            return (Direction)InvertDirection((byte)direction);
        }

        public static byte InvertDirection(byte direction)
        {
            return (byte)((direction + 2) % 4);
        }
    }

    public class VertexNode
    {
        private static readonly Comparison<VertexNode> yAxisComparison = (a, b) => a.Y.CompareTo(b.Y);
        private static readonly Comparison<VertexNode> xAxisComparison = (a, b) => a.X.CompareTo(b.X);

        public int X { get; }
        public int Y { get; }
        public VertexNode?[] AdjascentNodes { get; }

        public VertexNode(int x, int y)
        {
            X = x;
            Y = y;
            AdjascentNodes = new VertexNode?[] { null, null, null, null, };
        }

        public static void ConnectAdjascentNode(Direction direction, VertexNode node, VertexNode connectingNode)
        {
            ConnectAdjascentNode((byte)direction, node, connectingNode);
        }

        public static void ConnectAdjascentNode(byte direction, VertexNode node, VertexNode connectingNode)
        {
            node.AdjascentNodes[direction] = connectingNode;

            byte invertedDirection = DirectionUtils.InvertDirection(direction);
            VertexNode? reverseAdjascentNode = connectingNode.AdjascentNodes[invertedDirection];
            if (reverseAdjascentNode == null || reverseAdjascentNode != node)
            {
                ConnectAdjascentNode(invertedDirection, connectingNode, node);
            }
        }

        public static List<VertexNode> GetNodesAlongAxisLine(VertexNode root, Axis axis, int axisValue)
        {
            return CollectVertexNodes((node) => axis == Axis.X ? node.X == axisValue : node.Y == axisValue, root);
        }

        public static bool ConnectionExists(Direction direction, VertexNode v1, VertexNode v2)
        {
            return v1.AdjascentNodes[(byte)direction] == v2
                && v2.AdjascentNodes[DirectionUtils.InvertDirection((byte)direction)] == v1;
        }

        public static void InsertNode(Direction direction, VertexNode root, VertexNode newNode)
        {
            bool isVertical = DirectionUtils.IsVertical(direction);
            byte directionByte = (byte)direction;
            VertexNode? existingNode = root.AdjascentNodes[directionByte];
            if (existingNode == null)
            {
                int axisX = newNode.X;
                int axisY = newNode.Y;
                Axis axis = DirectionUtils.GetAxis(direction);

                List<VertexNode> axisList = GetNodesAlongAxisLine(root, axis, axis == Axis.X ? axisX : axisY);
                axisList.Sort(axis == Axis.X ? xAxisComparison : yAxisComparison);

                if (axisList.Count >= 2 && !axisList.Contains(newNode))
                {
                    VertexNode lower = axisList[0];
                    InsertNode(axis == Axis.Y ? Direction.Right : Direction.Down, lower, newNode);
                }

                ConnectAdjascentNode(direction, root, newNode);
                return;
            }

            int coordinateExisting = isVertical ? existingNode.Y : existingNode.X;
            int coordinateNew = isVertical ? newNode.Y : newNode.X;

            if (coordinateNew > coordinateExisting)
            {
                InsertNode(direction, newNode, existingNode);
            }
            else if (coordinateNew < coordinateExisting)
            {
                // Inserting node on a given axis
                root.AdjascentNodes[directionByte] = newNode;
                ConnectAdjascentNode(direction, root, newNode);
                ConnectAdjascentNode(DirectionUtils.InvertDirection(direction), existingNode, newNode);
            }
        }


        public static List<VertexNode> CollectVertexNodes(Predicate<VertexNode> predicate, VertexNode? root)
        {
            List<VertexNode> nodeList = new();
            HashSet<int> visited = new();

            CollectVertexNodes(nodeList, visited, predicate, root);

            return nodeList;
        }

        private static void CollectVertexNodes(List<VertexNode> nodeList,
                                               HashSet<int> visited,
                                               Predicate<VertexNode> predicate,
                                               VertexNode? node)
        {
            if (node == null)
            {
                return;
            }

            int hash = node.GetHashCode();
            if (visited.Contains(hash))
            {
                return;
            }

            _ = visited.Add(hash);

            if (predicate(node) && !nodeList.Contains(node))
            {
                nodeList.Add(node);
            }

            for (int i = 0; i < node.AdjascentNodes.Length; i++)
            {
                CollectVertexNodes(nodeList, visited, predicate, node.AdjascentNodes[i]);
            }
        }

        public static bool IsConnected(VertexNode root, VertexNode node)
        {
            foreach (VertexNode? n in root.AdjascentNodes)
            {
                if (n != null && n == node)
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            return $"(X={X}, Y={Y})";
        }
    }

    public class LayoutDescriptor
    {
        public VertexNode RootVertexNode { get; }
        public int AreaWidth { get; }
        public int AreaHeight { get; }

        public LayoutDescriptor(VertexNode vertexNode, int width, int height)
        {
            RootVertexNode = vertexNode;
            AreaHeight = height;
            AreaWidth = width;
        }
    }

    public class LayoutGenerator
    {
        public static LayoutDescriptor Generate()
        {
            int width = 20;
            int height = 10;

            Console.WriteLine($"[{width}x{height}]");

            VertexNode root = new(0, 0);

            AddRectangle(root, width, height);
            AddRectangle(root, 10, 10);
            AddRectangle(root, 4, 4);
            // AddRectangle(root, 15, 5);

            // VertexNode topLeft = new(2, 0);
            // VertexNode.InsertNode(Direction.Right, root, topLeft);
            //
            // VertexNode bottomRight = new(2, 2);
            // VertexNode.InsertNode(Direction.Down, topLeft, bottomRight);
            // //
            // VertexNode bottomLeft = new(0, 2);
            // VertexNode.InsertNode(Direction.Left, bottomRight, bottomLeft);

            // VertexNode.ConnectAdjascentNode(Direction.Up, bottomLeft, root);

            // PrintGraph(new HashSet<int>(), root);

            return new LayoutDescriptor(root, width, height);
        }

        private static void PrintGraph(HashSet<int> visited, VertexNode? root)
        {
            if (root == null || visited.Contains(root.GetHashCode()))
            {
                return;
            }

            _ = visited.Add(root.GetHashCode());

            Console.WriteLine(root);

            foreach (VertexNode? n in root.AdjascentNodes)
            {
                PrintGraph(visited, n);
            }
        }

        private static void AddRectangle(VertexNode root, int width, int height)
        {
            VertexNode topRight = new(root.X + (width - 1), root.Y);
            VertexNode.InsertNode(Direction.Right, root, topRight);

            VertexNode bottomRight = new(topRight.X, topRight.Y + (height - 1));
            VertexNode.InsertNode(Direction.Down, topRight, bottomRight);

            VertexNode bottomLeft = new(bottomRight.X - (width - 1), bottomRight.Y);
            VertexNode.InsertNode(Direction.Left, bottomRight, bottomLeft);

            VertexNode.InsertNode(Direction.Up, bottomLeft, root);
        }
    }
}


