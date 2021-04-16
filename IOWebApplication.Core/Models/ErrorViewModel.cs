using System;

namespace IOWebApplication.Core.Models
{
    public class ErrorViewModel
    {
        public string Title { get; set; }
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string Message { get; set; }
        public string InnerException { get; set; }

        public ErrorViewModel()
        {
            Title = "Грешка";
        }
    }
}