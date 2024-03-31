using Procedural;

namespace CSharpSandbox
{
    public class LayoutPrinter
    {
        private static readonly Dictionary<LayoutSymbol, char> symbolsMap = new() {
            { LayoutSymbol.StraightH, '─' },
            { LayoutSymbol.StraightV, '│' },
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
            { LayoutSymbol.StraightH, new bool[] { false, true, false, true }},
            { LayoutSymbol.StraightV, new bool[] { true, false, true, false }},
        };

        private readonly char[,] areaMatrix;

        public LayoutPrinter(LayoutDescriptor layoutDescriptor)
        {
            areaMatrix = new char[layoutDescriptor.AreaHeight, (layoutDescriptor.AreaWidth * 2) - 1];
            InitMatrix();

            HashSet<int> visitedSet = new();
            VertexNode root = layoutDescriptor.RootVertexNode;
            DrawLines(visitedSet, root);

            visitedSet.Clear();
            DrawIntersections(visitedSet, root);

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

        private void DrawIntersections(HashSet<int> visited, VertexNode currentNode)
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
                VertexNode? node = currentNode.AdjascentNodes[directionInt];
                if (node != null)
                {
                    DrawIntersections(visited, node);
                }
            }
        }

        private void DrawLines(HashSet<int> visited, VertexNode currentNode)
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
                VertexNode? node = currentNode.AdjascentNodes[directionInt];
                if (node != null)
                {
                    DrawConnectingLine(direction, currentNode, node);
                    DrawLines(visited, node);
                }
            }
        }

        private void DrawConnectingLine(Direction direction, VertexNode from, VertexNode to)
        {
            char connector = DirectionUtils.IsVertical(direction) ? symbolsMap[LayoutSymbol.StraightV] : symbolsMap[LayoutSymbol.StraightH];
            if (DirectionUtils.IsVertical(direction))
            {
                int lowerY = Math.Min(from.Y, to.Y) + 1;
                int upperY = Math.Max(from.Y, to.Y);
                for (int i = lowerY; i < upperY; i++)
                {
                    WriteCharacter(from.X, i, connector);
                }
            }
            else
            {
                int lowerX = Math.Min(from.X, to.X) + 1;
                int upperX = Math.Max(from.X, to.X) * 2;
                for (int i = lowerX; i < upperX; i++)
                {
                    areaMatrix[from.Y, i] = connector;
                }
            }
        }

        private static char GetNodeSymbol(VertexNode node)
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
            return '#';
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
        }
    }
}
