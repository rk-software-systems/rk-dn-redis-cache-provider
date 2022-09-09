namespace RKSoftware.Packages.Caching.Tests.Models
{
    public class CacheTestModel
    {
        public int MyIntProperty { get; set; }

        public string MyStringProperty { get; set; }

        public CacheTestModel MyObjectProperty { get; set; }



        public bool Equals(CacheTestModel obj)
        {
            return this != null &&
                   obj != null &&
                   this.MyIntProperty == obj.MyIntProperty &&
                   this.MyStringProperty == obj.MyStringProperty &&
                  ((this.MyObjectProperty == null && obj.MyObjectProperty == null) ||
                  (this.MyObjectProperty.Equals(obj.MyObjectProperty)));
        }

        public static string TestKey => nameof(CacheTestModel);

        public static CacheTestModel TestModel => new CacheTestModel
        {
            MyIntProperty = 1,
            MyStringProperty = "str1",
            MyObjectProperty = new CacheTestModel
            {
                MyIntProperty = 2,
                MyStringProperty = "str2"
            }
        };

    }
}
