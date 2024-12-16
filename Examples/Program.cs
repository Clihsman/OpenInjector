using OpenInjector;

namespace Examples;

internal class Program
{
    static void Main(string[] args)
    {
        Injector.Activate<App>();
        App app = Injector.Build<App>();
        app.Run();
    }
}

class App
{
    [Autowired]
    private ServiceA serviceA { get; set; }
    [Autowired]
    private ServiceB serviceB { get; set; }

    public void Run()
    {
        serviceA.Print();
        serviceB.Print();
    }
}

class ServiceA
{
    public void Print()
    {
        Console.WriteLine("Run ServiceA");
    }
}

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
    [Builder]
    private ServiceB ServiceB()
    {
        return new Service();
    }
}