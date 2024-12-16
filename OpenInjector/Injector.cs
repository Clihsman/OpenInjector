using System.Reflection;

namespace OpenInjector;

public static class Injector
{
    private readonly static Dictionary<Type, Inject> Injects = [];
    private readonly static Dictionary<Type, IEnumerable<PropertyInfo>> BuilderCache = [];
    private readonly static Dictionary<Type, object> Instances = [];

    public static void Activate<T>()
    {
        var types = Assembly.GetAssembly(typeof(T))?.GetTypes().Where(e => e.GetCustomAttribute<InjectorConfigurationAttribute>() is not null);
        if (types is null) return;

        foreach (Type configuration in types)
        {
            var nethods = configuration.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(e => e.GetCustomAttribute<BuilderAttribute>() is not null);

            object? instace = Activator.CreateInstance(configuration);

            foreach (MethodInfo method in nethods)
            {
                if (!Injects.TryAdd(method.ReturnType, new Inject { Instance = instace, MethodInfo = method }))
                    throw new InvalidOperationException($"A builder for the type '{method.ReturnType.FullName}' is already registered.");
            }
        }
    }

    private static void InjectField(PropertyInfo propertyInfo, object target)
    {
        if (Injects.TryGetValue(propertyInfo.PropertyType, out Inject inject))
        {
            inject.Build(propertyInfo, target);
            return;
        }

        object? value = Activator.CreateInstance(propertyInfo.PropertyType);
        ArgumentNullException.ThrowIfNull(value);

        ActivateInstance(value);
        propertyInfo.SetValue(target, value);
    }

    public static void Register<T>(Lifecycle lifecycle) where T : new()
    {
        if (lifecycle == Lifecycle.Singleton)
        {
            T instance = Build<T>();
            ArgumentNullException.ThrowIfNull(instance);
            Instances.Add(typeof(T), instance);
        }
    }

    public static T Build<T>() where T : new()
    {
        var properities = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(e => e.GetCustomAttribute<AutowiredAttribute>() is not null);

        T instance = new();

        foreach (PropertyInfo property in properities)
        {
            InjectField(property, instance);
        }

        return instance;
    }

    public static void ActivateInstance(object instance)
    {
        Type type = instance.GetType();

        if (!BuilderCache.TryGetValue(type, out var properities))
        {
            properities = instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(e => e.GetCustomAttribute<AutowiredAttribute>() is not null);
            BuilderCache.Add(type, properities);
        }

        foreach (PropertyInfo property in properities)
        {
            InjectField(property, instance);
        }
    }

    private struct Inject
    {
        public object? Instance { get; set; }
        public MethodInfo MethodInfo { get; set; }

        public readonly void Build(PropertyInfo propertyInfo, object? target)
        {
            ArgumentNullException.ThrowIfNull(target);

            object? value = MethodInfo.Invoke(Instance, null);
            ArgumentNullException.ThrowIfNull(value);

            ActivateInstance(value);
            propertyInfo.SetValue(target, value);
        }
    }
}
