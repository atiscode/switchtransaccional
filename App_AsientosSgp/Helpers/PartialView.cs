using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.Routing;
using App_Mundial_Miles.Controllers;


namespace App_Mundial_Miles.Helpers
{
    public static class ControllerExtensionsHelper
    {
        public static string PartialViewToString(this Controller controller)
        {
            return controller.PartialViewToString(null, null);
        }
        public static string PartialViewToString(this Controller controller, string controllerName)
        {
            return controller.PartialViewToString(null, controllerName, null);
        }

        public static string RenderPartialViewToString(this Controller controller, string viewName)
        {
            return controller.PartialViewToString(viewName, null);
        }
        public static string RenderPartialViewToString(this Controller controller, string controllerName, string viewName)
        {
            return controller.PartialViewToString(viewName, controllerName, null);
        }

        public static string RenderPartialViewToString(this Controller controller, object model)
        {
            return controller.PartialViewToString(null, model);
        }
        public static string RenderPartialViewToString(this Controller controller, string controllerName, object model)
        {
            return controller.PartialViewToString(null, controllerName, model);
        }

        public static string PartialViewToString(this Controller controller, string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.RouteData.GetRequiredString("action");
            }

       
            controller.ViewData.Model = model;

            using (StringWriter stringWriter = new StringWriter())
            {
              
               ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, stringWriter);


                if (controller.HttpContext.Session != null && controller.HttpContext.Session["culture"]!= null)
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(controller.HttpContext.Session["culture"].ToString());
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(controller.HttpContext.Session["culture"].ToString());
                }
         
                viewResult.View.Render(viewContext, stringWriter);
                return stringWriter.GetStringBuilder().ToString();
            }
        }
        public static string PartialViewToString(this Controller controller, string controllerName, string viewName, object model)
        {
            
            var route = new Route("{controller},{action},{id}", controller.RouteData.RouteHandler);
            var routeData = new RouteData(route,controller.RouteData.RouteHandler);
            routeData.Values.Add("controller",controllerName);
            routeData.Values.Add("action", "Index");
            routeData.Values.Add("id", null);
            var requestContext = new RequestContext(controller.HttpContext, routeData);
            var builder = new ControllerBuilder();
            var created = builder.GetControllerFactory().CreateController(requestContext, controllerName);
            var newController = created as Controller;
            if (newController == null)
                throw new Exception("Controlador no encontrado");
            newController.ControllerContext = new ControllerContext(requestContext, newController);
        
            return PartialViewToString(newController, viewName, model);
        }


    }
}