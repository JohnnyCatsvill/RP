using Common.Storage;
using System.Threading.Tasks;

namespace DisturbedStorage
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var calculator = new DistirbutedStorage(new Redis());
            calculator.Run();
        }
    }
}