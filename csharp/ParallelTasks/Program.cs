using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ParallelTasks
{
  internal class Program
  {
    private const int NumberOfTasks = 10;

    private static async Task Main()
    { 
      // the default 
      await TimeMethod("Tasks not in parallel", NonParallelTasks ).ConfigureAwait( false );
      await TimeMethod("Tasks use Parallel for each", UseParallelForEach).ConfigureAwait(false);
      await TimeMethod("Tasks use When All", WhenAll).ConfigureAwait(false);
      await TimeMethod("Fake Tasks use When All", FakeWhenAll).ConfigureAwait(false);

      Console.WriteLine("Bye");
      Console.ReadKey();
    }

    private static async Task TimeMethod(string name, Func<Task> method )
    {
      var sw = new Stopwatch();
      sw.Start();
      try
      {
        await method().ConfigureAwait( false );
      }
      finally
      {
        Console.WriteLine($"{name} took {sw.ElapsedMilliseconds}ms");
      }
    }

    private static async Task FakeWhenAll()
    {
      // this task does not really run async
      Task LocalTask()
      {
        Task.Delay(1000).GetAwaiter().GetResult();
        return Task.CompletedTask;
      };

      // start tasks that will run one after the other
      var tasks = new List<Task>();
      for (var i = 0; i < NumberOfTasks; ++i)
      {
        tasks.Add(LocalTask());
      }

      // then wait for them all
      await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
    }

    private static async Task WhenAll()
    {
      // start 4 tasks that will run one after the other
      var tasks = new List<Task>();
      for (var i = 0; i < NumberOfTasks; ++i)
      {
        tasks.Add(Task.Delay(1000));
      }

      // then wait for them all
      await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
    }

    private static async Task UseParallelForEach()
    {
      // start 4 tasks that will run one after the other
      var tasks = new List<Task>();
      for (var i = 0; i < NumberOfTasks; ++i)
      {
        tasks.Add(Task.Delay(1000));
      }

      // make me 'async'
      await Task.Yield();

      // then run them in parallel.
      // don't be tempted to 'await' them, that's not how it works
      Parallel.ForEach(tasks, task =>
      {
        task.GetAwaiter().GetResult();
      });
    }

    private static async Task NonParallelTasks()
    {
      // start 4 tasks that will run one after the other
      var tasks = new List<Task>();
      for (var i = 0; i < NumberOfTasks; ++i)
      {
        tasks.Add( Task.Delay(1000));
      }

      // then run them one at a time
      for (var i = 0; i < NumberOfTasks; ++i)
      {
        await tasks[i].ConfigureAwait(false);
      }
    }
  }
}
