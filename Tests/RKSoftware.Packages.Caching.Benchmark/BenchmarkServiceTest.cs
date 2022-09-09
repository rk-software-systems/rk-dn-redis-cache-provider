
using RKSoftware.Packages.Caching.Benchmark;

Console.WriteLine("Hello, BenchmarkDotNet!\n");


var source = new SourceBenchmarkTests();

source.RunGetCachedObjectBenchmark();


