
using Grid;

namespace Procedural
{
    public class LayoutGenerator
    {
        public static GridNode Generate()
        {
            Random random = new();


            GridNode root = new(0, 0);

            GridNode? current;
            for (int i = 0; i < 4; i++)
            {
                int n = random.Next(0, i * 4), counter = 0;
                current = GridNodeUtils.FindFirst(root, (node) =>
                {
                    bool result = n == counter;
                    counter++;
                    return result;
                });

                if (current == null)
                {
                    continue;
                }

                GridNodeUtils.AddRectangle(current, random.Next(5, 10), random.Next(5, 15));
            }

            // GridNodeUtils.AddRectangle(root, width, height);
            // GridNodeUtils.AddRectangle(root, 13, 20);
            //
            // // GridNode n = new(13, 0);
            // // GridNodeUtils.InsertNode(Direction.Right, root, n);
            // // 
            // // GridNode n2 = new(13, 13);
            // // GridNodeUtils.InsertNode(Direction.Down, n, n2);
            //
            // GridNodeUtils.AddRectangle(root, 25, 8);
            // // GridNodeUtils.AddRectangle(root, 4, 4);
            // // GridNodeUtils.AddRectangle(root, 4, 4);
            // GridNodeUtils.AddRectangle(root, 15, 5);
            // GridNodeUtils.AddRectangle(root, 18, 3);
            // GridNodeUtils.AddRectangle(root, 2, 2);

            PrintGraph(new(), root);

            return root;
        }

        public static void PrintGraph(HashSet<int> visited, GridNode? root)
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


