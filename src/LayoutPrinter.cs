using Grid;

namespace CSharpSandbox
{
    public class GridPrinter
    {
        private static readonly Dictionary<LayoutSymbol, char> symbolsMap = new() {
            { LayoutSymbol.StraightH, '─' },
            { LayoutSymbol.StraightV, '│' },
            { LayoutSymbol.StraightEW, '•' },
            { LayoutSymbol.StraightNS, '•' },
            { LayoutSymbol.CornerNW, '┘' },
            { LayoutSymbol.CornerNE, '└' },
            { LayoutSymbol.CornerSW, '┐' },
            { LayoutSymbol.CornerSE, '┌' },
            { LayoutSymbol.CornerNSE, '├' },
            { LayoutSymbol.CornerNSW, '┤' },
            { LayoutSymbol.CornerNSEW, '┼' },
            { LayoutSymbol.CornerSWE, '┬'},
            { LayoutSymbol.CornerNWE, '┴'}
        };

        private static readonly Dictionary<LayoutSymbol, bool[]> layoutSymbolConditionsMap = new() {
                                   // Up,   Left,  Down,  Right
            { LayoutSymbol.CornerNW, new bool[] { true, true, false, false }},
            { LayoutSymbol.CornerNE, new bool[] { true, false, false, true }},
            { LayoutSymbol.CornerSW, new bool[] { false, true, true, false }},
            { LayoutSymbol.CornerSE, new bool[] { false, false, true, true }},
            { LayoutSymbol.CornerNSE, new bool[] { true, false, true, true }},
            { LayoutSymbol.CornerNSW, new bool[] { true, true, true, false }},
            { LayoutSymbol.CornerSWE, new bool[] { false, true, true, true }},
            { LayoutSymbol.CornerNWE, new bool[] { true, true, false, true }},
            { LayoutSymbol.CornerNSEW, new bool[] { true, true, true, true }},
            { LayoutSymbol.StraightEW, new bool[] { false, true, false, true }},
            { LayoutSymbol.StraightNS, new bool[] { true, false, true, false }},
        };

        private readonly char[,] areaMatrix;

        public GridPrinter(GridNode root)
        {
            AreaBounds ab = GridNodeUtils.GetAreaBounds(root);
            Console.WriteLine(ab);

            int width = ((ab.MaxX - ab.MinX) * 2) + 1;
            int height = ab.MaxY - ab.MinY + 1;

            Console.WriteLine($"width={width}, height={height}");

            areaMatrix = new char[height, width];
            InitMatrix();

            DrawLines(new(), root);
            DrawIntersections(new(), root);

            PrintAreaMatrix();
        }

        private void WriteCharacter(int x, int y, char c)
        {
            areaMatrix[y, x * 2] = c;
        }

        private void PrintAreaMatrix()
        {
            for (int i = 0; i < areaMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < areaMatrix.GetLength(1); j++)
                {
                    Console.Write(areaMatrix[i, j]);
                }
                Console.Write('\n');
            }
        }

        private void InitMatrix()
        {
            for (int i = 0; i < areaMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < areaMatrix.GetLength(1); j++)
                {
                    areaMatrix[i, j] = ' ';
                }
            }
        }

        private void DrawIntersections(HashSet<int> visited, GridNode currentNode)
        {
            int hash = currentNode.GetHashCode();
            if (visited.Contains(hash))
            {
                return;
            }

            _ = visited.Add(hash);

            WriteCharacter(currentNode.X, currentNode.Y, GetNodeSymbol(currentNode));

            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                int directionInt = (int)direction;
                GridNode? node = currentNode.AdjascentNodes[directionInt];
                if (node != null)
                {
                    DrawIntersections(visited, node);
                }
            }
        }

        private void DrawLines(HashSet<int> visited, GridNode currentNode)
        {
            int hash = currentNode.GetHashCode();
            if (visited.Contains(hash))
            {
                return;
            }

            _ = visited.Add(hash);

            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                int directionInt = (int)direction;
                GridNode? node = currentNode.AdjascentNodes[directionInt];
                if (node != null)
                {
                    if (direction is Direction.Down or Direction.Right)
                    {
                        DrawConnectingLine(direction, currentNode, node);
                    }

                    DrawLines(visited, node);
                }
            }
        }

        private void DrawConnectingLine(Direction direction, GridNode from, GridNode to)
        {
            Axis axis = DirectionUtils.GetAxis(direction);
            char connector = axis == Axis.Y ? symbolsMap[LayoutSymbol.StraightV] : symbolsMap[LayoutSymbol.StraightH];

            if (axis == Axis.Y)
            {
                int lowerY = from.Y + 1;
                int upperY = to.Y;
                for (int i = lowerY; i < upperY; i++)
                {
                    WriteCharacter(from.X, i, connector);
                }
            }
            else
            {
                int lowerX = (from.X * 2) + 1;
                int upperX = to.X * 2;
                for (int i = lowerX; i < upperX; i++)
                {
                    areaMatrix[from.Y, i] = connector;
                }
            }
        }

        private static char GetNodeSymbol(GridNode node)
        {
            foreach (LayoutSymbol symbol in layoutSymbolConditionsMap.Keys)
            {
                bool[] match = layoutSymbolConditionsMap[symbol];
                bool foundMatch = true;
                for (int j = 0; j < 4; j++)
                {
                    if (match[j] != (node.AdjascentNodes[j] != null))
                    {
                        foundMatch = false;
                        break;
                    }
                }
                if (foundMatch)
                {
                    return symbolsMap[symbol];
                }
            }
            return '•';
        }


        public enum LayoutSymbol : int
        {
            CornerNW = 0,
            CornerNE = 1,
            CornerSW = 2,
            CornerSE = 3,
            CornerNSE = 4,
            CornerNSW = 5,
            CornerSWE = 6,
            CornerNWE = 7,
            CornerNSEW = 8,
            StraightH = 9,
            StraightV = 10,
            StraightEW = 11,
            StraightNS = 12,
        }
    }
}
