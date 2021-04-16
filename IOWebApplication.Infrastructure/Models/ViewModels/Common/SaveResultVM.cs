using IOWebApplication.Infrastructure.Constants;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class SaveResultVM
    {
        public bool Result { get; set; }

        public string ErrorMessage { get; set; }
        public string SaveMethod { get; set; }

        public SaveResultVM()
        {
            Result = false;
        }

        public SaveResultVM(bool result, string errorMessage = "", string saveMethod = "")
        {
            Result = result;
            ErrorMessage = errorMessage;
            if (!result && string.IsNullOrEmpty(errorMessage))
            {
                ErrorMessage = "Проблем по време на запис.";
            }
            SaveMethod = saveMethod;
        }
    }
}
