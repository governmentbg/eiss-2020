// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Migrations;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace IOWebApplication.Infrastructure.Services
{
    public interface ILazybleService<I>
        where I : class
    {
        I Service { get; }
    }

    public class LazybleService<I> : ILazybleService<I>
        where I : class
    {
        private I _instance;
        public I Service
        {
            get
            {
                if (_instance == null)
                {
                    _instance = _lazy.Value;
                }
                return _instance;
            }
        }

        private readonly Lazy<I> _lazy;
        public LazybleService(Lazy<I> lazy)
        {
            _lazy = lazy;
        }
    }

    public static class LazyExtensions
    {
        public static void AddLazybleService<Tint, Tclass>(this IServiceCollection services)
            where Tint : class
            where Tclass : class, Tint
        {
            services.AddScoped<Tint, Tclass>();
            services.AddTransient<ILazybleService<Tint>>(provider => new LazybleService<Tint>(new Lazy<Tint>(() => provider.GetRequiredService<Tint>())));
        }
    }
}
