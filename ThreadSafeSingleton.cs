using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Demo1
{
    public class ThreadSafeSingleton<T> where T : new()
    {
        private static T mInstance;
        private static object lockObject = new object();

        public static T Instance
        {
            get
            {
                if (ThreadSafeSingleton<T>.mInstance == null)
                {
                    lock (lockObject)
                    {
                        if (ThreadSafeSingleton<T>.mInstance == null)
                        {
                            T local = default(T);
                            ThreadSafeSingleton<T>.mInstance = (local == null) ? Activator.CreateInstance<T>() : default(T);
                        }
                    }
                }
                return ThreadSafeSingleton<T>.mInstance;
            }
        }
    }
}

