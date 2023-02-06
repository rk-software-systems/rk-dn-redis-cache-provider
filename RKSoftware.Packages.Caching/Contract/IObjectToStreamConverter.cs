using System.IO;
using System.Threading.Tasks;

namespace RKSoftware.Packages.Caching.Contract
{
    public interface IObjectToStreamConverter
    {
        T FromStream<T>(Stream data);

        Task<T> FromStreamAsync<T>(Stream data);

        byte[] ToBytes<T>(T obj);
    }
}
