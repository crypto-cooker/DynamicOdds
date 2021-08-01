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
using System.IO;
using System.Security.Permissions;
using System.Xml.Serialization;
using System.Security;

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
        private void WriteOut(RootObject data)
        {
            {
                TextWriter writer = null;
                string outfile = argList["outfile"];
                try
                {
                    FileIOPermission f = new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.Write, outfile);
                    try
                    {
                        f.Demand();
                    }
                    catch (SecurityException s)
                    {
                        return;
                    }
                    XmlSerializer formatter = new XmlSerializer(typeof(RootObject));
                    writer = new StreamWriter(outfile, false);
                    formatter.Serialize(writer, data);
                }
                finally
                {
                    if (writer != null)
                    {
                        writer.Close();
                    }
                }
            }
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
                var result = getOfficialFeed(timestamp);
                RootObject data = JsonConvert.DeserializeObject<RootObject>(result);
                WriteOut(data);
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

    public class Runner
    {
        public int no { get; set; }
        public string name { get; set; }
        public string jockey { get; set; }
        public bool jockey_changed { get; set; }
        public bool scr { get; set; }
        public bool late_scr { get; set; }
        public double price_open { get; set; }
        public double price { get; set; }
        public string price_flucs { get; set; }
        public int wsp_count { get; set; }
        public int? updated_ts { get; set; }
    }

    public class Result
    {
        public int plc { get; set; }
        public int no { get; set; }
        public string name { get; set; }
        public double div_win_v { get; set; }
        public double div_win_n { get; set; }
        public double div_win_q { get; set; }
        public double div_plc_v { get; set; }
        public double div_plc_n { get; set; }
        public double div_plc_q { get; set; }
    }

    public class CalcData
    {
        public string r_no_str { get; set; }
        public string scr_str { get; set; }
        public string odds_str { get; set; }
    }

    public class DedRunner
    {
        public int r_no { get; set; }
        public int ded_w { get; set; }
        public int ded_p { get; set; }
    }

    public class Deduction
    {
        public int r_no_scr { get; set; }
        public string r_no_scr_time { get; set; }
        public CalcData calc_data { get; set; }
        public List<DedRunner> ded_runners { get; set; }
    }

    public class Event
    {
        public string meeting_id { get; set; }
        public string event_id { get; set; }
        public string m_code_4char { get; set; }
        public string state { get; set; }
        public bool state_op_status { get; set; }
        public string date { get; set; }
        public string start_time_vic { get; set; }
        public string venue { get; set; }
        public string no { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string track_cond { get; set; }
        public int track_rtg { get; set; }
        public string weather { get; set; }
        public int updated_ts { get; set; }
        public List<Runner> runners { get; set; }
        public double op_mkt_pct { get; set; }
        public List<Result> results { get; set; }
        public List<Deduction> deductions { get; set; }
    }

    public class RootObject
    {
        public List<Event> @event { get; set; }
        public double feed_version { get; set; }
        public int server_ts { get; set; }
    }
}