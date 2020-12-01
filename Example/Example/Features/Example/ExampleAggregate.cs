namespace Example.Features.Example
{
    public class ExampleAggregate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Age { get; set; }
        public NestedObject NestedObject { get; set; }
        public string NotFilterableProperty { get; set; }
    }

    public class NestedObject
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }
}