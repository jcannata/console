using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Consoler
{
    class Program
    {
        static void Perform()
        {
            // Create an ActionBlock<int> object that prints its input 
            // and throws ArgumentOutOfRangeException if the input 
            // is less than zero. 
            var throwIfNegative = new ActionBlock<int>(n =>
            {
                Console.WriteLine("n = {0}", n);
                if (n < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
            });

            // Post values to the block.
            throwIfNegative.Post(0);
            throwIfNegative.Post(-1);
            throwIfNegative.Post(1);
            throwIfNegative.Post(-2);
            throwIfNegative.Complete();

            // Wait for completion in a try/catch block. 
            try
            {
                throwIfNegative.Completion.Wait();
            }
            catch (AggregateException ae)
            {
                // If an unhandled exception occurs during dataflow processing, all 
                // exceptions are propagated through an AggregateException object.
                ae.Handle(e =>
                {
                    Console.WriteLine("Encountered {0}: {1}",
                       e.GetType().Name, e.Message);
                    return true;
                });
            }
        }

        static void Main(string[] args)
        {
            Perform();
            Console.ReadLine();
        }
    }
}
