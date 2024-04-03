
using Grid;

namespace Procedural
{
    public class LayoutDescriptor
    {
        public GridNode Node { get; }
        public int AreaWidth { get; }
        public int AreaHeight { get; }

        public LayoutDescriptor(GridNode vertexNode, int width, int height)
        {
            Node = vertexNode;
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

            GridNodeUtils.AddRectangle(root, width, height);
            GridNodeUtils.AddRectangle(root, 10, 10);
            GridNodeUtils.AddRectangle(root, 4, 4);
            GridNodeUtils.AddRectangle(root, 15, 5);
            GridNodeUtils.AddRectangle(root, 18, 3);
            GridNodeUtils.AddRectangle(root, 2, 2);

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


