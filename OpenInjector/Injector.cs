using System.Reflection;

namespace OpenInjector;

public static class Injector
{
    private readonly static Dictionary<Type, Inject> Injects = [];

    public static void Activate<T>() {
        var types = Assembly.GetAssembly(typeof(T))?.GetTypes().Where(e => e.GetCustomAttribute<InjectorConfigurationAttribute>() is not null);
        if (types is null) return;

        foreach (Type configuration in types) {
            var nethods = configuration.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(e => e.GetCustomAttribute<BuilderAttribute>() is not null);

            object? instace = Activator.CreateInstance(configuration);

            foreach (MethodInfo method in nethods) {
                Injects.Add(method.ReturnType, new Inject { Instance= instace, MethodInfo = method });
            }
        }
    }

    private static void InjectField(PropertyInfo propertyInfo, object target) {
        if (Injects.TryGetValue(propertyInfo.PropertyType, out Inject inject))
        {
            inject.Build(propertyInfo, target);
            return;
        }

        object? value = Activator.CreateInstance(propertyInfo.PropertyType);
        ArgumentNullException.ThrowIfNull(value);

        propertyInfo.SetValue(target, value); 
    }

    public static T Build<T>() where T : new() {
        var properities = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(e => e.GetCustomAttribute<AutowiredAttribute>() is not null);

        T instance = new();

        foreach (PropertyInfo property in properities)
        {
            InjectField(property, instance);
        }

        return instance;
    }

    private struct Inject { 
        public  object? Instance { get; set; }
        public MethodInfo MethodInfo { get; set; }

        public readonly void Build(PropertyInfo propertyInfo, object? target) {
            ArgumentNullException.ThrowIfNull(target);

            object? value = MethodInfo.Invoke(Instance, null);
            propertyInfo.SetValue(target, value);
        }
    }
}
