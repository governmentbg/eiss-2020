using IOWebApplication.Core.Models;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface INewsService : IBaseService
    {
        NewsViewModel GetById(int id);
        bool SaveNews(NewsViewModel model, string authorId);
        IQueryable<NewsViewModel> News_Select();
        NewsViewModel GetLatest(string userId = null);
        Task SetAsRead(int id, string userId);
        int GetUnreadNewsCount(string userId);
        LatestNewsViewModel GetLastNews(string userId);
    }
}
