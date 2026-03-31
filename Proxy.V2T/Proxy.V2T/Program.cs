// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Builder;
using Proxy.EISS.Services;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureBuilder();

builder.Services.AddAppDbContext(builder.Configuration);
builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();

app.ConfigureApplication(builder.Configuration);

app.Run();

