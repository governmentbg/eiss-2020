using Newtonsoft.Json;
using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    /// <summary>
    /// Saved depersonalization queries
    /// </summary>
    public class DepersonalizationHistoryItem
    {
        /// <summary>
        /// String to be replaced
        /// </summary>
        [JsonProperty("searchValue")]
        public string SearchValue { get; set; }

        /// <summary>
        /// String to be used for replacement
        /// </summary>
        [JsonProperty("replaceValue")]
        public string ReplaceValue { get; set; }

        /// <summary>
        /// Search is case sesitive
        /// </summary>
        [JsonProperty("isCaseSensitive")]
        public bool IsCaseSensitive { get; set; }

        public StringComparison CaseSensitivity
        {
            get
            {
                if (IsCaseSensitive)
                {
                    return StringComparison.InvariantCulture;
                }
                else
                {
                    return StringComparison.InvariantCultureIgnoreCase;
                }
            }
        }
    }
}
