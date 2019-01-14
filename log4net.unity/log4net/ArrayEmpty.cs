namespace log4net
{
    internal static class ArrayEmpty<T>
    {
        public static readonly T[] Instance = new T[0];
    }
}