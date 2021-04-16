using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IOWebApplication.TagHelpers
{
    [HtmlTargetElement("menuitem", Attributes = "menu")]
    public class MenuItemTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger logger;

        public MenuItemTagHelper(IHttpContextAccessor _httpContextAccessor, ILogger<MenuItemTagHelper> _logger)
        {
            httpContextAccessor = _httpContextAccessor;
            logger = _logger;
        }

        [HtmlAttributeName("menu")]
        public string DataMenuItem { get; set; }

        [HtmlAttributeName("roles")]
        public string Roles { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            try
            {
                bool userOk = true;
                if (!string.IsNullOrEmpty(this.Roles))
                {
                    userOk = false;
                    var currentUser = httpContextAccessor.HttpContext.User;

                    if (currentUser != null)
                    {
                        foreach (var role in this.Roles.Split(','))
                        {
                            if (currentUser.IsInRole(role))
                            {
                                userOk = true;
                                break;
                            }
                        }
                    }

                }
                if (userOk)
                {
                    var content = (await output.GetChildContentAsync()).GetContent();

                    output.TagName = "li";
                    output.Attributes.Add("data-menuitem", DataMenuItem);
                    output.Content.AppendHtml(content);
                }
                else
                {
                    output.SuppressOutput();
                }
            }
            catch (Exception ex)
            {
                logger.LogError("TextBoxTagHelper.cs", ex);
            }

            base.Process(context, output);
        }

    }
}
