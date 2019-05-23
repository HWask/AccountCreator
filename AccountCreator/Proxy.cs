using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace AccountCreator
{
    public class Proxy
    {
        public struct ProxyServer
        {
            public ProxyServer(string ip, int Port, int viewers = 0)
            {
                m_IP = ip;
                PORT = Port;
                Viewers = viewers;            
            }

            public string m_IP;
            public int PORT;
            public int Viewers;
        }

        public List<ProxyServer> Proxies;

        public Proxy()
        {
            Proxies = new List<ProxyServer>();
        }

        public bool GetToken(ref string token)
        {
            string response = "";

            if (Http.HttpGetRequest("http://www.proxy-listen.de/Proxy/Proxyliste.html", out response))
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(response);
                HtmlNode a = doc.GetElementbyId("right_row");
                HtmlNode b = a.SelectSingleNode(".//input[@type='hidden']");
                token = b.Attributes["value"].Value;

                return true;
            }

            return false;
        }

        public bool GetProxyList()
        {
            string token = "";

            if(!GetToken(ref token))
            {
                return false;
            }

            string htmlCode = "";
            Dictionary<string, string> Params = new Dictionary<string,string>();

            Params["filter_port"] = "";
            Params["filter_http_gateway"] = "";
            Params["filter_http_anon"] = "";
            Params["filter_response_time_http"] = "5";
            Params["ggfhgfjcfgds"] = token; //token: <input name="ggfhgfjcfgds" value="b483caab08c7a64211946427d4fa53de" type="hidden">
            Params["filter_country"] = "";
            Params["filter_timeouts1"] = "10";
            Params["liststyle"] = "info";
            Params["proxies"] = "300"; // How many proxies shall we retrieve?
            Params["type"] = "http";
            Params["submit"] = "Anzeigen";

            if(Http.HttpPostRequest("http://www.proxy-listen.de/Proxy/Proxyliste.html", Params, out htmlCode))
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlCode);

                HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//tr[@class='proxyListOdd'] | //tr[@class='proxyListEven']");

                foreach(HtmlNode n in nodes)
                {
                    HtmlNodeCollection tds = n.SelectNodes(".//td");
                    string ip = tds[0].Descendants().First().InnerHtml;
                    string port = tds[1].InnerHtml;
                    Proxies.Add(new ProxyServer(ip,  int.Parse(port)));
                }

                return true;
            }
            
            return false;
        }

        public ProxyServer? ChangeProxy(ProxyServer proxy)
        {
            for (int i = 0; i < Proxies.Count; i++ )
            {
                if (Proxies[i].Viewers < 5)
                {
                    Proxies[i] = new ProxyServer(Proxies[i].m_IP, Proxies[i].PORT, Proxies[i].Viewers+1);

                    return Proxies[i];
                }
            }

            return null;
        }
    }
}
