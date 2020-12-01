namespace Example.Features.Example
{
    public class ExampleModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Age { get; set; }
        public string NestedProperty { get; set; }
    }

    public class NestedModel
    {
        public string Description { get; set; }
    }
}