using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace IT481_Fitzpatrick_Unit_6
{
    public class DressingRooms
    {
        public int max_rooms;
        public Semaphore pool;
        public int maxItemsAllowed { get { return 6; } }
        public DressingRooms(int rooms = 3)
        {
            this.max_rooms = rooms;
            this.pool = new Semaphore(rooms, rooms);
        }
        public void RequestRoom(Customer customer)
        {
            Debug.WriteLine($"Customer {customer.id} Queued For Room");
            Thread thread = new Thread(() =>
            {
                this.pool.WaitOne();
                customer.TryOnItems(Thread.Sleep);
                this.pool.Release();
            });
            thread.Start();

        }
    }
    public class Customer
    {
        public int id = -1;
        public int numberOfItems;
        public DateTime queuedTime;
        public DateTime startTime;
        public DateTime endTime;
        public bool finished = false;
        public Customer(int numberOfItems)
        {
            this.startTime = DateTime.Now;
            if (numberOfItems == 0)
            {
                this.numberOfItems = new Random().Next(1, 20+1);
            }
            else
            {
                this.numberOfItems = numberOfItems;
            }
            
        }
        public void TryOnItems(Action<int> mockAction)
        {
            Debug.WriteLine($"Customer {this.id} in room");
            this.startTime = DateTime.Now;
            for (int i = 0; i < this.numberOfItems; i++)
            {
                // 1 to 3 minutes random for each item.
                int time = new Random().Next(1, 3+1);
                mockAction(time * 60 * 1000);
            }
            this.endTime = DateTime.Now;
            this.finished = true;
                
            Debug.WriteLine($"Customer {this.id} finished");
        }
        public double WaitTime
        {
            get
            {
                TimeSpan ts = this.startTime - this.queuedTime;
                return ts.TotalSeconds;
            }
        }
        public double UsageTime
        {
            get
            {
                TimeSpan ts = this.endTime - this.startTime;
                return ts.TotalSeconds;
            }
        }
  
    }
    public class Scenario
    {
        private DressingRooms room;
        private List<Customer> customers = new List<Customer>();
        public bool ScenarioFinished { get { return this.customers.All(customer => customer.finished); } }
        public int TotalCustomers { get { return this.customers.Count; } }
        public double AverageItems { get { return this.customers.Select(customer => customer.numberOfItems).Average(); } }
        public double AverageUsage { get { return this.customers.Select(customer => customer.UsageTime).Average(); } }
        public double AverageWait { get { return this.customers.Select(customer => customer.WaitTime).Average(); } }
        public double TotalWait { get { return this.customers.Select(customer => customer.WaitTime).Sum(); } }
        public string name;
        public Scenario(string name, int numberRooms, int numberCustomers)
        {
            this.name = name;
            this.room = new DressingRooms(numberRooms);
            for (int i = 0; i < numberCustomers; i++)
            {
                int numberOfItems = new Random().Next(1, this.room.maxItemsAllowed + 1);
                Customer customer = new Customer(numberOfItems);
                customer.id = i + 1;
                this.customers.Add(customer);
            }
        }
        public void Run()
        {
            foreach (Customer customer in this.customers)
            {
                customer.queuedTime = DateTime.Now;
                this.room.RequestRoom(customer);
            }
        }
        public string FormatSeconds(double seconds)
        {
            // Lose some precision here
            int secs = (int)seconds;
            return this.FormatSeconds(secs);
        }
        public string FormatSeconds(int seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            return string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                t.Hours,
                t.Minutes,
                t.Seconds
            );
        }
        public void PrintReport(string header = "Scenario Report")
        {
            Console.WriteLine();
            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine(header);
            Console.WriteLine($"Number of Customers: {this.TotalCustomers}");
            Console.WriteLine($"Number of Rooms: {this.room.max_rooms}");
            Console.WriteLine($"Average Number of Items: {this.AverageItems:F1}");
            Console.WriteLine($"Average Usage Time of the Room: {this.FormatSeconds(this.AverageUsage)}");
            Console.WriteLine($"Average Waiting For a Room: {this.FormatSeconds(this.AverageWait)}");
            Console.WriteLine($"Total Time Spent Waiting For a Room: {this.FormatSeconds(this.TotalWait)}");
            Console.WriteLine("-----------------------------------------------");
        }   
    }

    class Program
    {
        static void Main(string[] args)
        {

            Scenario ScenarioOne = new Scenario("One", 2, 10);
            Scenario ScenarioTwo = new Scenario("Two", 4, 10);
            Scenario ScenarioThree = new Scenario("Three", 6, 10);

            List<Scenario> scenarios = new List<Scenario> { ScenarioOne, ScenarioTwo, ScenarioThree };
            foreach (Scenario s in scenarios)
            {
                Console.WriteLine($"Running Scenario {s.name}");
                s.Run();
            }

            // Manually block
            // Forgive me father for I have sinned
            Console.WriteLine("Waiting for all scenarios to finish");
            while (!scenarios.All(s => s.ScenarioFinished))
            {
                continue;
            }

            foreach (Scenario s in scenarios)
            {
                
                s.PrintReport($"Scenario {s.name} Results");
            }


        }
    }
}
