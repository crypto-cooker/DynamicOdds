using MSXML2;
using System;
using RestSharp;
using System.Net;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Xml;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml.Linq;

namespace DOFeed
{
    public class WebFeed
    {
        private Dictionary<string, string> argList;
        private string feedUrl = "http://dynamicraceodds.com/xml/data/";
        private string userName = "EricPhillips";
        private string pw = "Roger4870";

        private string graphQLEndpointUrl = "https://ql.uat.coredb.tbm.sh/query";
        private string tokenEndpointUrl = "https://bmcoredb-uat.auth.ap-southeast-2.amazoncognito.com/oauth2/token";

        private string officialUrl = "http://feeds.officialprice.com.au/feeds/op_feed_new.php";

        private string clientID = "4eevk9m5f39kut9njqhfi19qn0";
        private string clientSecret = "1obi38gcdov3eupau41ns3cp3nquf08kfvpal3r3ifhfmh1sgg8u";
        private RestClient _client;
        private RestClient _tokenClient;
        private string tokenStr = "";
        public WebFeed(Dictionary<string, string> argList)
        {
            this.argList = argList;
            _client = new RestClient(graphQLEndpointUrl);
            _tokenClient = new RestClient(tokenEndpointUrl);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", "grant_type=client_credentials&scope=graphql/api&client_id=" + clientID+"&client_secret="+clientSecret, ParameterType.RequestBody);
            IRestResponse response = _tokenClient.Execute(request);
            dynamic json = JsonConvert.DeserializeObject(response.Content);
            tokenStr = json["access_token"];
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }
        public XmlDocument JsonToXML(string json)
        {
            XmlDocument doc = new XmlDocument();

            using (var reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(json), XmlDictionaryReaderQuotas.Max))
            {
                XElement xml = XElement.Load(reader);
                doc.LoadXml(xml.ToString());
            }

            return doc;
        }
        public string getOfficialFeed(string timestamp)
        {
            string url = officialUrl + "?ts=" + timestamp;
            try
            {
                var http = new ServerXMLHTTP60();
                http.open("GET", url, false);
                http.setRequestHeader("Connection", "Keep-Alive");
                http.setRequestHeader("User-Agent", "Mozilla/5.0 " + DateTime.Now.ToLongTimeString());

                http.send();
                return http.responseText;
            }
            catch (Exception e)
            {
                return "";
            }
        }
        internal string ProcessFeed()
        {
            string xmlResult = string.Empty;
            string query;
            var xmlStr = "";
            var official = argList["official"];
            if (official == "true")
            {
                string timestamp = argList["timestamp"];
                //query = makeMeetingQuery(timestamp);
                var result = getOfficialFeed(timestamp);
                //xmlStr = result.data.ToString();
                XmlDocument doc = JsonToXML(result);
                doc.Save(argList["outfile"]);

            } else {
                switch (argList["method"])
                {
                    case "Login":
                        break;
                    case "GetMeetingsAll":
                        string date = argList["date"];
                        query = makeMeetingQuery(date);
                        var result = Execute(query, null, new Dictionary<string, string>());
                        xmlStr = result.data.ToString();
                        XmlDocument doc = JsonToXML(xmlStr);
                        doc.Save(argList["outfile"]);
                        break;
                    case "GetRunner":
                        string id = argList["id"];
                        query = makeRunnerQuery(id);
                        result = Execute(query, null, new Dictionary<string, string>());
                        xmlStr = result.data.ToString();
                        doc = JsonToXML(xmlStr);
                        doc.Save(argList["outfile"]);
                        break;

                    case "GetRace":
                        id = argList["id"];
                        query = makeRaceQuery(id);
                        result = Execute(query, null, new Dictionary<string, string>());
                        xmlStr = result.data.ToString();
                        doc = JsonToXML(xmlStr);
                        doc.Save(argList["outfile"]);
                        break;
                    case "GetSportSources":
                        query = makeSportSources();
                        result = Execute(query, null, new Dictionary<string, string>());
                        xmlStr = result.data.ToString();
                        doc = JsonToXML(xmlStr);
                        doc.Save(argList["outfile"]);
                        break;
                    case "GetEventSchedule":
                        break;
                    case "GetRunnerOdds":
                        break;
                    case "GetEventResults":
                        break;
                    case "GetExotics":
                        break;
                    case "GetBettingAgencies":
                        break;
                    case "GetBookmakerFlucs":
                        break;
                    case "GetSportTypes":
                        query = makeSportTypes();
                        result = Execute(query, null, new Dictionary<string, string>());
                        xmlStr = result.data.ToString();
                        doc = JsonToXML(xmlStr);
                        doc.Save(argList["outfile"]);
                        break;
                    case "GetSources":
                        query = makeSources();
                        result = Execute(query, null, new Dictionary<string, string>());
                        xmlStr = result.data.ToString();
                        doc = JsonToXML(xmlStr);
                        doc.Save(argList["outfile"]);
                        break;
                    case "GetSportEvents":
                        query = makeSportEvents();
                        result = Execute(query, null, new Dictionary<string, string>());
                        xmlStr = result.data.ToString();
                        doc = JsonToXML(xmlStr);
                        doc.Save(argList["outfile"]);
                        break;
                    case "GetTracks":
                        query = makeTracks();
                        result = Execute(query, null, new Dictionary<string, string>());
                        xmlStr = result.data.ToString();
                        doc = JsonToXML(xmlStr);
                        doc.Save(argList["outfile"]);
                        break;
                    default:
                        break;
                }
            }
            return xmlResult;
        }

