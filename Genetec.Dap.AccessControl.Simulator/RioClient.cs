namespace Genetec.Dap
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Xml.Linq;

    public enum RequestType
    {
        Login,
        CreateChannel,
        DeleteChannel,
        InterfaceState,
        InputState,
        CardSwipe,
        LongPolling,
        LongPollingSession,
        OfflineAccessGranted,
        OfflineAccessDenied,
        KeepAlive
    }

    public static class InputState
    {
        public const string Normal = "Normal";

        public const string Active = "Active";

        public const string Trouble = "Trouble";

        public const string TroubleShort = "Short";

        public const string TroubleCut = "Cut";
    }

    public class RioClient
    {
        private Cookie m_cookie;

        private readonly string m_username;
        private readonly string m_password;

        public event EventHandler<bool> ConnectionState;

        public bool IsConnected => _connected();

        public string Hostname { get; set; }

        public RioClient(string hostname, string username, string password)
        {
            m_cookie = new Cookie { Expired = true };

            Hostname = hostname;
            m_username = username;
            m_password = password;
        }

     
        public void Connect()
        {
            _connect();
        }
      
        public bool CreateChannel(RioChannel channel)
        {
            return _createChannel(channel);
        }
    
        public bool DeleteChannel(string channel)
        {
            return _deleteChannel(channel);
        }
      
        public bool InterfaceState(string channel, RioInterfaceState state)
        {
            return _changeInterfaceState(channel, state);
        }
        public bool InputState(string channel, RioInputState state)
        {
            return _changeInputState(channel, state);
        }
   
        public bool CardSwipe(string channel, RioCardDetected rioCard)
        {
            return _cardSwipe(channel, rioCard);
        }
    
        public string LongPollingSession(string channel)
        {
            var xElement = XElement.Parse(_longPollingSession(channel));

            IEnumerable<XElement> allChildElements = xElement.Elements();
            IEnumerable<XElement> specificChildElements = xElement.Elements("LongPollUri");
            XElement firstSpecificChildElement = xElement.Element("LongPollUri");
            var value = firstSpecificChildElement.Value;
            return value;
        }
    
        public string LongPolling(string session)
        {
            return _longPolling(session);
        }
   
        public bool OfflineAccessGranted(string channel, RioAccessGranted accessGranted)
        {
            return _offlineAccessGranted(channel, accessGranted);
        }

        public bool KeepAlive(string channel, RioKeepAlive duration)
        {
            return _keepAlive(channel, duration);
        }

  
        private bool _connected()
        {
            return !m_cookie.Expired;
        }
        static RioClient()
        {
            ServicePointManager.ServerCertificateValidationCallback +=(sender, cert, chain, sslPolicyErrors) => true;
  
        }

        private void _connect()
        {
          
          
         
            var httpWebRequest = _request(RequestType.Login);

            
            string postData = $"username={m_username}&password={m_password}";
           
            byte[] postDataBytes = new ASCIIEncoding().GetBytes(postData);
           
            httpWebRequest.ContentLength = postDataBytes.Length;

            
            using (var stream = httpWebRequest.GetRequestStream())
            {
                stream.Write(postDataBytes, 0, postDataBytes.Length);
                stream.Close();
            }

            try
            {
              
                using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
              
                    var strcookie = httpWebResponse.Headers["Set-Cookie"];
                    m_cookie = new Cookie(strcookie.Split('=')[0], strcookie.Split('=')[1].Split(';')[0], "", Hostname);

                    ConnectionState?.Invoke(this, true);
                }
            }
            catch (WebException)
            {
                ConnectionState?.Invoke(this, false);
            }
        }

     
        private HttpWebRequest _request(RequestType type, string aditionnal = "")
        {
            HttpWebRequest httpWebRequest;
            switch (type)
            {
                case RequestType.Login:
                    httpWebRequest = (HttpWebRequest)WebRequest.Create("https://" + Hostname + "/Login");
                    httpWebRequest.Method = "POST";
                    httpWebRequest.ContentType = "application/xml";
                    httpWebRequest.AllowAutoRedirect = false;
                    return httpWebRequest;
                case RequestType.CreateChannel:
                    httpWebRequest = (HttpWebRequest)WebRequest.Create($"https://{Hostname}/ExternalIntegrations/{aditionnal}");
                    httpWebRequest.Method = "PUT";
                    httpWebRequest.ContentType = "application/xml";
                    httpWebRequest.Accept = "application/xml";
                    httpWebRequest.Host = Hostname;
                    httpWebRequest.AllowAutoRedirect = false;
                    return httpWebRequest;
                case RequestType.KeepAlive:
                    httpWebRequest = (HttpWebRequest)WebRequest.Create($"https://{Hostname}/ExternalIntegrations/{aditionnal}/Update");
                    httpWebRequest.Method = "POST";
                    httpWebRequest.ContentType = "application/xml";
                    httpWebRequest.Accept = "application/xml";
                    httpWebRequest.Host = Hostname;
                    httpWebRequest.AllowAutoRedirect = false;
                    return httpWebRequest;
                case RequestType.DeleteChannel:
                    httpWebRequest = (HttpWebRequest)WebRequest.Create($"https://{Hostname}/ExternalIntegrations/{aditionnal}");
                    httpWebRequest.Method = "DELETE";
                    httpWebRequest.ContentType = "application/xml";
                    httpWebRequest.Accept = "application/xml";
                    httpWebRequest.Host = Hostname;
                    httpWebRequest.AllowAutoRedirect = false;
                    return httpWebRequest;
                case RequestType.InterfaceState:
                    httpWebRequest = (HttpWebRequest)WebRequest.Create($"https://{Hostname}/ExternalIntegrations/{aditionnal}/Update");
                    httpWebRequest.Method = "POST";
                    httpWebRequest.ContentType = "application/xml";
                    httpWebRequest.Accept = "application/xml";
                    httpWebRequest.Host = Hostname;
                    httpWebRequest.AllowAutoRedirect = false;
                    return httpWebRequest;
                case RequestType.CardSwipe:
                    httpWebRequest = (HttpWebRequest)WebRequest.Create($"https://{Hostname}/ExternalIntegrations/{aditionnal}/Update");
                    httpWebRequest.Method = "POST";
                    httpWebRequest.ContentType = "application/xml";
                    httpWebRequest.Accept = "application/xml";
                    httpWebRequest.Host = Hostname;
                    return httpWebRequest;
                case RequestType.InputState:
                    httpWebRequest = (HttpWebRequest)WebRequest.Create($"https://{Hostname}/ExternalIntegrations/{aditionnal}/Update");
                    httpWebRequest.Method = "POST";
                    httpWebRequest.ContentType = "application/xml";
                    httpWebRequest.Accept = "application/xml";
                    httpWebRequest.Host = Hostname;
                    return httpWebRequest;
                case RequestType.LongPollingSession:
                    httpWebRequest = (HttpWebRequest)WebRequest.Create($"https://{Hostname}/ExternalIntegrations/{aditionnal}/Events/StartSession");
                    httpWebRequest.Method = "GET";
                    httpWebRequest.ContentType = "application/xml";
                    httpWebRequest.Accept = "application/xml";
                    httpWebRequest.Host = Hostname;
                    return httpWebRequest;
                case RequestType.LongPolling:
                    httpWebRequest = (HttpWebRequest)WebRequest.Create($"https://{Hostname}{aditionnal}");
                    httpWebRequest.Method = "GET";
                    httpWebRequest.ContentType = "application/xml";
                    httpWebRequest.Accept = "application/xml";
                    httpWebRequest.Host = Hostname;
                    return httpWebRequest;
                case RequestType.OfflineAccessDenied:
                    httpWebRequest = (HttpWebRequest)WebRequest.Create($"https://{Hostname}/ExternalIntegrations/{aditionnal}/Update");
                    httpWebRequest.Method = "POST";
                    httpWebRequest.ContentType = "application/xml";
                    httpWebRequest.Accept = "application/xml";
                    httpWebRequest.Host = Hostname;
                    return httpWebRequest;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

  
        private bool _createChannel(RioChannel channel)
        {
         
            if (!IsConnected) _connect();

  
            if (!IsConnected) return false;

            var httpWebRequest = _request(RequestType.CreateChannel, channel.Name);

    
            string postData = channel.ToString();
            byte[] postDataBytes = new ASCIIEncoding().GetBytes(postData);
           
            httpWebRequest.ContentLength = postDataBytes.Length;

            if (httpWebRequest.CookieContainer == null)
            {
                httpWebRequest.CookieContainer = new CookieContainer();
            }

            httpWebRequest.CookieContainer.Add(m_cookie);

            using (var stream = httpWebRequest.GetRequestStream())
            {
                stream.Write(postDataBytes, 0, postDataBytes.Length);
                stream.Close();
            }

            using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                var cookie = httpWebResponse.Cookies[0];
                return !cookie.Expired;
            }
        }

        private bool _deleteChannel(string channel)
        {
            if (!IsConnected) _connect();

            if (!IsConnected) return false;

            var httpWebRequest = _request(RequestType.DeleteChannel, channel);

            string postData = "";
            byte[] postDataBytes = new ASCIIEncoding().GetBytes(postData);
            httpWebRequest.ContentLength = postDataBytes.Length;

            if (httpWebRequest.CookieContainer == null)
            {
                httpWebRequest.CookieContainer = new CookieContainer();
            }

            httpWebRequest.CookieContainer.Add(m_cookie);

            using (var stream = httpWebRequest.GetRequestStream())
            {
                stream.Write(postDataBytes, 0, postDataBytes.Length);
                stream.Close();
            }

            using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                var cookie = httpWebResponse.Cookies[0];
                return !cookie.Expired;
            }
        }

        private bool _keepAlive(string channel, RioKeepAlive duration)
        {
            if (!IsConnected) _connect();

            if (!IsConnected) return false;

            var httpWebRequest = _request(RequestType.KeepAlive, channel);

            string postData = duration.ToString();

            byte[] postDataBytes = new ASCIIEncoding().GetBytes(postData);
            httpWebRequest.ContentLength = postDataBytes.Length;

            if (httpWebRequest.CookieContainer == null)
            {
                httpWebRequest.CookieContainer = new CookieContainer();
            }

            httpWebRequest.CookieContainer.Add(m_cookie);

            using (var stream = httpWebRequest.GetRequestStream())
            {
                stream.Write(postDataBytes, 0, postDataBytes.Length);
                stream.Close();
            }

            using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                var cookie = httpWebResponse.Cookies[0];
                if (!cookie.Expired)
                {
                    m_cookie = cookie;
                }
                return !cookie.Expired;
            }
        }

        private bool _changeInterfaceState(string channel, RioInterfaceState state)
        {
            if (!IsConnected) _connect();

            if (!IsConnected) return false;

            var httpWebRequest = _request(RequestType.InterfaceState, channel);

            string postData = state.ToString();

            byte[] postDataBytes = new ASCIIEncoding().GetBytes(postData);
            httpWebRequest.ContentLength = postDataBytes.Length;

            if (httpWebRequest.CookieContainer == null)
            {
                httpWebRequest.CookieContainer = new CookieContainer();
            }

            httpWebRequest.CookieContainer.Add(m_cookie);

            using (var stream = httpWebRequest.GetRequestStream())
            {
                stream.Write(postDataBytes, 0, postDataBytes.Length);
                stream.Close();
            }

            using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                var cookie = httpWebResponse.Cookies[0];
                return !cookie.Expired;
            }
        }

        private bool _cardSwipe(string channel, RioCardDetected rioCard)
        {

            if (!IsConnected) _connect();

            if (!IsConnected) return false;

            var httpWebRequest = _request(RequestType.CardSwipe, channel);

            string postData = rioCard.ToString();

            byte[] postDataBytes = new ASCIIEncoding().GetBytes(postData);
            httpWebRequest.ContentLength = postDataBytes.Length;

            if (httpWebRequest.CookieContainer == null)
            {
                httpWebRequest.CookieContainer = new CookieContainer();
            }

            httpWebRequest.CookieContainer.Add(m_cookie);

            using (var stream = httpWebRequest.GetRequestStream())
            {
                stream.Write(postDataBytes, 0, postDataBytes.Length);
                stream.Close();
            }

            try
            {
                using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    var cookie = httpWebResponse.Cookies[0];
                    return !cookie.Expired;
                }
            }
            catch (WebException webex)
            {
                WebResponse errResp = webex.Response;
                using (Stream respStream = errResp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(respStream);
                    string text = reader.ReadToEnd();
                }
            }

            return false;

        }

        private bool _changeInputState(string channel, RioInputState rioInput)
        {
            if (!IsConnected) _connect();

            if (!IsConnected) return false;

            var httpWebRequest = _request(RequestType.InputState, channel);

            string postData = rioInput.ToString();

            byte[] postDataBytes = new ASCIIEncoding().GetBytes(postData);
            httpWebRequest.ContentLength = postDataBytes.Length;

            if (httpWebRequest.CookieContainer == null)
            {
                httpWebRequest.CookieContainer = new CookieContainer();
            }

            httpWebRequest.CookieContainer.Add(m_cookie);

            using (var stream = httpWebRequest.GetRequestStream())
            {
                stream.Write(postDataBytes, 0, postDataBytes.Length);
                stream.Close();
            }

            try
            {
                using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    var cookie = httpWebResponse.Cookies[0];
                    return !cookie.Expired;
                }
            }
            catch (WebException webex)
            {
                WebResponse errResp = webex.Response;
                using (Stream respStream = errResp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(respStream);
                    string text = reader.ReadToEnd();
                }
            }

            return false;
        }

        private string _longPollingSession(string channel)
        {
            if (!IsConnected) _connect();

            if (!IsConnected) return "";

            var httpWebRequest = _request(RequestType.LongPollingSession, channel);

            if (httpWebRequest.CookieContainer == null)
            {
                httpWebRequest.CookieContainer = new CookieContainer();
            }

            httpWebRequest.CookieContainer.Add(m_cookie);

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException webex)
            {
                WebResponse errResp = webex.Response;
                using (Stream respStream = errResp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(respStream);
                    string text = reader.ReadToEnd();
                }
            }

            return "";

        }

        private string _longPolling(string session)
        {
            if (!IsConnected) _connect();

            if (!IsConnected) return null;

            var httpWebRequest = _request(RequestType.LongPolling, session);

            if (httpWebRequest.CookieContainer == null)
            {
                httpWebRequest.CookieContainer = new CookieContainer();
            }

            httpWebRequest.CookieContainer.Add(m_cookie);

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException webex)
            {
                WebResponse errResp = webex.Response;
                using (Stream respStream = errResp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(respStream);
                    string text = reader.ReadToEnd();
                }
            }

            return null;
        }

        private bool _offlineAccessGranted(string channel, RioAccessGranted accessGranted)
        {
            if (!IsConnected) _connect();

            if (!IsConnected) return false;

            var httpWebRequest = _request(RequestType.CardSwipe, channel);

            string postData = accessGranted.ToString();

            byte[] postDataBytes = new ASCIIEncoding().GetBytes(postData);
            httpWebRequest.ContentLength = postDataBytes.Length;

            if (httpWebRequest.CookieContainer == null)
            {
                httpWebRequest.CookieContainer = new CookieContainer();
            }

            httpWebRequest.CookieContainer.Add(m_cookie);

            using (var stream = httpWebRequest.GetRequestStream())
            {
                stream.Write(postDataBytes, 0, postDataBytes.Length);
                stream.Close();
            }

            try
            {
                using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    var cookie = httpWebResponse.Cookies[0];
                    return !cookie.Expired;
                }
            }
            catch (WebException webex)
            {
                WebResponse errResp = webex.Response;
                using (Stream respStream = errResp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(respStream);
                    string text = reader.ReadToEnd();
                }
            }

            return false;
        }
    }
}
