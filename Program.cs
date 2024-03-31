using CSharpSandbox;
using Procedural;

LayoutDescriptor descriptor = LayoutGenerator.Generate();

_ = new LayoutPrinter(descriptor);

