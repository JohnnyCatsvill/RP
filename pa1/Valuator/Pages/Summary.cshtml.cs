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
    public class SummaryModel : PageModel
    {
        private readonly ILogger<SummaryModel> _logger;
        private readonly Redis db = new Redis();
        public SummaryModel(ILogger<SummaryModel> logger)
        {
            _logger = logger;
        }

        public double Rank { get; set; }
        public double Similarity { get; set; }

        public void OnGet(string id)
        {
            _logger.LogDebug(id);

            //TODO: проинициализировать свойства Rank и Similarity сохранёнными в БД значениями
            Rank = Convert.ToDouble(db.Load(RankName + id));
            Similarity = Convert.ToDouble(db.Load(SimilarityName + id));
        }
    }
}
