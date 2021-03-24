using Common.Storage;
using System.Threading.Tasks;

namespace RankCalculator
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var calculator = new RankCalculator(new Redis());
            await Task.Delay(-1);
        }
    }
}