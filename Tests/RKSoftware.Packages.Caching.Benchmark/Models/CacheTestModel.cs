using System.Security.Cryptography;

namespace RKSoftware.Packages.Caching.Benchmark
{
	public class CacheTestModel
	{
		public int MyIntProperty { get; set; }

#pragma warning disable CS8618
		public string MyStringProperty { get; set; }
#pragma warning restore CS8618

#pragma warning disable CS8618
		public CacheTestModel MyObjectProperty { get; set; }
#pragma warning restore CS8618


		public bool Equals(CacheTestModel obj)
		{
			return this != null &&
				obj != null &&
				this.MyIntProperty == obj.MyIntProperty &&
				this.MyStringProperty == obj.MyStringProperty &&
				((this.MyObjectProperty == null && obj.MyObjectProperty == null) ||
#pragma warning disable CS8602
					(this.MyObjectProperty.Equals(obj.MyObjectProperty)));
#pragma warning restore CS8602
		}

		public static string TestKey => nameof(CacheTestModel);

		public static CacheTestModel TestModel =>
			new CacheTestModel
			{
				MyIntProperty = 1,
				MyStringProperty = GetUniqueRandomString(10000),
				MyObjectProperty = new CacheTestModel
				{
					MyIntProperty = 2,
					MyStringProperty = GetUniqueRandomString(10000)
				}
			};

		private static string GetUniqueRandomString(int string_length)
		{
#pragma warning disable SYSLIB0023
			using var rng = new RNGCryptoServiceProvider();
#pragma warning restore SYSLIB0023

			var bit_count = (string_length * 6);
			var byte_count = ((bit_count + 7) / 8); // rounded up
			var bytes = new byte[byte_count];
			rng.GetBytes(bytes);
			return Convert.ToBase64String(bytes);
		}
	}
}