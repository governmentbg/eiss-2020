// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Attributes;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class WorkTaskEditVM
    {
        public long Id { get; set; }
        public long? ParentTaskId { get; set; }
        public string SourceInfo { get; set; }
        public int SourceType { get; set; }
        public long SourceId { get; set; }
        public long? SubSourceId { get; set; }

        [Display(Name = "Вид задача")]
        [Required(ErrorMessage ="Изберете {0}.")]
        public int TaskTypeId { get; set; }

        [Display(Name = "Вид задача")]
        public string TaskTypeName { get; set; }

        [Display(Name = "Начин на изпълнение")]
        [IORequired]
        public int? TaskExecutionId { get; set; }
        [Display(Name = "Изберете потребител")]
        [IORequired]
        public string UserId { get; set; }
        [Display(Name = "Изберете структура")]
        [IORequired]
        public int? CourtOrganizationId { get; set; }

        [Display(Name = "Срок за изпълнение")]
        public DateTime? DateEnd { get; set; }
        [Display(Name = "Описание")]
        public string DescriptionCreated { get; set; }

        [Display(Name = "Забележка за пренасочването")]
        public string DescriptionRedirect { get; set; }

        public void ToEntity(WorkTask entity)
        {
            entity = entity ?? new WorkTask();
            entity.SourceType = this.SourceType;
            entity.SourceId = this.SourceId;
            entity.SubSourceId = this.SubSourceId;
            entity.DateEnd = this.DateEnd;
            entity.TaskTypeId = this.TaskTypeId;
            entity.TaskExecutionId = this.TaskExecutionId;
            switch (entity.TaskExecutionId)
            {
                case WorkTaskConstants.TaskExecution.ByUser:
                    entity.UserId = this.UserId;
                    entity.CourtOrganizationId = null;
                    break;
                case WorkTaskConstants.TaskExecution.ByOrganization:
                    entity.UserId = null;
                    entity.CourtOrganizationId = this.CourtOrganizationId;
                    break;
            }
            entity.DescriptionCreated = this.DescriptionCreated;
            entity.ParentTaskId = this.ParentTaskId;
        }
    }
}
