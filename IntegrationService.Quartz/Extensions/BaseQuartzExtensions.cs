using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.AspNetCore;
using System;
using System.Collections.Generic;

namespace IntegrationService.Quartz
{
    public class QuartzJobInfo
    {
        public string Description { get; set; }
        public string TypeName { get; set; }
        public string CronExpression { get; set; }
        public int? FetchCount { get; set; }
        public bool Disabled { get; set; }
    }
    public static class BaseQuartzExtensions
    {
        public static void AddQuartConfiguration(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddQuartz(q =>
            {
                q.ConfigureQuartzServices(Configuration);
            });

            // ASP.NET Core hosting
            services.AddQuartzServer(options =>
            {
                // when shutting down we want jobs to complete gracefully
                options.AwaitApplicationStarted = true;
                options.WaitForJobsToComplete = true;
            });
        }


        static void ConfigureQuartzServices(this IServiceCollectionQuartzConfigurator service, IConfiguration Configuration)
        {
            

            service.UseMicrosoftDependencyInjectionJobFactory();

            var quartzJobs = Configuration.GetSection("QuartzJobs").Get<List<QuartzJobInfo>>();
            string jobGroup = "statGroup";

            foreach (var qJob in quartzJobs)
            {
                if (qJob.Disabled)
                {
                    continue;
                }
                try
                {
                    Type jobType = Type.GetType(qJob.TypeName);
                    var job1Key = new JobKey(Guid.NewGuid().ToString(), jobGroup);
                    service.AddJob(jobType, job1Key, a =>
                    {
                        a.WithDescription(qJob.Description);
                        if (qJob.FetchCount > 0)
                        {
                            a.UsingJobData("fetchCount", qJob.FetchCount.Value);
                        }
                    });

                    var cronsSchedules = qJob.CronExpression.Split('|', StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < cronsSchedules.Length; i++)
                    {
                        string schedule = cronsSchedules[i];
                        service.AddTrigger(t => t
                           .ForJob(job1Key)
                           .WithDescription($"cron {i + 1}")
                           .StartNow()
                           .WithCronSchedule(schedule)
                        );
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"QUARTZ init Error!, job:{qJob.TypeName}; Exception:{ex.Message}");
                }
            }
        }

        /*
"QuartzJobs": [
    {
      "Description": "ЕЕСПП",
      "TypeName": "QuartzTest.MockJob",
      "CronExpression": "0 0 8/2 ? * * *",
      "ExpressionInfo": "Всеки втори час започвайки от 08:00, всеки ден"
    },
    {
      "Description": "СИСМА",
      "TypeName": "QuartzTest.Mock2Job",
      "CronExpression": "0/2 * * ? * * *"
    }
  ]

        */
    }
}
