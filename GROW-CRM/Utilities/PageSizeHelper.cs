using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace GROW_CRM.Utilities
{
    public static class PageSizeHelper
    {
        /// <summary>
        /// Gets the page size coming from either the Select control
        /// or the cookie.  Otherwise it sets the default of 5 if the user
        /// has never set a preference.  The first time they select a page size 
        /// on any View, that becomes the default for all other Views as well.
        /// However, if they set a preferred page size on any View after that
        /// it is remembered for that controller.
        /// </summary>
        /// <param name="httpContext">The HttpContext from the controller</param>
        /// <param name="pageSizeID">the pageSizeID value from the Request</param>
        /// <param name="ControllerName">the name of the Controller</param>
        /// <returns></returns>
        public static int SetPageSize(HttpContext httpContext, int? pageSizeID, string ControllerName = "")
        {
            //
            int pageSize = 0;
            if (pageSizeID.HasValue)
            {
                //Value selected from DDL so use and save it to Cookie
                pageSize = pageSizeID.GetValueOrDefault();
                CookieHelper.CookieSet(httpContext, ControllerName + "pageSizeValue", pageSize.ToString(), 480);
                //Set this value as the new default if a custom page size has not been set for a controller
                CookieHelper.CookieSet(httpContext, "DefaultpageSizeValue", pageSize.ToString(), 480);
            }
            else
            {
                //Not selected so see if it is in Cookie
                pageSize = Convert.ToInt32(httpContext.Request.Cookies[ControllerName + "pageSizeValue"]);
            }
            if (pageSize == 0)
            {
                //Get the saved defalult if there is one
                pageSize = Convert.ToInt32(httpContext.Request.Cookies["DefaultpageSizeValue"]);
            }
            return (pageSize == 0) ? 5 : pageSize;//Neither Selected or in Cookie so go with default
        }
        /// <summary>
        /// Creates a SelectList for choices for page size
        /// </summary>
        /// <param name="pageSize">Current value for the selected option</param>
        /// <returns></returns>
        public static SelectList PageSizeList(int? pageSize)
        {
            return new SelectList(new[] { "3", "5", "10", "20", "30", "40", "50", "100", "500" }, pageSize.ToString());
        }
    }
}