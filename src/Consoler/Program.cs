using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using ReactiveLib;

namespace Consoler
{
    internal class Program
    {
        static async Task<TimeSpan> PerformTransform(CancellationToken token)
        {
            DateTime start = DateTime.Now;

            // Create a TransformManyBlock<string, char> object that splits 
            // a string into its individual characters. 
            var transformManyBlock = new TransformManyBlock<string, char>(
                s => s.ToCharArray());

            // Post two messages to the first block.
            transformManyBlock.Post("Hello");
            transformManyBlock.Post("World");
            transformManyBlock.Complete();

            // Receive all output values from the block. 
            for (int i = 0; i < ("Hello" + "World").Length; i++)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250), token);
                Console.WriteLine(await transformManyBlock.ReceiveAsync(token));
            }

            transformManyBlock.Completion.Wait(token);

            return DateTime.Now - start;
        }

        static async Task<TimeSpan> PerformBroadcast(CancellationToken token)
        {
            DateTime start = DateTime.Now;

            // Create a BroadcastBlock<double> object. 
            var broadcastBlock = new BroadcastBlock<double>(null);

            // Post a message to the block.
            broadcastBlock.Post(Math.PI);

            // Receive the messages back from the block several times. 
            for (int i = 0; i < 3; i++)
            {
                double number = await broadcastBlock.ReceiveAsync(token);
                Console.WriteLine(number);
            }

            return DateTime.Now - start;
        }

        static async Task<TimeSpan> PerformAction(CancellationToken token)
        {
            DateTime start = DateTime.Now;

            // Create an ActionBlock<int> object that prints its input 
            // and throws ArgumentOutOfRangeException if the input 
            // is less than zero. 
            var throwIfNegative = new ActionBlock<int>(n =>
            {
                Console.WriteLine("n = {0}", n);
                if (n < 0)
                    throw new ArgumentOutOfRangeException();
            });

            // Create a continuation task that prints the overall  
            // task status to the console when the block finishes.
            var finalFinal = throwIfNegative.Completion.ContinueWith(task => Console.WriteLine("The status of the completion task is '{0}'.", task.Status), token);

            // Post values to the block.
            throwIfNegative.Post(0);
            throwIfNegative.Post(-1);
            throwIfNegative.Post(1);
            throwIfNegative.Post(-2);
            throwIfNegative.Complete();

            // Wait for completion in a try/catch block. 
            try
            {
                throwIfNegative.Completion.Wait(token);
                finalFinal.Wait(token);
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

            return DateTime.Now - start;
        }

        static async Task<TimeSpan> PerformBatch(CancellationToken token)
        {
            var start = DateTime.Now;

            // Create a BatchBlock<int> object that holds ten 
            // elements per batch. 
            var batchBlock = new BatchBlock<int>(10);

            var doSomething = new ActionBlock<int[]>(a =>
            {
                Console.WriteLine("The sum of the elements is {0}.", a.Sum());
            });

            batchBlock.LinkTo(doSomething);
            var finalFinal = batchBlock.Completion.ContinueWith(delegate { doSomething.Complete(); }, token);

            // Post several values to the block. 
            for (var i = 0; i < 13; i++)
            {
                batchBlock.Post(i);
            }

            // Set the block to the completed state. This causes 
            // the block to propagate out any any remaining 
            // values as a final batch.
            batchBlock.Complete();
            //await finalFinal;
            doSomething.Completion.Wait(token);

            // Print the sum of both batches.

            return DateTime.Now - start;
        }

        static async Task MainAsync(string[] args, CancellationToken token)
        {
            try
            {
                var reactiveSample = new Class1(token);
                reactiveSample.Time();
                return;

                // start tasks...
                var batch = PerformBatch(token);
                var broadcast = PerformBroadcast(token);
                var action = PerformAction(token);
                var transform = PerformTransform(token);

                Console.WriteLine("broadcast:{0}", await broadcast);
                Console.WriteLine("action:{0}", await action);
                Console.WriteLine("transform:{0}", await transform);
                Console.WriteLine("batch:{0}", await batch);
            }
            catch (Exception e)
            {
                Console.WriteLine("Encountered {0}: {1}",
                    e.GetType().Name, e.Message);
            }
        }

        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            MainAsync(args, cts.Token).Wait();
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }
}