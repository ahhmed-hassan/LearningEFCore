using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingCSharp
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Starting main thread: " + Thread.CurrentThread.ManagedThreadId);

            try
            {
                // Run two tasks in parallel
                Task<string> api1Task = FetchFromApi1Async();
                Task<string> api2Task = FetchFromApi2Async();
                Task<string> api3Task = FetchFromApi3Async();

                // Await both at once (non-blocking)
                string[] results = await Task.WhenAll(api1Task, api2Task, api3Task);

                Console.WriteLine("Both APIs done. Combining results on thread: " + Thread.CurrentThread.ManagedThreadId);
                Console.WriteLine($"Result: {results[0]} + {results[1]}+ {results[2]}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static async Task<string> FetchFromApi1Async()
        {
            Console.WriteLine("API1 starting on thread: " + Thread.CurrentThread.ManagedThreadId);
            await Task.Delay(2000);  // Simulate 2s network call
            Console.WriteLine("API1 done on thread: " + Thread.CurrentThread.ManagedThreadId);
            return "Data from API1";
        }

        static async Task<string> FetchFromApi2Async()
        {
            Console.WriteLine("API2 starting on thread: " + Thread.CurrentThread.ManagedThreadId);
            await Task.Delay(2000);  // Simulate 3s network call
                                     // Simulate an error sometimes (uncomment to test: throw new Exception("API2 failed!");)
            Console.WriteLine("API2 done on thread: " + Thread.CurrentThread.ManagedThreadId);
            return "Data from API2";
        }

        static async Task<string> FetchFromApi3Async()
        {
            Console.WriteLine("API3 starting on thread: " + Thread.CurrentThread.ManagedThreadId);
            await Task.Delay(2000);  // Simulate 1s network call
            Console.WriteLine("API3 done on thread: " + Thread.CurrentThread.ManagedThreadId);
            return "Data from API3";
        }
    }
}
