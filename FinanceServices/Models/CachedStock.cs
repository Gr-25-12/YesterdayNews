using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FinanceServices.Models
{
    public  class CachedStock
    {
        public string Symbol { get; set; }
        public string DisplayName { get; set; }
        public string Exchange { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal ClosingPrice { get; set; }

        // New property for logos/links
        public string WebsiteDomain { get; set; }

        public string LogoUrl { get; set; }  // new property for Clearbit logo

        public decimal Change => ClosingPrice != 0 ? CurrentPrice - ClosingPrice : 0;
        public decimal PercentageChange => ClosingPrice != 0
            ? (CurrentPrice - ClosingPrice) / ClosingPrice * 100
            : 0;
    }
}
