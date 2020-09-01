using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using App_Mundial_Miles.Helpers;
using App_Mundial_Miles.Models;
using NLog;

namespace App_Mundial_Miles.Controllers
{
    public abstract class GenericController<T, K> : Controller where T : class
    {

        private Logger logger = LogManager.GetCurrentClassLogger();
        public virtual JsonResult UploadFinished()
        {
            try
            {
                var finished = (string)Session["uploaded"];
                return Json(finished, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
        }

    }
}
