namespace RKSoftware.Packages.Caching.Contract
{
    /// <summary>
    /// This service is used to convert objects that are being stored as string to CLR object
    /// </summary>
    public interface IObjectToTextConverter
    {
        /// <summary>
        /// Convert object to string
        /// </summary>
        /// <typeparam name="T">Type of target object</typeparam>
        /// <param name="obj">Target object value</param>
        /// <returns>String representation of the target object</returns>
        string ToString<T>(T obj);

        /// <summary>
        /// Convert object from string
        /// </summary>
        /// <typeparam name="T">Type of destination object</typeparam>
        /// <param name="data">Object string representation</param>
        /// <returns>Converted object value</returns>
        T FromString<T>(string data);
    }
}
