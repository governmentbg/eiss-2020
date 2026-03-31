// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.Cdn
{
    public class CdnConfigVM
    {
        public string FileDbName { get; set; }
        public ConnectionStringsModel ConnectionStrings { get; set; }
    }

    public class ConnectionStringsModel
    {
        public string MongoDbConnection { get; set; }
    }
}
