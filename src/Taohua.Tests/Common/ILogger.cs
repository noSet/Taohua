namespace Taohua.Tests.Common
{
    public interface ILogger<T>
    {
        string Log(string log);
    }

    public class Logger<T> : ILogger<T>
    {
        string ILogger<T>.Log(string log)
        {
            return typeof(T).Name + log;
        }
    }
}
