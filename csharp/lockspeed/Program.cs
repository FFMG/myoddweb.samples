//MIT License
//
//Copyright(c) 2019 FFMG
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.
using System;
using System.Diagnostics;
using System.Threading;
// ReSharper disable NotAccessedVariable

namespace lockspeed
{
  internal class Program
  {
    private static void Main()
    {
      const int count = 10_000_00;
      TimedFunction( count, NoLock, "No lock");
      TimedFunction(count, WithLock, "Lock Object");
      TimedFunction(count, WithSemaphore, "Lock Semaphore");
      TimedFunction(count, WithMutex, "Lock Mutex"); 
      TimedFunction(count, WithReaderWriterLockRead, "Lock WithReaderWriterLock (read)");
      TimedFunction(count, WithReaderWriterLockWrite, "Lock WithReaderWriterLock (write)");

      Console.WriteLine("Press a key to continue ...");
      Console.ReadKey();
    }

    private static void TimedFunction( int count, Action<int> fn, string name )
    {
      var ts = new TimeSpan(0);
      for (var i = 0; i < 5; ++i)
      {
        var sw = new Stopwatch();
        sw.Start();
        try
        {
          fn(count);
        }
        finally
        {
          sw.Stop();
          if (sw.Elapsed > ts )
          {
            ts = sw.Elapsed;
          }
        }
      }
      Console.WriteLine($"{ts} : {name}");
    }

    private static void NoLock( int count )
    {
      var rng = new Random();
      var someNumber = 0;
      
      for (var i = 0; i < count; ++i)
      {
        someNumber += rng.Next(0, 10);
      }
    }

    private static void WithLock(int count)
    {
      var l = new object();
      var rng = new Random();
      var someNumber = 0;
      
      for (var i = 0; i < count; ++i)
      {
        lock (l)
        {
          someNumber += rng.Next(0, 10);
        }
      }
    }

    private static void WithMutex(int count)
    {
      var m = new Mutex();
      var rng = new Random();
      var someNumber = 0;
      
      for (var i = 0; i < count; ++i)
      {
        m.WaitOne();
        try
        {
          someNumber += rng.Next(0, 10);
        }
        finally
        {
          m.ReleaseMutex();
        }
      }
    }

    private static void WithSemaphore(int count)
    {
      var s = new SemaphoreSlim(1,1);
      var rng = new Random();
      var someNumber = 0;
      
      for (var i = 0; i < count; ++i)
      {
        s.Wait();
        try
        {
          someNumber += rng.Next(0, 10);
        }
        finally
        {
          s.Release();
        }
      }
    }

    private static void WithReaderWriterLockRead(int count)
    {
      var s = new ReaderWriterLockSlim();
      var rng = new Random();
      var someNumber = 0;
      
      for (var i = 0; i < count; ++i)
      {
        s.EnterReadLock();
        try
        {
          someNumber += rng.Next(0, 10);
        }
        finally
        {
          s.ExitReadLock();
        }
      }
    }

    private static void WithReaderWriterLockWrite(int count)
    {
      var s = new ReaderWriterLockSlim();
      var rng = new Random();
      var someNumber = 0;
      for (var i = 0; i < count; ++i)
      {
        s.EnterWriteLock();
        try
        {
          someNumber += rng.Next(0, 10);
        }
        finally
        {
          s.ExitWriteLock();
        }
      }
    }
  }
}
