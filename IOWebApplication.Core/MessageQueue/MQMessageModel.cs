// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Core.MessageQueue
{
    public class MQMessageModel
    {
        public string ClientId { get; set; }

        public string Method { get; set; }

        public string Params { get; set; }
    }
}
