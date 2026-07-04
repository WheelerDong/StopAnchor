namespace Singleton.Base
{
    public abstract class Singleton<T> where T : class, new() 
    {
        private static readonly object _lock = new object();
        private static T _instance;

        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    return _instance ?? (_instance = new T());
                }
            }
        }
    }
}