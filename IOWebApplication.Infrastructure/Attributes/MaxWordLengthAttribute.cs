using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace IOWebApplication.Infrastructure.Attributes
{
    public class MaxWordLengthAttribute : ValidationAttribute
    {
        private int maxValidLength { get; set; }
        public MaxWordLengthAttribute(int maxValidLength = 20)
        {
            this.maxValidLength = maxValidLength;
        }
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            if (string.IsNullOrEmpty(value.ToString()))
            {
                return true;
            }
            char[] separators = { ',', ' ', '.' };
            string[] array = value.ToString().Split(separators);
            if (array.Max().Length > maxValidLength)
            {
                return false;
            }

            return true;
        }
    }
}
