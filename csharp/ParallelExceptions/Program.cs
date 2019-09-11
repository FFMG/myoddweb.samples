using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParallelExceptions
{
  internal class Program
  {
    private static void Main()
    {
      ParallelForEach2Async();
      ParallelForEachAsync();
      //ParallelForEach();
      Console.WriteLine("Press a key!");
      Console.ReadKey();
    }

    private static void ParallelForEachAsync()
    {
      try
      {
        // use a concurent queue so all the thread can add
        // while it is an overhead we do not expect to have that many exceptions in production code.
        var exceptions = new ConcurrentQueue<Exception>();
        var numbers = new[] { 1, 2, 3, 4, 5 };
        var tasks = new List<Task>();

        async Task t(int number)
        {
          // protect everything with a try catch
          try
          {
            await Task.Delay(100).ConfigureAwait(false);
            Console.WriteLine($"Working on number: {number}");
            if (number == 3)
            {
              throw new Exception("Boom!");
            }
          }
          catch (Exception e)
          {
            // save it for later.
            exceptions.Enqueue(e);
          }
        }

        foreach (var number in numbers)
        {
          tasks.Add(t(number));
        }

        Task.WaitAll(tasks.ToArray());

        // we are back in our own thread
        if (exceptions.Count > 0)
        {
          throw new AggregateException(exceptions);
        }
      }
      catch (Exception e)
      {
        Console.WriteLine($"Caught exception! {e.Message}");
      }
    }

    private static void ParallelForEach2Async()
    {
      try
      {
        // use a concurent queue so all the thread can add
        // while it is an overhead we do not expect to have that many exceptions in production code.
        var numbers = new[] { 1, 2, 3, 4, 5 };
        var tasks = new List<Task<Exception>>();

        async Task<Exception> t(int number)
        {
          // protect everything with a try catch
          try
          {
            await Task.Delay(100).ConfigureAwait(false);
            Console.WriteLine($"Working on number: {number}");
            if (number == 3)
            {
              throw new Exception("Boom!");
            }
          }
          catch (Exception e)
          {
            // save it for later.
            return e;
          }
          return null;
        }

        foreach (var number in numbers)
        {
          tasks.Add( t(number) );
        }

        // get all the errors ... that are not null
        var errors = Task.WhenAll(tasks.ToArray()).GetAwaiter().GetResult().Where( e => e != null ).ToList();
        
        // we are back in our own thread
        if (errors.Count > 0)
        {
          throw new AggregateException(errors);
        }
      }
      catch (Exception e)
      {
        Console.WriteLine($"Caught exception! {e.Message}");
      }
    }

    private static void ParallelForEach()
    {
      try
      {
        var numbers = new[] {1, 2, 3, 4, 5};
        Parallel.ForEach( numbers, (number) =>
          {
            Console.WriteLine($"Working on number: {number}");
            if (number == 3)
            {
              throw new Exception( "Boom!");
            }
          }
        );
      }
      catch (Exception e)
      {
        Console.WriteLine( $"Caught exception! {e.Message}");
      }
    }
  }
}
