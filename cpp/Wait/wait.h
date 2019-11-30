#pragma once
#include <chrono>
#include <thread>
#include <condition_variable>
#include <functional>
#include <future>

class Wait final
{
  struct Helper
  {
    Wait* parent;
    long long ms;
    std::function<bool()> predicate;
  };

  /** 
   * \brief the conditional locl
   */
  std::mutex _mutex;

  /**
   * \brief the thread wait
   */
  std::condition_variable _conditionVariable;

private:
  /**
   * \brief constructor, prevent direct construction
   */
  Wait()
  {
  }

public:
  // prevent copies.
  Wait(const Wait& ) = delete;
  const Wait& operator=(const Wait&) = delete;


  /**
   * \brief the number of ms we want to wait for.
   */
  static void Delay( const long long milliseconds )
  {
    SpinUntilInternal( nullptr, milliseconds );
  }

  /**
  * \brief the number of ms we want to wait for.
  * \param condition if this returns true we will return.
  * \param milliseconds the number of milliseconds we want to wait for
  *        if the timeout is reached, we will simply get out.
  * \return true if the condition fired, false if we timeout
  */
  static bool SpinUntil(std::function<bool()> condition, const long long milliseconds)
  {
    return SpinUntilInternal(condition, milliseconds);
  }

private:
  /**
   * \brief the number of ms we want to wait for and/or check for a condition
   *        this function does not do validations.
   * \param condition if this returns true we will return.
   * \param milliseconds the number of milliseconds we want to wait for
   *        if the timeout is reached, we will simply get out.
   * \return true if the condition fired, false if we timeout
   */
  static bool SpinUntilInternal(std::function<bool()> condition, const long long milliseconds)
  {
    Wait waiter;
    std::promise<bool> callResult;
    auto callFuture = callResult.get_future();

    // create the helper.
    Helper helper = { &waiter, milliseconds, condition };

    // start the thread with the arguments we have
    std::thread thread( _Awaiter, helper, std::move(callResult));

    // wait for the thread to complete.
    thread.join();

    // the result
    return callFuture.get();
  }

  bool Awaiter(const long long milliseconds, std::function<bool()> condition)
  {
    bool result = false;
    std::unique_lock<std::mutex> lock(_mutex);
    try
    {
      if (condition == nullptr)
      {
        // just wait for 'x' ms.
        _conditionVariable.wait_for(lock, std::chrono::milliseconds(milliseconds));

        // simple timeout, always false
        // because the condition could never be 'true'
        result = true;
      }
      else
      {
        const auto oneMillisecond = std::chrono::milliseconds(1);

        // when we want to sleep until.
        const auto until = std::chrono::high_resolution_clock::now() + std::chrono::milliseconds(milliseconds);
        for (auto count = 0; count < std::numeric_limits<int>::max(); ++count)
        {
          if (condition())
          {
            result = true;
            break;
          }

          if (count % 4 == 0)
          {
            // slee a little bit
            std::this_thread::sleep_for(oneMillisecond);
          }
          else
          {
            // yield.
            std::this_thread::yield();
          }

          // are we done?
          if (std::chrono::high_resolution_clock::now() >= until)
          {
            break;
          }
        }
      }
    }
    catch(...)
    {
      //  something broke ... maybe we should re-throw.
      result = false;
    }

    // release everything
    lock.unlock();
    _conditionVariable.notify_one();

    // all done
    return result;
  }

  static void _Awaiter(Helper&& helper, std::promise<bool>&& callResult)
  {
    callResult.set_value( helper.parent->Awaiter(helper.ms, helper.predicate ));
  }
};