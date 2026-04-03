namespace ORLtrack.Tests.TestingFramework;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class FactAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal sealed class TraitAttribute : Attribute
{
    public TraitAttribute(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }
    public string Value { get; }
}
