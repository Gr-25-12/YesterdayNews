using OutsourcedNews.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutsourcedNews.Services
{
     public interface INewsApiService
    {
         Task<List<ArticleDto>> GetTopNewsAsync(string country);
    }
}
