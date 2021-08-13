using IOWebApplication.Infrastructure.Data.Models.Cases;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseNotificationLinkVM
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int PersonRelId { get; set; }
        public int? PersonSecondRelId { get; set; }
        public int PersonCaseId { get; set; }
        public int PersonCaseRelId { get; set; }
        public int? PersonCaseSecondRelId { get; set; }
        public string PersonGuid { get; set; }
        public string PersonRelGuid { get; set; }
        public int LinkDirectionId { get; set; }
        public int? LinkDirectionSecondId { get; set; }
        public string PersonName { get; set; }
        public string PersonRelName { get; set; }
        public string PersonSecondRelName { get; set; }
        public string PersonRole { get; set; }
        public string PersonRelRole { get; set; }
        public string PersonSecondRelRole { get; set; }
        public string LinkTemplate { get; set; }
        public string LinkTemplateVks { get; set; }
        public string SecondLinkTemplate { get; set; }
        public string SecondLinkTemplateVks { get; set; }
        public string Label { get; set; }
        public string LabelWithoutFirstPerson { get; set; }
        public string LabelWithoutSecondRel { get; set; }
        public bool isXFirst { get; set; }
    }
}
