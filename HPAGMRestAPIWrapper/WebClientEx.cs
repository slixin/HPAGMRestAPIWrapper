using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    /// WebClientEx class
    /// </summary>
    public class WebClientEx : WebClient
    {
        /// <summary>
        /// constructor
        /// </summary>
        public WebClientEx(CookieContainer container)
        {
            this.container = container;
        }

        private readonly CookieContainer container = new CookieContainer();

        /// <summary>
        /// Send web request
        /// </summary>
        /// <param name="address">request URL</param>
        /// <returns></returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest r = base.GetWebRequest(address);
            var request = r as HttpWebRequest;
            if (request != null)
            {
                request.CookieContainer = container;
            }
            return r;
        }

        /// <summary>
        /// Get response
        /// </summary>
        /// <param name="request">request</param>
        /// <param name="result">async result</param>
        /// <returns></returns>
        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = base.GetWebResponse(request, result);
            ReadCookies(response);
            return response;
        }

        /// <summary>
        /// Get response
        /// </summary>
        /// <param name="request">response</param>
        /// <returns></returns>
        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            ReadCookies(response);
            return response;
        }

        private void ReadCookies(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response != null)
            {
                CookieCollection cookies = response.Cookies;
                container.Add(cookies);
            }
        }
    }
}
