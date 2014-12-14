using System;
using System.Reflection;

namespace MusicMinkAppLayer.Helpers
{
    /// <summary>
    /// Useful static utility functions
    /// </summary>
    public static class Utilities
    {
        public static bool IsSupportedFileType(string fileType)
        {
            return (fileType.ToLowerInvariant() == ".mp3" || fileType.ToLowerInvariant() == ".m4a" || fileType.ToLowerInvariant() == ".wma");
        }

        public static object GetDefault(Type t)
        {
            Type t1 = typeof(Utilities);

            MethodInfo m1 = t1.GetRuntimeMethod("GetDefaultGeneric", new Type[] {});

            MethodInfo m2 = m1.MakeGenericMethod(t);

            object o1 = m2.Invoke(null, null);

            return o1;
        }

        public static T GetDefaultGeneric<T>()
        {
            return default(T);
        }
    }
}
