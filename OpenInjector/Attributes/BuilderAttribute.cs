namespace OpenInjector;

[AttributeUsage(AttributeTargets.Method)]
public class BuilderAttribute : Attribute
{
    public Type? RequiredAttribute { get; private set; }

    public BuilderAttribute() { }

    public BuilderAttribute(Type? requiredAttribute) {
        if (requiredAttribute?.BaseType != typeof(Attribute))
            throw new NotSupportedException();

        RequiredAttribute = requiredAttribute;
    }
}
