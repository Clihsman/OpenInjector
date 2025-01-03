namespace OpenInjector;

[AttributeUsage(AttributeTargets.Method)]
public class BuilderAttribute : Attribute
{
    public Attribute? RequiredAttribute { get; private set; }

    public BuilderAttribute() { }

    public BuilderAttribute(Attribute? requiredAttribute) {
        RequiredAttribute = requiredAttribute;
    }
}
