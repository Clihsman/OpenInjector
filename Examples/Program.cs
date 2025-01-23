using OpenInjector;

namespace Examples;

internal class Program
{
    static void Main(string[] args)
    {
        Injector.Activate<App>();
       // Injector.Register<ServiceA>(Lifecycle.Singleton);
     //   Injector.Register<ServiceB>(Lifecycle.Singleton);
        App app = Injector.Build<App>();
        app.Run();
    }
}

class App
{
    [Autowired]
    private ServiceB serviceA { get; set; }
    [Autowired]
    private ServiceB serviceB { get; set; }

    [Autowired]
    private ServiceB serviceC { get; set; }
    [Autowired]
    private ServiceB serviceD { get; set; }
    [Autowired]
    private ServiceB serviceF { get; set; }



    public void Run()
    {
        serviceA.Print();
        serviceB.Print();
        serviceC.Print();
        serviceD.Print();
        serviceF.Print();
    }
}

class ServiceA
{
    static int instances = 0;

    public ServiceA() {
        Console.WriteLine(instances++);
    }

    public void Print()
    { 
        Console.WriteLine("Run ServiceA");
    }
}

[Singleton]
interface ServiceB
{
    public void Print();
}

class Service : ServiceB
{
    public void Print()
    {
        Console.WriteLine("Run ServiceB");
    }
}

[InjectorConfiguration]
class ConfigurationServiceC
{
    static int instances = 0;

    [Builder(requiredAttribute: typeof(SingletonAttribute))]
    private T ServiceB<T>()
    {
        Console.WriteLine(instances++);
        return (T)(ServiceB)(object)new Service();
    }
}