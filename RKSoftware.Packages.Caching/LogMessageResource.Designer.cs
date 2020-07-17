﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RKSoftware.Packages.Caching {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class LogMessageResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal LogMessageResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("RKSoftware.Packages.Caching.LogMessageResource", typeof(LogMessageResource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Redis connection exception during removing. PartOfKey: {0}, ProjectName: {1}.
        /// </summary>
        internal static string RedisBulkPartialReset {
            get {
                return ResourceManager.GetString("RedisBulkPartialReset", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Redis connection exception during removing. Keys: {0}.
        /// </summary>
        internal static string RedisBulkResetError {
            get {
                return ResourceManager.GetString("RedisBulkResetError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Provide at least one key to be deleted.
        /// </summary>
        internal static string RedisBulkResetNoKeys {
            get {
                return ResourceManager.GetString("RedisBulkResetNoKeys", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Redis connection exception during getting. Key: {key}.
        /// </summary>
        internal static string RedisConnectionError {
            get {
                return ResourceManager.GetString("RedisConnectionError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Opening Redis connection.
        /// </summary>
        internal static string RedisConnectionOpenening {
            get {
                return ResourceManager.GetString("RedisConnectionOpenening", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to get object from Redis. Key: {key}.
        /// </summary>
        internal static string RedisGetObjectError {
            get {
                return ResourceManager.GetString("RedisGetObjectError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Object not found in cache. Key: {key}.
        /// </summary>
        internal static string RedisObjectNotFound {
            get {
                return ResourceManager.GetString("RedisObjectNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to reset value from Redis. Redis key: {RedisKey}.
        /// </summary>
        internal static string RedisRemoveObjectError {
            get {
                return ResourceManager.GetString("RedisRemoveObjectError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to set value in Redis. Key: {key}.
        /// </summary>
        internal static string RedisSetObjectError {
            get {
                return ResourceManager.GetString("RedisSetObjectError", resourceCulture);
            }
        }
    }
}
