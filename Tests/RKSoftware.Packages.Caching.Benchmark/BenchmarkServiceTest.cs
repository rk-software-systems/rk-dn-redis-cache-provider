//using RKSoftware.Packages.Caching.Benchmark;

//Console.WriteLine("Hello, BenchmarkDotNet!\n");

//var source = new SourceBenchmarkTests();

//source.RunGetCachedObjectBenchmark();

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.Extensions.Logging;

using RKSoftware.Packages.Caching.Benchmark;
using RKSoftware.Packages.Caching.Contract;
using RKSoftware.Packages.Caching.Converter.Mock;
using RKSoftware.Packages.Caching.Implementation;
using RKSoftware.Packages.Caching.Infrastructure;

namespace BenchmarkService
{
	internal class Program
	{
		private static string? _projectName => typeof(Initialization).Namespace;


		private static IConfigurationSection LoadConfiguration()
		{
			var builder = new ConfigurationBuilder()
			              .SetBasePath(Directory.GetCurrentDirectory())
			              .AddJsonFile("appsettings.json");

			var configuration = builder.Build();

			return configuration.GetSection(nameof(RedisCacheSettings));
		}

		
		public static void Main(string[] args)
		{
			var services = new ServiceCollection();

			var configuration = LoadConfiguration();
			services.UseRKSoftwareCache(configuration, _projectName)
			        .UseMockJsonTextConverter();

			var loggerMoq = new Mock<ILogger<CacheService>>();
			services.AddScoped<ILogger<CacheService>>(x => loggerMoq.Object);
			var loggerConnectionMoq = new Mock<ILogger<RedisConnectionProvider>>();
			services.AddScoped<ILogger<RedisConnectionProvider>>(x => loggerConnectionMoq.Object);

			var provider = services.BuildServiceProvider();
			var service = provider.GetRequiredService<ICacheService>();

			service.SetCachedObject("test", "valueTest");

			const string author = @"{'Person':[{'Speciality':'Archer','PWZ':432742,'Name':'Charlie','Surname':'Evans','Items':['Bow','Arrow']},{'Speciality':'Soldier','PWZ':432534879,'Name':'Harry','Surname':'Thomas','Items':['Gun','Knife']}],'Monster':[{'Name':'Papua','Skills':['Jump','SlowWalk']},{'Name':'Geot','Skills':['Run','Push']}]}";

			service.SetCachedStreamObject("key", author, 3600, false);

			var tt = service.GetCachedSteramObject<string>("key", false);
		}
	}
}