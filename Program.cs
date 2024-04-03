using CSharpSandbox;
using Grid;
using Procedural;

GridNode node = LayoutGenerator.Generate();

_ = new GridPrinter(node);

