﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEPUutilitiesTests
{
    public static class Helper
    {
        public static void Test(string testName, Func<int, float> function, int benchmarkIterations = 100000000, int warmupIterations = 8192)
        {
            GC.Collect();
            function(warmupIterations);
            var start = Stopwatch.GetTimestamp();
            var accumulator = function(benchmarkIterations);
            var end = Stopwatch.GetTimestamp();
            Console.WriteLine($"{testName} time: {(end - start) / (double)Stopwatch.Frequency}");
        }
    }
}
