using DnsClient.Internal;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class NewsService : BaseService, INewsService
    {
        public NewsService(
            ILogger<NewsService> _logger,
            IRepository _repo,
            IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }

        public NewsViewModel GetById(int id)
        {
            return repo.All<News>()
                .Where(n => n.Id == id)
                .Include(n => n.Author.LawUnit)
                .Select(n => new NewsViewModel()
                {
                    Author = n.Author.LawUnit.FullName,
                    Id = n.Id,
                    Content = n.Content,
                    PublishDate = n.PublishDate,
                    Title = n.Title,
                    IsUnread = !n.NewsUsers.Any(u => u.UserId == userContext.UserId)
                })
                .FirstOrDefault();
        }

        public LatestNewsViewModel GetLastNews(string userId)
        {
            var latestNews = GetLatest(userId);
            var unreadNews = GetUnreadNewsCount(userId);

            return new LatestNewsViewModel()
            {
                News = latestNews,
                UnreadNews = unreadNews,
                IsUnreadNews = unreadNews > 0
            };
        }

        public NewsViewModel GetLatest(string userId = null)
        {
            NewsViewModel result = new NewsViewModel();

            var newsUnRead = repo.AllReadonly<News>()
                           .Include(n => n.Author.LawUnit)
                           .Include(n => n.NewsUsers)
                           .Where(n => !n.NewsUsers.Any(u => u.UserId == userId))
                           .OrderByDescending(n => n.PublishDate)
                           .FirstOrDefault();

            var newsRead = repo.AllReadonly<News>()
                           .Include(n => n.Author.LawUnit)
                           .Include(n => n.NewsUsers)
                           .Where(n => n.NewsUsers.Any(u => u.UserId == userId))
                           .OrderByDescending(n => n.PublishDate)
                           .FirstOrDefault();

            if (newsUnRead != null)
            {
                result = new NewsViewModel() 
                {
                    Author = newsUnRead.Author.LawUnit.FullName,
                    Content = newsUnRead.Content,
                    Id = newsUnRead.Id,
                    PublishDate = newsUnRead.PublishDate,
                    Title = newsUnRead.Title,
                    IsUnread = true
                };
            }
            else
            {
                if (newsRead != null)
                {
                    result = new NewsViewModel()
                    {
                        Author = newsRead.Author.LawUnit.FullName,
                        Content = newsRead.Content,
                        Id = newsRead.Id,
                        PublishDate = newsRead.PublishDate,
                        Title = newsRead.Title,
                        IsUnread = false
                    };
                }
                else
                    return null;
            }

            return result;
        }

        public int GetUnreadNewsCount(string userId)
        {
            return repo.AllReadonly<News>()
                .Count(n => !n.NewsUsers.Any(u => u.UserId == userId));
        }

        public IQueryable<NewsViewModel> News_Select()
        {
            return repo.AllReadonly<News>()
                       .Include(x => x.Author.LawUnit)
                       .Select(x => new NewsViewModel()
                       {
                           Author = x.Author.LawUnit.FullName,
                           Content = x.Content,
                           Id = x.Id,
                           PublishDate = x.PublishDate,
                           Title = x.Title,
                           IsUnread = !x.NewsUsers.Any(u => u.UserId == userContext.UserId)
                       })
                       .AsQueryable();
        }

        public bool SaveNews(NewsViewModel model, string authorId)
        {
            bool result = false;
            News entity = null; 

            try
            {
                if (model.Id == 0)
                {
                    repo.Add(new News()
                    {
                        UserId = authorId,
                        Content = model.Content,
                        PublishDate = DateTime.Now,
                        Title = model.Title
                    });
                }
                else
                {
                    entity = repo.GetById<News>(model.Id);

                    entity.UserId = authorId;
                    entity.Title = model.Title;
                    entity.Content = model.Content;
                }

                repo.SaveChanges();

                if (entity != null)
                {
                    model.Id = entity.Id;
                }

                result = true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "NewsService/SaveNews");
            }

            return result;
        }

        public void SetAsRead(int id, string userId)
        {
            var entity = repo.GetById<News>(id);

            if (entity != null)
            {
                entity.NewsUsers.Add(new NewsUser() 
                {
                    UserId = userId
                });

                repo.SaveChanges();
            }
        }
    }
}
