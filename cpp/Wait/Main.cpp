// Wait.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include "wait.h"

int main()
{
  // display time taken
  //  https://en.cppreference.com/w/cpp/chrono/duration/duration_cast

  auto t1 = std::chrono::high_resolution_clock::now();
  Wait::Delay(1000);
  auto t2 = std::chrono::high_resolution_clock::now();

  Wait::SpinUntil([&] { 
    auto now = std::chrono::high_resolution_clock::now();
    std::chrono::duration<double, std::milli> inner = now - t2;
    return inner.count() >= 29000;
  }, 30000 );
  auto t3 = std::chrono::high_resolution_clock::now();

  std::chrono::duration<double, std::milli> ms1 = t2 - t1;
  std::cout << "Wait::Delay(ms) took " << ms1.count() << " ms" << std::endl;
  std::chrono::duration<double, std::milli> ms2 = t3 - t2;
  std::cout << "Wait::Until(ms) took " << ms2.count() << " ms" << std::endl;

  std::cout << "Bye!\n";
}