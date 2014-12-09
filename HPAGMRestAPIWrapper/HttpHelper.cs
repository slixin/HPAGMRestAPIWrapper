using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Cache;
using System.IO;
using System.Net.Security;

namespace HPAGMRestAPIWrapper
{
    public class HttpHelper
    {
        #region public properties
        public Dictionary<string, string> ResponseHeaders { get; set; }
        public Dictionary<string, string> RequestHeaders { get; set; }
        public string Method { get; set; }
        public string ContentType { get; set; }
        public string Accept { get; set; }
        public string Response { get; set; }
        public string PostData { get; set; }
        public string PostFilePath { get; set; }
        public RequestCacheLevel Cache { get; set; }
        public CookieContainer CookieContainer { get; set; }
        public bool IsIgnoreCertificateSecure { get; set; }
        #endregion

        #region Private members
        private readonly string _username;
        private readonly string _password;
        private WebClientEx _webClient;
        private HttpWebRequest _request;
        #endregion

        public HttpHelper(string username, string password)
        {
            CookieContainer = new CookieContainer();
            _username = username;
            _password = password;

            RequestHeaders = new Dictionary<string, string>();
        }

        /// <summary>
        /// Download file
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="saveToFilePath"></param>
        public void DownloadFile(string uri, string saveToFilePath)
        {
            using (_webClient = new WebClientEx(CookieContainer))
            {
                _webClient.Credentials = new NetworkCredential(_username, _password);
                _webClient.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                if (RequestHeaders != null)
                {
                    foreach (var kvp in RequestHeaders)
                    {
                        _webClient.Headers.Set(kvp.Key, kvp.Value);
                    }
                }
                _webClient.Headers.Add("Accept", "application/octet-stream");

                _webClient.DownloadFile(uri, saveToFilePath);
            }
        }

        /// <summary>
        /// Send Request, support GET/POST/DELETE/PUT
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public int Request(string uri)
        {
            Response = null;
            int statuscode;
            HttpWebResponse response;

            try
            {
                if (IsIgnoreCertificateSecure)
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
                _request = (HttpWebRequest)HttpWebRequest.Create(uri);
                _request.Credentials = new NetworkCredential(_username, _password);
                _request.Method = Method;
                _request.CachePolicy = new RequestCachePolicy(Cache);
                _request.ContentType = ContentType;
                _request.CookieContainer = CookieContainer;
                _request.Accept = Accept;

                if (RequestHeaders != null)
                {
                    foreach (var kvp in RequestHeaders)
                    {
                        _request.Headers.Set(kvp.Key, kvp.Value);
                    }
                }                
                // If it is post, there is 2 ways, one for file uploading, one for add entity.
                if (Method.Equals("post", StringComparison.CurrentCultureIgnoreCase) || Method.Equals("put", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(PostFilePath))
                    {
                        var fi = new FileInfo(PostFilePath);
                        var fileLength = (int)fi.Length;
                        var rdr = new FileStream(PostFilePath, FileMode.Open);
                        var requestBytes = new byte[fileLength];

                        _request.ContentLength = requestBytes.Length;
                        using (var requestStream = _request.GetRequestStream())
                        {
                            int bytesRead;
                            while ((bytesRead = rdr.Read(requestBytes, 0, requestBytes.Length)) != 0)
                            {
                                requestStream.Write(requestBytes, 0, bytesRead);
                                requestStream.Close();
                            }
                        }
                        rdr.Close();
                        PostFilePath = string.Empty;
                    }
                    else if (!string.IsNullOrEmpty(PostData))
                    {
                        var requestBytes = Encoding.UTF8.GetBytes(PostData);
                        _request.ContentLength = requestBytes.Length;
                        using (var requestStream = _request.GetRequestStream())
                        {
                            requestStream.Write(requestBytes, 0, requestBytes.Length);
                            requestStream.Close();
                        }                        
                        
                        PostData = string.Empty;
                    }
                    else
                    {
                        throw new Exception("Post data or post file path cannot be empty.");
                    }
                }                
                response = (HttpWebResponse)_request.GetResponse();
                statuscode = (int)response.StatusCode;
                if (statuscode == 200 || statuscode == 201)
                {
                    ResponseHeaders = new Dictionary<string, string>();
                    for (var i = 0; i < response.Headers.Count; i++)
                    {
                        ResponseHeaders.Add(response.Headers.Keys[i], response.Headers.Get(i));
                    }

                    var streamResponse = response.GetResponseStream();
                    using (var streamRead = new StreamReader(streamResponse))
                    {
                        Response = streamRead.ReadToEnd();
                        streamRead.Close();
                    }
                    response.Close();
                }
                else
                {
                    throw new Exception(string.Format("Return Status Code is: {0}", statuscode));
                }
            }
            catch (WebException wex)
            {
                string exception;
                if (wex.Response == null)
                    throw new Exception(wex.Message);

                var webExceptionResponse = wex.Response.GetResponseStream();
                using (var streamRead = new StreamReader(webExceptionResponse))
                {
                    exception = streamRead.ReadToEnd();
                    streamRead.Close();
                }

                throw exception.IndexOf("<QCRestException>") > 0
                          ? new Exception(Common.GetQCRestException(exception))
                          : new Exception(exception);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return statuscode;
        }
    }
}
