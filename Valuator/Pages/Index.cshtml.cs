using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using static Valuator.Constants;

namespace Valuator.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly Redis db = new Redis();

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        public IActionResult OnPost(string text)
        {
            _logger.LogDebug(text);

            string id = Guid.NewGuid().ToString();

            string rankKey = RankName + id;
            int alphabeticLetters = 0;
            foreach (char ch in text)
            {
                if (Char.IsLetter(ch))
                {
                    alphabeticLetters++;
                }
            }
            double rank = (double)alphabeticLetters / (double)text.Length;
            db.Save(rankKey, rank.ToString());


            string similarityKey = SimilarityName + id;
            int similarity = 0;
            var keys = db.GetKeys(TextName);
            foreach(var key in keys)
            {
                if (db.Load(key) == text)
                {
                    similarity = 1;
                    break;
                }
            }
            db.Save(similarityKey, similarity.ToString());


            string textKey = TextName + id;
            db.Save(textKey, text);

            return Redirect($"summary?id={id}");
        }
    }
}
