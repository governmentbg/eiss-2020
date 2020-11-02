// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Newtonsoft.Json;

namespace IOWebApplication.Core.Models
{
    /// <summary>
    /// Информация за контекст на извършено действие в ЕИСС
    /// </summary>
    public class CurrentContextModel : IAccessControl
    {
        public ContextInfoModel Info { get; set; }

        //----------------Права за достъп-----------------------------
        /// <summary>
        /// Има право на достъп
        /// </summary>
        public bool CanAccess { get; set; }

        private bool canChange { get; set; }
        /// <summary>
        /// Има право за редакция
        /// </summary>
        //[JsonIgnore]
        public bool CanChange
        {
            get
            {
                return CanAccess && canChange;
            }
            set
            {
                canChange = value;
            }
        }

        /// <summary>
        /// Има пълни права
        /// </summary>
        //[JsonIgnore]
        public bool CanChangeFull { get; set; }

        public string LastController { get; set; }

        public bool IsRead { get; set; }
        public string CdnFileEditMode
        {
            get
            {
                if (CanChange)
                {
                    return "all";
                }
                else
                {
                    return "none";
                }
            }
        }

        public CurrentContextModel()
        {
            IsRead = false;
        }
        public CurrentContextModel(int sourceType, object sourceId, string operation) : base()
        {
            canChange = true;
            IsRead = true;
            CanChangeFull = false;
            Info = new ContextInfoModel()
            {
                SourceType = sourceType,
                SourceId = (sourceId ?? "").ToString(),
                Operation = operation,
                ObjectType = SourceTypeSelectVM.GetSourceTypeName(sourceType)
            };

        }
    }
}
