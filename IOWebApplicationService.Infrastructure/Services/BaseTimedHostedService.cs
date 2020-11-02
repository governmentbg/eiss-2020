// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplicationService.Infrastructure.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NPOI.OpenXmlFormats.Spreadsheet;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace IOWebApplicationService.Infrastructure.Services
{
    /// <summary>
    /// Имплементация на универсална услуга с Таймер
    /// (Windows Service / Linux Systemd Service)
    /// </summary>
    public class BaseTimedHostedService : IHostedService, IDisposable
    {
        public IApplicationLifetime appLifetime;
        public ILogger<BaseTimedHostedService> logger;
        public IHostingEnvironment environment;
        public IConfiguration configuration;
        private System.Timers.Timer _timer = null;
        public double interval;
        public int stopServiceTimeout;
        public int fetchCount;
        public IServiceProvider serviceProvider;
        private bool processCompleted;


        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("StartAsync method called.");

            this.appLifetime.ApplicationStarted.Register(OnStarted);
            this.appLifetime.ApplicationStopping.Register(OnStopping);
            this.appLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;

        }

        private void OnStarted()
        {
            this.logger.LogInformation("OnStarted method called.");

            try
            {
                _timer = new System.Timers.Timer();
                _timer.Interval = interval;

                // Закачаме функцията, която се вика при изтичане на таймера
                _timer.Elapsed += Timer_Elapsed;
                _timer.Start();
                processCompleted = true;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
            }
        }

        private void OnStopping()
        {
            this.logger.LogInformation("OnStopping method called.");
            int seconds = 0;

            while (!processCompleted)
            {
                Thread.Sleep(1000);
                seconds++;

                if (stopServiceTimeout > 0 && (seconds * 1000) > stopServiceTimeout)
                {
                    break;
                }
            }

            try
            {
                _timer?.Stop();
                _timer?.Close();
                _timer?.Dispose();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
            }

        }

        private void OnStopped()
        {
            this.logger.LogInformation("OnStopped method called.");
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("StopAsync method called.");

            return Task.CompletedTask;
        }

        protected void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                // Спираме таймера за да си осигурим една единствена инстанция на work-ъра
                _timer.Stop();
                // рестартиране на всеки ... в милисекунди
                _timer.Interval = interval;


                processCompleted = false;

                TimerElapsedAction();

                processCompleted = true;

            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
            }
            finally
            {
                // Включваме таймера независимо от резултата
                _timer.Start();
            }
        }

        public virtual void TimerElapsedAction() { }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }
        }
    }
}