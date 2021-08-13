// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.IndexService
{
    public class IndexRequestModel
    {
        /// <summary> 
        /// add;delete
        /// </summary>
        /// <value>The method.</value>
        public string Method { get; set; }

        /// <summary>
        /// CaseSessionActId
        /// </summary>
        /// <value>The file identifier.</value>
        public int ActId { get; set; }

        /// <summary>
        /// Integration Key GUID
        /// </summary>
        /// <value>The act key.</value>
        public string  IntegrationActKey { get; set; }
    }
}