        public dynamic Execute(string query, object variables = null, Dictionary<string, string> additionalHeaders = null, int timeout = 0)
        {
            var request = new RestRequest("/", Method.POST);
            request.Timeout = timeout;
            if(tokenStr.Length>0)
            {
                request.AddHeader("authorization", "Bearer "+tokenStr);
            }
            //request.AddCookie("BMDBTOKEN", "DEMO");
            if (additionalHeaders != null && additionalHeaders.Count > 0)
            {
                foreach (var additionalHeader in additionalHeaders)
                {
                    request.AddHeader(additionalHeader.Key, additionalHeader.Value);
                }
            }
            request.AddJsonBody(new
            {
                query = query,
                variables = variables
            });

            return JObject.Parse(_client.Execute(request).Content);
        }
        private string makeMeetingQuery(string date)
        {
            return @"
                {
                    meetingsDated(date: "+'"'+ date + '"'+@") {
                        id
                        type
                        track { name country }
                            races { number id }
                    }
                }
            ";
        }

        private string makeSources()
        {
            return @"
            {
                sources{
                    code
                    name
                    gbsCode
                    isPriceSource
                }
            }
            ";
        }
        private string makeTracks()
        {
            return @"
            {
                tracks{
                    id
                    created
                    updated
                    name
                    country
                    state
                    surface
                    abbreviation
                }
            }
            ";
        }
        private string makeSportEvents()
        {
            return @"
            {
                sources{
                    code
                    name
                    gbsCode
                    isPriceSource
                }
            }
            ";
        }
        private string makeSportSources()
        {
            return @"
                {
                  sportSources{
                    id
                    name
                    description
                    trusted
                    modified_date
                  }
                }
            ";
        }
        private string makeSportTypes()
        {
            return @"
                {
                  sportTypes{
                    id
                    name
                    description
                  }
                }
            ";
        }
        private string makeRunnerQuery(string id)
        {
            return @"
                {
                    runner(id: " + '"' + id + '"' + @") {
                        competitorsConnection {
                            competitors {
                                    finalPosition
                                    jockey
                                    margin
                                race {
                                        number
                                        name
                                    meeting {
                                            date
                                            track { name }
                                        }
                                    }
                                }
                            }
                        }
                }
            ";
        }
        private string makeRaceQuery(string id)
        {
            return @"
                {
                    race(id: " + '"' + id + '"' + @") {
                            id
                            name
                            number
                            class
                            distance
                            results(sources:" + '"' + "N" + '"' + @")
                            {
                                positions { position margin tabNo}
                            }
                            competitors {
                                runner { id
                                        }
                                tabNo
                                barrier
                                name
                                age
                                sex
                                jockey
                                trainer
                                country
                                scratched
                                prices(sources: [" + '"' + "LB2" + '"' + @"])
                        {
                            price
                                }
                    }
                        }
                }
            ";
        }
        

        private string ApplyMethod(string finalUrl)
        {
            return NavigateOfficialFeed(finalUrl);
        }

        private string NavigateOfficialFeed(string finalUrl)
        {
            try
            {
                var http = new ServerXMLHTTP60();
                http.open("GET", finalUrl, false);

                http.send();
                return http.responseText;
            }
            catch (Exception e)
            {
                return "";
            }
        }

        private string constructLoginUrl()
        {
            return feedUrl + "Login.asp?UserName=" + userName + "&Password=" + pw;
        }
    }
}