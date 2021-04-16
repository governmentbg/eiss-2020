using IOWebApplication.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface INewsService
    {
        NewsViewModel GetById(int id);
        bool SaveNews(NewsViewModel model, string authorId);
        IQueryable<NewsViewModel> News_Select();
        NewsViewModel GetLatest(string userId = null);
        void SetAsRead(int id, string userId);
        int GetUnreadNewsCount(string userId);
        LatestNewsViewModel GetLastNews(string userId);
    }
}
