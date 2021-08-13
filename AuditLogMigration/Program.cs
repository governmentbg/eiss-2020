using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Audit;
using System.Linq;
using Audit.Core;
using Newtonsoft.Json;
using IOWebApplication.Core.Models;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using System.Threading;

namespace AuditLogMigration
{
    class Program
    {
        static void Main(string[] args)
        {
            var Configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json")
                .SetBasePath(Directory.GetCurrentDirectory())
                //.AddJsonFile("hosting.json", optional: true)
                //.AddCommandLine(args)
                .Build();

            string connStr = Configuration.GetConnectionString("DefaultConnection");
            //setup our DI
            var services = new ServiceCollection()
                .AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseNpgsql(connStr, opt =>
                    {
                        opt.EnableRetryOnFailure(
                                maxRetryCount: 2,
                                maxRetryDelay: TimeSpan.FromSeconds(10),
                                errorCodesToAdd: null
                        );
                        opt.CommandTimeout(60);
                    });
                    //options
                }
                )
                .AddScoped<IRepository, Repository>()
                .BuildServiceProvider();

            int retryAttempt = 1;
            do
            {
                Console.WriteLine($"pass {retryAttempt}");
                try
                {
                    var repo = services.GetService<IRepository>();
                    Thread.Sleep(1000);
                    Migrate(repo, connStr);
                    retryAttempt = 100;
                }
                catch (Exception ex)
                {
                    retryAttempt++;
                    Console.WriteLine(ex.Message);
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine(ex.InnerException.Message);
                    }
                }
            } while (retryAttempt <= 5);
            Console.WriteLine("Execution terminated.");
            Console.ReadLine();
        }

        static void Migrate(IRepository repo, string connStr)
        {
            int fetchCount = 100;
            Console.WriteLine($"Started:{DateTime.Now}; Fetch count:{fetchCount}");
            bool canLoop = false;
            Stopwatch swDB = new Stopwatch();
            Stopwatch sw = new Stopwatch();
            var allUsers = repo.AllReadonly<ApplicationUser>().Select(x => new { x.Id, x.UserName }).ToList();
            int entitiesLoaded = 0;
            do
            {

                sw.Reset();
                sw.Start();
                if (entitiesLoaded > 1000)
                {
                    repo.RefreshDbContext(connStr);
                    entitiesLoaded = 0;
                }
                var batch = repo.All<AuditLog>()
                                .Where(x => x.UserId == null)                                
                                //.Where(x => x.Data != "{}")
                                //.OrderByDescending(x => x.Id)
                                .Take(fetchCount)
                                .ToList();

                entitiesLoaded += batch.Count();

                foreach (var logItem in batch)
                {
                    try
                    {


                        AuditEvent ai = AuditEvent.FromJson(logItem.Data);
                        if (ai.CustomFields.ContainsKey("currentContext"))
                        {
                            string ctx = ((JObject)ai.CustomFields["currentContext"]).ToString();
                            ContextInfoModel currentContext = JsonConvert.DeserializeObject<ContextInfoModel>(ctx);
                            logItem.Operation = currentContext.Operation;
                            logItem.ObjectType = currentContext.ObjectType;
                            logItem.ObjectInfo = currentContext.ObjectInfo;
                            logItem.BaseObject = currentContext.BaseObject;
                            logItem.CourtId = currentContext.CourtId;
                            logItem.UserId = currentContext.UserId;
                        }
                        if (ai.CustomFields.ContainsKey("currentIp"))
                        {
                            logItem.ClientIP = (string)ai.CustomFields["currentIp"];
                        }
                        logItem.RequestUrl = ai.EventType;
                        if (ai.CustomFields.ContainsKey("Action"))
                        {
                            //GetMvcAuditAction

                            string ctx = ((JObject)ai.CustomFields["Action"]).ToString();
                            ActionVM _action = JsonConvert.DeserializeObject<ActionVM>(ctx);
                            logItem.RequestUrl = _action.RequestUrl.Replace("https://eiss-integration.uslugi.io", "").Replace("https://eiss.local:8080", "").Replace("https://www.eiss.justice.bg", "");
                            logItem.Method = _action.HttpMethod;
                            logItem.FullName = _action.ResponseBody?.Type;
                            if (string.IsNullOrEmpty(logItem.UserId) && _action.UserName != null)
                            {
                                logItem.UserId = allUsers.FirstOrDefault(x => x.UserName.ToLower() == _action.UserName.ToLower())?.Id;
                            }
                        }

                        if (!string.IsNullOrEmpty(logItem.UserId))
                        {
                            if (!allUsers.Where(x => x.Id == logItem.UserId).Any())
                            {
                                logItem.UserId = null;
                            }
                        }
                        if (
                            string.IsNullOrEmpty(logItem.UserId)
                            || logItem.FullName?.ToLower().Contains("json") == true
                            || logItem.FullName?.ToLower().Contains("partial") == true)
                        {
                            repo.Delete(logItem);
                        }
                        else
                        {
                            logItem.Data = "{}";
                            repo.Update(logItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        logItem.RequestUrl = ex.Message;
                    }

                }

                if (batch.Any())
                {
                    swDB.Reset();
                    swDB.Start();
                    repo.SaveChanges();
                    swDB.Stop();
                    sw.Stop();
                    Console.WriteLine($"({DateTime.Now:dd HH:mm:ss})Elapsed:{sw.Elapsed.TotalSeconds:00.000} sec.; First ID = {batch.FirstOrDefault()?.Id}");
                    //Console.WriteLine($"Elapsed:{sw.Elapsed.TotalSeconds:N3} sec. DB:{swDB.Elapsed.TotalSeconds:N3} sec.; First ID = {batch.FirstOrDefault()?.Id}");
                    canLoop = true;
                }
                else
                {
                    canLoop = false;
                }

            } while (canLoop);
            Console.WriteLine($"Finished:{DateTime.Now}");
        }
    }
}
