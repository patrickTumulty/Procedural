
namespace Grid
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

    public static class GridNodeUtils
    {
        public static void AddRectangle(GridNode root, int width, int height)
        {
            GridNode topRight = new(root.X + (width - 1), root.Y);
            InsertNode(Direction.Right, root, topRight);

            GridNode bottomRight = new(topRight.X, topRight.Y + (height - 1));
            InsertNode(Direction.Down, topRight, bottomRight);

            GridNode bottomLeft = new(bottomRight.X - (width - 1), bottomRight.Y);
            InsertNode(Direction.Left, bottomRight, bottomLeft);

            InsertNode(Direction.Up, bottomLeft, root);
        }

        public static void ConnectAdjascentNode(Direction direction, GridNode node, GridNode connectingNode)
        {
            ConnectAdjascentNode((byte)direction, node, connectingNode);
        }

        public static void ConnectAdjascentNode(byte direction, GridNode node, GridNode connectingNode)
        {
            node.AdjascentNodes[direction] = connectingNode;
            byte invertedDirection = DirectionUtils.InvertDirection(direction);
            GridNode? reverseAdjascentNode = connectingNode.AdjascentNodes[invertedDirection];
            if (reverseAdjascentNode == null || reverseAdjascentNode != node)
            {
                ConnectAdjascentNode(invertedDirection, connectingNode, node);
            }
        }

        public static bool ConnectionExists(Direction direction, GridNode v1, GridNode v2)
        {
            return v1.AdjascentNodes[(byte)direction] == v2
                   && v2.AdjascentNodes[DirectionUtils.InvertDirection((byte)direction)] == v1;
        }

        public static void InsertNode(Direction direction, GridNode root, GridNode insertNode)
        {
            GridNode? existingNode = root.AdjascentNodes[(byte)direction];
            if (existingNode != null)
            {
                if (insertNode != existingNode)
                {
                    InsertWithExistingNode(direction, root, insertNode, existingNode);
                }
            }
            else
            {
                ConnectAdjascentNode(direction, root, insertNode);

                ConnectNewNodeWithIntersectingLine(root, insertNode);

                ConnectLineIntersections(direction, root, insertNode);
            }
        }

        public static int GetSeachAxisValue(Axis axis, GridNode node)
        {
            return axis == Axis.X ? node.X : node.Y;
        }

        private static bool HasBeenVisited(HashSet<int> visited, GridNode node)
        {
            int hash = node.GetHashCode();
            if (visited.Contains(hash))
            {
                return true;
            }

            _ = visited.Add(hash);

            return false;
        }

        public static bool IsConnected(GridNode root, GridNode node)
        {
            foreach (GridNode? n in root.AdjascentNodes)
            {
                if (n != null && n == node)
                {
                    return true;
                }
            }
            return false;
        }

        private static void InsertWithExistingNode(Direction direction, GridNode root, GridNode newNode, GridNode existingNode)
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

        public static bool GreaterThan(Direction direction, GridNode a, GridNode b)
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

        public static bool LessThan(Direction direction, GridNode a, GridNode b)
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

        private readonly struct InsertionPoint
        {
            public readonly GridNode Node { get; }
            public readonly Direction InsertDirection { get; }
            public readonly GridNode? InsertNode { get; }

            public InsertionPoint(GridNode node, Direction direction) : this(node, direction, null)
            {
            }

            public InsertionPoint(GridNode node, Direction direction, GridNode? insertNode)
            {
                Node = node;
                InsertDirection = direction;
                InsertNode = insertNode;
            }
        }

        private static InsertionPoint? FindInsertionPoint(HashSet<int> visited, GridNode root, GridNode insertNode)
        {
            if (HasBeenVisited(visited, root))
            {
                return null;
            }

            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                GridNode? adjascentNode = root.AdjascentNodes[(byte)direction];
                if (adjascentNode == null)
                {
                    continue;
                }

                if (NodeBetweenTwoNodes(insertNode, adjascentNode, root))
                {
                    return new InsertionPoint(root, direction);
                }
            }

            foreach (GridNode? adjascentNode in root.AdjascentNodes)
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

        private static void FindIntersectionPoints(HashSet<int> visited,
                                                   List<InsertionPoint> intersections,
                                                   GridNode root,
                                                   GridNode insertNode,
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
                GridNode? adjascentNode = root.AdjascentNodes[(byte)direction];
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

            foreach (GridNode? adjascentNode in root.AdjascentNodes)
            {
                if (adjascentNode == null)
                {
                    continue;
                }

                FindIntersectionPoints(visited, intersections, adjascentNode, insertNode, insertDirection, insertLength);
            }
        }

        private static bool NodeBetweenTwoNodes(GridNode node, GridNode nodeA, GridNode nodeB)
        {
            if (node.X == nodeA.X && node.X == nodeB.X)
            {
                return NumberUtils.WithinRangeExclusive(node.Y, nodeA.Y, nodeB.Y);
            }
            else if (node.Y == nodeA.Y && node.Y == nodeB.Y)
            {
                return NumberUtils.WithinRangeExclusive(node.X, nodeA.X, nodeB.X);
            }
            return false;
        }

        private static bool NodeIntersectsLine(Axis axis, GridNode node, GridNode nodeA, GridNode nodeB)
        {
            return axis == Axis.Y
                ? NumberUtils.WithinRangeExclusive(node.Y, nodeA.Y, nodeB.Y)
                : NumberUtils.WithinRangeExclusive(node.X, nodeA.X, nodeB.X);
        }

        /// <summary>
        /// Get the length of the line between two nodes
        /// </summary>
        /// <param name="a">Node A</param>
        /// <param name="direction">Direction</param>
        /// <param name="b">Node B</param>
        /// <return>line length between a and b</returns>
        private static int GetLineLength(GridNode a, Direction direction, GridNode b)
        {
            return direction switch
            {
                Direction.Up or Direction.Down => Math.Abs(a.Y - b.Y),
                Direction.Left or Direction.Right => Math.Abs(a.X - b.X),
                _ => 0,
            };
        }

        /// <summary>
        /// Check if inserted node lands on a perpendicular line. If so, connect
        /// the new node to the line
        /// </summary>
        /// <param name="root">root node</param>
        /// <param name="insertNode">Node that is being inserted</param>
        private static void ConnectNewNodeWithIntersectingLine(GridNode root, GridNode insertNode)
        {
            InsertionPoint? insertionPoint = FindInsertionPoint(new HashSet<int>(), root, insertNode);
            if (insertionPoint != null)
            {
                InsertNode(insertionPoint.Value.InsertDirection, insertionPoint.Value.Node, insertNode);
            }
        }

        /// <summary>
        /// The line that is created between root and insertNode, check if that
        /// line intersects with any other line that are perpendicular. If so,
        /// connect those lines to the new line by adding additional nodes.
        /// </summary>
        /// <param name="direction">insert direction</param>
        /// <param name="root">root node</param>
        /// <param name="insertNode">Node that is being inserted</param>
        private static void ConnectLineIntersections(Direction direction, GridNode root, GridNode insertNode)
        {
            List<InsertionPoint> intersectionPoints = new();
            FindIntersectionPoints(new HashSet<int>(), intersectionPoints, root, insertNode, direction, GetLineLength(root, direction, insertNode));

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
}

