using System.Reflection;

namespace OpenInjector;

public static class Injector
{
    private readonly static Dictionary<Type, Inject> Injects = [];
    private readonly static Dictionary<Type, IEnumerable<PropertyInfo>> BuilderCache = [];
    private readonly static Dictionary<Type, object> Instances = [];

    public static void ActivateAll()
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) { 
            Activate(assembly);
        }
    }

    public static void ActivateAssemblyBuilder()
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()
            .Where(e => e.GetCustomAttribute<AssemblyBuilderAttribute>() is not null))
        {
            Activate(assembly);
        }
    }

    public static void Activate<T>() 
        => Activate(Assembly.GetAssembly(typeof(T))!);

    public static void Activate(Assembly assembly)
    {
        var types = assembly.GetTypes().Where(e => e.GetCustomAttribute<InjectorConfigurationAttribute>() is not null);
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

        #region Singleton

        var singletonTypes = assembly.GetTypes().Where(e => e.GetCustomAttribute<SingletonAttribute>() is not null);
        if (singletonTypes is null) return;

        foreach (Type singletonType in singletonTypes)
        {
            Register(singletonType, Lifecycle.Singleton);
        }

        #endregion
    } 

    public static void Register<T>(Lifecycle lifecycle) => Register(typeof(T), lifecycle);

    public static void Register(Type type, Lifecycle lifecycle)
    {
        if (lifecycle == Lifecycle.Singleton)
        {
            if (Instances.ContainsKey(type)) {
                throw new InvalidOperationException("ServiceA is already registered as Singleton.");
            }

            if (Injects.TryGetValue(type, out Inject inject))
            {
                object? value = inject.MethodInfo.Invoke(inject.Instance, null);
                ArgumentNullException.ThrowIfNull(value);
                ActivateInstance(value);
                Instances.Add(type, value);
                return;
            }

            object? instance = Activator.CreateInstance(type);
            ArgumentNullException.ThrowIfNull(instance);
            Instances.Add(type, instance);
        }
    }

    public static T Build<T>() where T : new()
    {
        Type type = typeof(T);

        if (!BuilderCache.TryGetValue(type, out var properities))
        {
            properities = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(e => e.GetCustomAttribute<AutowiredAttribute>() is not null);
            BuilderCache.Add(type, properities);
        }

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
            properities = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(e => e.GetCustomAttribute<AutowiredAttribute>() is not null);
            BuilderCache.Add(type, properities);
        }

        foreach (PropertyInfo property in properities)
        {
            InjectField(property, instance);
        }
    }

    private static void InjectField(PropertyInfo propertyInfo, object target)
    {
        if (Instances.TryGetValue(propertyInfo.PropertyType, out object? instance))
        {
            ArgumentNullException.ThrowIfNull(instance);
            propertyInfo.SetValue(target, instance);
            return;
        }

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
