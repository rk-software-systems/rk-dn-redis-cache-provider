using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RKSoftware.Packages.Caching.Contract;

namespace RKSoftware.Packages.Caching.Stream.Converter
{
    public class StreamConverter : IObjectToTextConverter
    {
        public T FromString<T>(string data)
        {
            throw new NotImplementedException();
        }

        public string ToString<T>(T obj)
        {
            throw new NotImplementedException();
        }
    }
}
