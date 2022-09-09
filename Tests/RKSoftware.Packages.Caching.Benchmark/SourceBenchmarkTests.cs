using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using RKSoftware.Packages.Caching.Contract;

namespace RKSoftware.Packages.Caching.Benchmark;

public class SourceBenchmarkTests
{
	#region fields

	private static ICacheService? _cacheService;
	private static readonly string key = CacheTestModel.TestKey;

	#endregion


	[GlobalSetup]
	public void GetSourceFile()
	{
		using var scope = Initialization.CreateScope();

		if (scope.ServiceProvider is not null)
		{
			_cacheService = scope.ServiceProvider.GetService<ICacheService>() ?? _cacheService;
		}

		_cacheService?.SetCachedObject(key, new CacheTestModel());
	}

	public class GetCachedObjectBenchmark
	{
		[Benchmark]
		public CacheTestModel? result1() => _cacheService?.GetCachedObject<CacheTestModel>(key);

		[Benchmark]
		public CacheTestModel? result2() => _cacheService?.GetCachedObject<CacheTestModel>(key);

	}


	public void RunGetCachedObjectBenchmark()
	{
		BenchmarkRunner.Run<GetCachedObjectBenchmark>(ManualConfig
		                             .Create(DefaultConfig.Instance)
		                             .WithOptions(ConfigOptions.JoinSummary)
		                             .WithOptions(ConfigOptions.DisableLogFile));
	}
}