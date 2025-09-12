using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using YesterdayNews.Data;
using YesterdayNews.Models.Db;
using YesterdayNews.Models.ViewModels;

namespace YesterdayNews.Utils
{
    public interface IDbInitalizlier
    {
        public  Task Initialize();

    }
}
