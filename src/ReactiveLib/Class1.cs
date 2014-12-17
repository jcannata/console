using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReactiveLib
{
    public class Class1
    {
        readonly CancellationToken _token;

        public Class1(CancellationToken token)
        {
            _token = token;
        }

        public void Time()
        {
            Console.WriteLine(DateTime.Now);

            // create a single event in 10 seconds time
            var observable = Observable.Timer(TimeSpan.FromSeconds(10)).Timestamp();

            // raise exception if no event received within 9 seconds
            var observableWithTimeout = observable.Timeout(TimeSpan.FromSeconds(9));

            using (observableWithTimeout.Subscribe(
                x => Console.WriteLine("{0}: {1}", x.Value, x.Timestamp),
                ex => Console.WriteLine("{0} {1}", ex.Message, DateTime.Now)))
            {
                Console.WriteLine("Press any key to unsubscribe");
                Console.ReadKey();
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        public void Test()
        {
            var observable = Observable.Interval(TimeSpan.FromMilliseconds(750)).TimeInterval();

            using (observable.Subscribe(
                x => Console.WriteLine("{0}: {1}", x.Value, x.Interval)))
            {
                Console.WriteLine("Press any key to unsubscribe");
                Console.ReadKey();
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
