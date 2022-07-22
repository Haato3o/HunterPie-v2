using System.Threading.Tasks;

namespace HunterPie.Core.Logger
{
    public interface ILogger
    {
        public Task Debug(string message);
        public Task Debug(string format, params object[] args);

        public Task Info(string message);
        public Task Info(string format, params object[] args);

        public Task Warn(string message);
        public Task Warn(string format, params object[] args);

        public Task Native(string message);
        public Task Native(string format, params object[] args);

        public Task Error(string message);
        public Task Error(string format, params object[] args);

        public Task Benchmark(string message);
        public Task Benchmark(string format, params object[] args);
    }
}
