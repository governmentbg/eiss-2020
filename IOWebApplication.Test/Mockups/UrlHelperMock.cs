// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace IOWebApplication.Test.Mockups
{
    public class UrlHelperMock : IUrlHelper
    {
        public ActionContext ActionContext => new ActionContext();

        public string Action(UrlActionContext actionContext)
        {
            return "UnitTestMock";
        }

        public string Content(string contentPath)
        {
            return "UnitTestMock";
        }

        public bool IsLocalUrl(string url)
        {
            return true;
        }

        public string Link(string routeName, object values)
        {
            return "UnitTestMock";
        }

        public string RouteUrl(UrlRouteContext routeContext)
        {
            return "UnitTestMock";
        }
    }
}
