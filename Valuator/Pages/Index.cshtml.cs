using System;
using System.Text;
using Common;
using Common.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NATS.Client;

namespace Valuator.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IStorage _storage;
        private static IConnection _broker;

        public IndexModel(ILogger<IndexModel> logger, IStorage storage)
        {
            _logger = logger;
            _storage = storage;
            _broker = new ConnectionFactory().CreateConnection();
        }

        public void OnGet() { }

        public IActionResult OnPost(string text)
        {
            _logger.LogDebug(text);

            string id = Guid.NewGuid().ToString();

            string similarityKey = Constants.SIMILARITY_NAME + id;
            //TODO: посчитать similarity и сохранить в БД по ключу similarityKey
            {
                int similarity = 0;
                var keys = _storage.GetKeys(Constants.TEXT_NAME);
                foreach(var key in keys)
                {
                    if (_storage.Load(key) == text)
                    {
                        similarity = 1;
                        break;
                    }
                }
                _storage.Save(similarityKey, similarity.ToString());
            }

            string textKey = Constants.TEXT_NAME + id;
            //TODO: сохранить в БД text по ключу textKey
            _storage.Save(textKey, text);

            string rankKey = Constants.RANK_NAME + id;
            //TODO: посчитать rank и сохранить в БД по ключу rankKey
            _broker.Publish(Constants.BROKER_CHANNEL, Encoding.UTF8.GetBytes(id));

            return Redirect($"summary?id={id}");
        }
    }
}
