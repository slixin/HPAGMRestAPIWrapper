using System;
using System.Net;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    /// Connect to AGM Server
    /// </summary>
    public class AGMConnection : IDisposable
    {
        private string _urlAuthenticate;
        private string _urlLogout;
        private string _urlIsAuthenticated;

        #region Properties
        /// <summary>
        /// Server name of AGM
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Domain of AGM
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Project of AGM
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// Username of login AGM
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Password of login AGM
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// HTTP object
        /// </summary>
        public HttpHelper Http { get; set; }

        public bool IsIgnoreCertificateSecure { get; set; }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serverName">Server Name</param>
        /// <param name="username">User name</param>
        /// <param name="password">Password</param>
        public AGMConnection(string serverName, string username, string password, string domain, string project, bool ignoreCertificate)
        {
            ServerName = serverName;
            UserName = username;
            Password = password;
            Domain = domain;
            Project = project;
            IsIgnoreCertificateSecure = ignoreCertificate;
            _urlAuthenticate = string.Format("{0}/agm/authentication-point/alm-authenticate", ServerName);
            _urlLogout = string.Format("{0}/agm/authentication-point/logout", ServerName);
            _urlIsAuthenticated = string.Format("{0}/agm/rest/is-authenticated", ServerName);

            Logon();
        }

        public void Logon()
        {
            string xmlContent = null;

            try
            {
                xmlContent = string.Format("<alm-authentication><user>{0}</user><password>{1}</password></alm-authentication>", UserName, Password);
                Http = new HttpHelper(UserName, Password)
                {
                    Cache = System.Net.Cache.RequestCacheLevel.NoCacheNoStore,
                    ContentType = "application/xml",
                    Method = "POST",
                    IsIgnoreCertificateSecure = IsIgnoreCertificateSecure,
                };
                Http.PostData = xmlContent;
                Http.Request(_urlAuthenticate);
                var token = Common.ParseHeaders(Http.ResponseHeaders["Set-Cookie"], "LWSSO_COOKIE_KEY");
                if (string.IsNullOrEmpty(token))
                    throw new Exception("Token is empty.");
                Http.RequestHeaders.Add("Cookie", string.Format("LWSSO_COOKIE_KEY={0}", token));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Log off from AGM
        /// </summary>
        public void Logoff()
        {
            try
            {
                Http.ContentType = "application/xml";
                Http.Method = "GET";
                Http.Request(_urlLogout);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool IsAuthenticated()
        {
            bool result = false;
            try
            {
                Http.ContentType = "application/xml";
                Http.Method = "GET";
                int res = Http.Request(_urlIsAuthenticated);
                if (res == 200)
                    result = true;
                else
                    result = false;
            }
            catch (Exception ex)
            {
                if (ex.Message.IndexOf("Authentication failed") > 0)
                    result = false;
                else if (ex.Message.IndexOf("User access denied") > 0 )
                    result = true;
                else
                    throw new Exception(ex.Message);
            }

            return result;
        }

        void IDisposable.Dispose()
        {
            Logoff();
        }
    }
}
