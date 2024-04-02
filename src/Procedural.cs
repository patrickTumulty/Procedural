
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

    public readonly struct InsertionPoint
    {
        public readonly VertexNode Node { get; }
        public readonly Direction InsertDirection { get; }
        public readonly VertexNode? InsertNode { get; }

        public InsertionPoint(VertexNode node, Direction direction) : this(node, direction, null)
        {
        }

        public InsertionPoint(VertexNode node, Direction direction, VertexNode? insertNode)
        {
            Node = node;
            InsertDirection = direction;
            InsertNode = insertNode;
        }
    }

    public class VertexNode
    {
        public int X { get; }
        public int Y { get; }
        public VertexNode?[] AdjascentNodes { get; }

        public VertexNode(int x, int y)
        {
            X = x;
            Y = y;
            AdjascentNodes = new VertexNode?[] { null, null, null, null, };
        }

        public static bool WithinRangeInclusive(int value, int r1, int r2)
        {
            int min = Math.Min(r1, r2);
            int max = Math.Max(r1, r2);
            return min <= value && value <= max;
        }

        public static bool WithinRangeExclusive(int value, int r1, int r2)
        {
            int min = Math.Min(r1, r2);
            int max = Math.Max(r1, r2);
            return min < value && value < max;
        }

        public static void GetAxisIntersectionsBetween(Dictionary<int, HashSet<VertexNode>> axisMap,
                                                       HashSet<int> visited,
                                                       VertexNode node,
                                                       Axis axis,
                                                       int axisRangeLow,
                                                       int axisRangeHigh,
                                                       int searchAxis)
        {
            if (HasBeenVisited(visited, node))
            {
                return;
            }

            if (axis == Axis.X)
            {
                AddNodeToMapIfWithinRange(axisMap, node, node.X, node.Y, axisRangeLow, axisRangeHigh, searchAxis);
            }
            else
            {
                AddNodeToMapIfWithinRange(axisMap, node, node.Y, node.X, axisRangeLow, axisRangeHigh, searchAxis);
            }

            foreach (VertexNode? n in node.AdjascentNodes)
            {
                if (n == null)
                {
                    continue;
                }

                GetAxisIntersectionsBetween(axisMap, visited, n, axis, axisRangeLow, axisRangeHigh, searchAxis);
            }
        }

        private static void AddNodeToMapIfWithinRange(Dictionary<int, HashSet<VertexNode>> axisMap,
                                                      VertexNode node,
                                                      int rangeValue,
                                                      int keyValue,
                                                      int axisRangeLow,
                                                      int axisRangeHigh,
                                                      int searchAxis)
        {
            if (axisRangeLow <= rangeValue && rangeValue <= axisRangeHigh && keyValue != searchAxis)
            {
                if (axisMap.TryGetValue(keyValue, out HashSet<VertexNode>? value))
                {
                    _ = value.Add(node);
                }
                else
                {
                    axisMap.Add(keyValue, new() { node });
                }
            }
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

        public static bool ConnectionExists(Direction direction, VertexNode v1, VertexNode v2)
        {
            return v1.AdjascentNodes[(byte)direction] == v2
                   && v2.AdjascentNodes[DirectionUtils.InvertDirection((byte)direction)] == v1;
        }

        public static void InsertNode(Direction direction, VertexNode root, VertexNode newNode)
        {
            VertexNode? existingNode = root.AdjascentNodes[(byte)direction];
            if (existingNode != null)
            {
                if (newNode != existingNode)
                {
                    InsertWithExistingNode(direction, root, newNode, existingNode);
                }
            }
            else
            {
                ConnectAdjascentNode(direction, root, newNode);

                InsertionPoint? insertionPoint = FindInsertionPoint(new HashSet<int>(), root, newNode);
                if (insertionPoint != null)
                {
                    InsertNode(insertionPoint.Value.InsertDirection, insertionPoint.Value.Node, newNode);
                }

                List<InsertionPoint> intersectionPoints = new();
                FindIntersectionPoints(new HashSet<int>(), intersectionPoints, root, newNode, direction, GetInsertLength(root, direction, newNode));

                foreach (InsertionPoint point in intersectionPoints)
                {
                    if (point.InsertNode == null)
                    {
                        continue;
                    }

                    InsertNode(point.InsertDirection, point.Node, point.InsertNode);
                    InsertNode(direction, root, point.InsertNode);
                }
            }
        }

        private static int GetInsertLength(VertexNode root, Direction direction, VertexNode insertNode)
        {
            return direction switch
            {
                Direction.Up or Direction.Down => Math.Abs(root.Y - insertNode.Y),
                Direction.Left or Direction.Right => Math.Abs(root.X - insertNode.X),
                _ => 0,
            };
        }

        private static bool NodeBetweenTwoNodes(VertexNode node, VertexNode nodeA, VertexNode nodeB)
        {
            if (node.X == nodeA.X && node.X == nodeB.X)
            {
                return WithinRangeExclusive(node.Y, nodeA.Y, nodeB.Y);
            }
            else if (node.Y == nodeA.Y && node.Y == nodeB.Y)
            {
                return WithinRangeExclusive(node.X, nodeA.X, nodeB.X);
            }
            return false;
        }

        private static bool NodeIntersectsLine(Axis axis, VertexNode node, VertexNode nodeA, VertexNode nodeB)
        {
            if (axis == Axis.Y)
            {
                return WithinRangeExclusive(node.Y, nodeA.Y, nodeB.Y);
            }
            else
            {
                return WithinRangeExclusive(node.X, nodeA.X, nodeB.X);
            }
        }

        private static void FindIntersectionPoints(HashSet<int> visited,
                                                   List<InsertionPoint> intersections,
                                                   VertexNode root,
                                                   VertexNode insertNode,
                                                   Direction insertDirection,
                                                   int insertLength)
        {
            if (HasBeenVisited(visited, root))
            {
                return;
            }

            Axis insertAxis = DirectionUtils.GetAxis(insertDirection);

            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                VertexNode? adjascentNode = root.AdjascentNodes[(byte)direction];
                if (adjascentNode == null)
                {
                    continue;
                }

                Axis axis = DirectionUtils.GetAxis(direction);

                if (NodeIntersectsLine(axis, insertNode, adjascentNode, root))
                {
                    bool isInBounds = false;
                    switch (insertDirection)
                    {
                        case Direction.Up:
                            isInBounds = root.Y > insertNode.Y && root.Y < (insertNode.Y + insertLength);
                            break;
                        case Direction.Left:
                            isInBounds = root.X > insertNode.X && root.X < (insertNode.X + insertLength);
                            break;
                        case Direction.Down:
                            isInBounds = root.Y < insertNode.Y && root.Y > (insertNode.Y - insertLength);
                            break;
                        case Direction.Right:
                            isInBounds = root.X < insertNode.X && root.X > (insertNode.X - insertLength);
                            break;
                        default:
                            break;
                    }

                    if (isInBounds)
                    {
                        if (axis == Axis.X && insertAxis == Axis.Y)
                        {
                            intersections.Add(new InsertionPoint(root, direction, new(root.Y, insertNode.X)));
                        }
                        else if (axis == Axis.Y && insertAxis == Axis.X)
                        {
                            intersections.Add(new InsertionPoint(root, direction, new(root.X, insertNode.Y)));
                        }
                    }
                }
            }

            foreach (VertexNode? adjascentNode in root.AdjascentNodes)
            {
                if (adjascentNode == null)
                {
                    continue;
                }

                FindIntersectionPoints(visited, intersections, adjascentNode, insertNode, insertDirection, insertLength);
            }
        }

        private static InsertionPoint? FindInsertionPoint(HashSet<int> visited, VertexNode root, VertexNode insertNode)
        {
            if (HasBeenVisited(visited, root))
            {
                return null;
            }

            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                VertexNode? adjascentNode = root.AdjascentNodes[(byte)direction];
                if (adjascentNode == null)
                {
                    continue;
                }

                if (NodeBetweenTwoNodes(insertNode, adjascentNode, root))
                {
                    return new InsertionPoint(root, direction);
                }
            }

            foreach (VertexNode? adjascentNode in root.AdjascentNodes)
            {
                if (adjascentNode == null)
                {
                    continue;
                }

                InsertionPoint? insertionPoint = FindInsertionPoint(visited, adjascentNode, insertNode);
                if (insertionPoint != null)
                {
                    return insertionPoint;
                }
            }

            return null;
        }

        private static void InsertWithExistingNode(Direction direction, VertexNode root, VertexNode newNode, VertexNode existingNode)
        {
            if (GreaterThan(direction, newNode, existingNode))
            {
                InsertNode(direction, existingNode, newNode);
            }
            else if (LessThan(direction, newNode, existingNode))
            {
                ConnectAdjascentNode(direction, root, newNode);
                ConnectAdjascentNode(DirectionUtils.InvertDirection(direction), existingNode, newNode);
            }
        }

        public static bool GreaterThan(Direction direction, VertexNode a, VertexNode b)
        {
            return direction switch
            {
                Direction.Up => a.Y < b.Y,
                Direction.Left => a.X < b.X,
                Direction.Down => a.Y > b.Y,
                Direction.Right => a.X > b.X,
                _ => false,
            };
        }

        public static bool LessThan(Direction direction, VertexNode a, VertexNode b)
        {
            return direction switch
            {
                Direction.Up => a.Y > b.Y,
                Direction.Left => a.X > b.X,
                Direction.Down => a.Y < b.Y,
                Direction.Right => a.X < b.X,
                _ => false,
            };
        }

        public static int GetSeachAxisValue(Axis axis, VertexNode node)
        {
            return axis == Axis.X ? node.X : node.Y;
        }

        private static bool HasBeenVisited(HashSet<int> visited, VertexNode node)
        {
            int hash = node.GetHashCode();
            if (visited.Contains(hash))
            {
                return true;
            }

            _ = visited.Add(hash);

            return false;
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

        public string GetAdjascentNodesString()
        {
            return $"[U{AdjascentNodes[0],8}, L{AdjascentNodes[1],8}, D{AdjascentNodes[2],8}, R{AdjascentNodes[3],8}]";
        }

        public override string ToString()
        {
            return $"({X,2}, {Y,2})";
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

    public static class VertexNodeOperator
    {
    }

    public class LayoutGenerator
    {
        public static LayoutDescriptor Generate()
        {
            int width = 20;
            int height = 10;

            Console.WriteLine($"[{width}x{height}]");

            VertexNode root = new(0, 0);

            // AddRectangle(root, 2, 2);
            //
            // VertexNode n1 = new(5, 0);
            // VertexNode.InsertNode(Direction.Right, root, n1);
            //
            // VertexNode n6 = new(0, 5);
            // VertexNode.InsertNode(Direction.Down, root, n6);
            //
            // VertexNode n3 = new(5, 5);
            // VertexNode.InsertNode(Direction.Down, n1, n3);
            //
            // VertexNode n2 = new(10, 0);
            // VertexNode.InsertNode(Direction.Right, n1, n2);
            //
            // VertexNode n4 = new(10, 3);
            // VertexNode.InsertNode(Direction.Down, n2, n4);
            //
            // VertexNode n5 = new(4, 3);
            // VertexNode.InsertNode(Direction.Left, n4, n5);
            //
            // PrintGraph(new HashSet<int>(), root);

            AddRectangle(root, width, height);
            AddRectangle(root, 10, 10);
            AddRectangle(root, 4, 4);
            AddRectangle(root, 15, 5);
            AddRectangle(root, 18, 3);
            AddRectangle(root, 2, 2);

            PrintGraph(new HashSet<int>(), root);

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

            Console.WriteLine($"{root} : {root.GetAdjascentNodesString()}");

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


