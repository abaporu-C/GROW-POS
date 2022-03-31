using Microsoft.AspNetCore.Http;

namespace GROW_CRM.Utilities
{
    public class MaintainURL
    {
        /// <summary>
        /// Maintain the URL for and Index View including filter, sort and page informaiton.
        /// Warning: Works with our default route by setting the href to /Controller
        /// so you may need to make adjustments for a custom route.
        /// </summary>
        /// <param name="httpContext">the HttpContext from the Controller</param>
        /// <param name="ControllerName">The Name of the Controller</param>
        /// <returns>The Index URL with paramerters if required</returns>
        public static string ReturnURL(HttpContext httpContext, string ControllerName)
        {
            string cookieName = ControllerName + "URL";
            string SearchText = "/" + ControllerName + "?";
            //Get the URL of the page that sent us here
            string returnURL = httpContext.Request.Headers["Referer"].ToString();
            if (returnURL.Contains(SearchText))
            {
                //Came here from the Index with some parameters
                //Save the Parameters in a Cookie
                returnURL = returnURL[returnURL.LastIndexOf(SearchText)..];
                CookieHelper.CookieSet(httpContext, cookieName, returnURL, 30);
                return returnURL;
            }
            else
            {
                //Get it from the Cookie
                //Note that this might return an empty string but we will
                //change it to go back to the Index of the Controller.
                returnURL = httpContext.Request.Cookies[cookieName];
                return returnURL ?? "/" + ControllerName;
            }
        }
    }
}