using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using Common;
using Common.Storage;
using Common.Structures;
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

        public IActionResult OnPost(string text, string country)
        {
            _logger.LogDebug(text);

            string id = Guid.NewGuid().ToString();

            _storage.SaveIdToRegion(id, country);

            string similarityKey = Constants.SIMILARITY_NAME + id;
            {
                int similarity = 0;
                var keys = _storage.GetKeys(Constants.TEXT_NAME, country);
                foreach(var key in keys)
                {
                    string key_id = key.Substring(key.IndexOf("-")+1);
                    string key_obj = key.Substring(0, key.IndexOf("-")+1);
                    if (_storage.Load(key_obj, key_id) == text)
                    {
                        similarity = 1;
                        break;
                    }
                }
                _storage.Save(Constants.SIMILARITY_NAME, id, similarity.ToString());

                LoggerData loggerData = new("similarity_calculated", id, similarity.ToString());
                string dataToSend = JsonSerializer.Serialize(loggerData);
       
                _broker.Publish(Constants.BROKER_CHANNEL_EVENTS_LOGGER, Encoding.UTF8.GetBytes(dataToSend));
            }

            string textKey = Constants.TEXT_NAME + id;
            _storage.Save(Constants.TEXT_NAME, id, text);

            string rankKey = Constants.RANK_NAME + id;
            _broker.Publish(Constants.BROKER_CHANNEL_FOR_RANK_CALCULATION, Encoding.UTF8.GetBytes(id));


            while(_storage.Load(Constants.RANK_NAME, id) == null)
            {
                Thread.Sleep(100);
                return Redirect($"summary?id={id}");
            }

            return Redirect($"summary?id={id}");
        }
    }
}
