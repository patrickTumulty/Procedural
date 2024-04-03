
using Grid;

namespace Procedural
{
    public class LayoutDescriptor
    {
        public GridNode RootVertexNode { get; }
        public int AreaWidth { get; }
        public int AreaHeight { get; }

        public LayoutDescriptor(GridNode vertexNode, int width, int height)
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

            GridNode root = new(0, 0);

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
            //

            GridNodeUtils.AddRectangle(root, width, height);
            GridNodeUtils.AddRectangle(root, 10, 10);
            GridNodeUtils.AddRectangle(root, 4, 4);
            GridNodeUtils.AddRectangle(root, 15, 5);
            GridNodeUtils.AddRectangle(root, 18, 3);
            GridNodeUtils.AddRectangle(root, 2, 2);

            PrintGraph(new HashSet<int>(), root);

            return new LayoutDescriptor(root, width, height);
        }

        private static void PrintGraph(HashSet<int> visited, GridNode? root)
        {
            if (root == null || visited.Contains(root.GetHashCode()))
            {
                return;
            }

            _ = visited.Add(root.GetHashCode());

            Console.WriteLine($"{root} : {root.GetAdjascentNodesString()}");

            foreach (GridNode? n in root.AdjascentNodes)
            {
                PrintGraph(visited, n);
            }
        }
    }
}


