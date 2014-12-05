using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Consoler
{
    class Program
    {
        static async Task<TimeSpan> PerformBroadcast(CancellationToken token)
        {
            var start = DateTime.Now;

            // Create a BroadcastBlock<double> object. 
            var broadcastBlock = new BroadcastBlock<double>(null);

            // Post a message to the block.
            broadcastBlock.Post(Math.PI);

            // Receive the messages back from the block several times. 
            for (int i = 0; i < 3; i++)
            {
                var number = await broadcastBlock.ReceiveAsync(token);
                Console.WriteLine(number);
            }

            // Create a TransformManyBlock<string, char> object that splits 
            // a string into its individual characters. 
            var transformManyBlock = new TransformManyBlock<string, char>(
               s => s.ToCharArray());

            // Post two messages to the first block.
            transformManyBlock.Post("Hello");
            transformManyBlock.Post("World");
            transformManyBlock.Complete();

            // Receive all output values from the block. 
            for (int i = 0; i < ("Hello" + "World").Length ; i++)
            {
                await Task.Delay(TimeSpan.FromSeconds(.5), token);
                Console.WriteLine(await transformManyBlock.ReceiveAsync(token));
            }

            transformManyBlock.Completion.Wait(token);
            Console.WriteLine("PerformBroadcast complete");

            return DateTime.Now - start;
        }

        static async Task PerformAction()
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

            // Create a continuation task that prints the overall  
            // task status to the console when the block finishes.
            throwIfNegative.Completion.ContinueWith(task =>
            {
                Console.WriteLine("The status of the completion task is '{0}'.", task.Status);
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

        static async Task MainAsync(string[] args, CancellationToken token)
        {
            try
            {
                var broadcast = PerformBroadcast(token);
                await PerformAction();
                var time = await broadcast;
                Console.WriteLine(time);
            }
            catch (Exception e)
            {
                Console.WriteLine("Encountered {0}: {1}",
                    e.GetType().Name, e.Message);
            }
        }

        static void Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            System.Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            MainAsync(args, cts.Token).Wait();
            Console.ReadLine();
        }
    }
}
