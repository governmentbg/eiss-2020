using System;
using System.Text.Json;

namespace IOWebApplication.Extensions
{
    public class DataTablesResponseDataNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {

            if (String.IsNullOrEmpty(name))
            {
                return name;
            }

            char[] nameChars = name.ToCharArray();
            nameChars[0] = char.ToLowerInvariant(nameChars[0]);

            string convertedName = new string(nameChars);

            if (name.Equals("TotalRecordsFiltered", StringComparison.OrdinalIgnoreCase))
            {
                convertedName = "recordsFiltered";
            }
            else if (name.Equals("TotalRecords", StringComparison.OrdinalIgnoreCase))
            {
                convertedName = "recordsTotal";
            }
            else if (name.Equals("Draw", StringComparison.OrdinalIgnoreCase))
            {
                convertedName = "draw";
            }

            return convertedName;
        }
    }
}
