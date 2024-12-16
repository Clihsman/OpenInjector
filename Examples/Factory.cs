using OpenInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examples
{
    internal class ProgramFactory
    {
        public static void Main(string[] args)
        {
            Injector.Activate<ProgramFactory>();
            AppFactory appFactory = Injector.Build<AppFactory>();
            appFactory.Run();
        }
    }

    class AppFactory
    {
        [Autowired]
        private Car Car { get; set; }

        public void Run() {
            Car.Engine.WriteLine();
            Car.Chassis.WriteLine();    
        }
    }

    public class Engine
    {
        public string Name { get; private set; }
        public float Force { get; private set; }

        public Engine(string name, float force) { 
            Name = name;
            Force = force;
        }

        public void WriteLine()
        {
            Console.WriteLine("Engine");
        }

        
    }

    public class Chassis
    {
        public string Color { get; private set; }

        public Chassis(string color)
        {
            Color = color;
        }

        public void WriteLine()
        {
            Console.WriteLine("Chassis");
        }
    }

    public class Car
    {
        public Engine Engine { get; private set; }
        public Chassis Chassis { get; private set; }

        public Car(Engine engine, Chassis chassis) {
            Engine  = engine;
            Chassis = chassis;
        }

    }

    [InjectorConfiguration]
    public class Factory
    {
        [Builder]
        public Car Car() {
            Engine engine = new Engine("V8", 16);
            Chassis chassis = new Chassis("RED");
            return new Car(engine, chassis);
        }
    }
}
