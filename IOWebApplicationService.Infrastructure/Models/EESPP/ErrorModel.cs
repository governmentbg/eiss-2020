using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplicationService.Infrastructure.Models.EESPP
{
    public class ErrorModel
    {
        /// <summary>
        /// Код за грешка
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Съобщение на грешката
        /// </summary>
        public string ErrorDetails { get; set; }
    }
}
