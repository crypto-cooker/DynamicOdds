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
        private RestClient _client;
        public WebFeed(Dictionary<string, string> argList)
        {
            this.argList = argList;
            _client = new RestClient(graphQLEndpointUrl);
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
        internal string ProcessFeed()
        {
            string xmlResult = string.Empty;
            string query;
            var xmlStr = "";
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
                    doc = JsonConvert.DeserializeXmlNode(xmlStr);
                    doc.Save(argList["outfile"]);
                    break;
                case "GetRace":
                    id = argList["id"];
                    query = makeRaceQuery(id);
                    result = Execute(query, null, new Dictionary<string, string>());
                    xmlStr = result.data.ToString();
                    doc = JsonConvert.DeserializeXmlNode(xmlStr);
                    doc.Save(argList["outfile"]);
                    break;
                case "GetEvent":
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
                default:
                    break;
            }
            return xmlResult;
        }

        public dynamic Execute(string query, object variables = null, Dictionary<string, string> additionalHeaders = null, int timeout = 0)
        {
            var request = new RestRequest("/", Method.POST);
            request.Timeout = timeout;
            request.AddCookie("BMDBTOKEN", "DEMO");
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
        private string constructBookmakerFlucsUrl()
        {
            string sessionId = argList["sessionid"];
            string eventId = argList["eventid"];
            string baid = argList["baid"];
            return $"{feedUrl}GetData.asp?SessionID={sessionId}&Method=GetBookmakerFlucs&EventID={eventId}&BAID={baid}";
        }

        private string constructBettingAgenciesUrl()
        {
            string sessionId = argList["sessionid"];
            return $"{feedUrl}GetData.asp?SessionID={sessionId}&Method=GetBettingAgencies";
        }

        private string constructExoticsUrl()
        {
            string sessionId = argList["sessionid"];
            string exoticType = argList["exotictype"];
            string eventId = argList["eventid"];
            return $"{feedUrl}GetData.asp?SessionID={sessionId}&Method=GetExotics&EventID={eventId}&ExoticType={exoticType}";
        }

        private string constructEventResultsUrl()
        {
            string sessionId = argList["sessionid"];
            string eventId = argList["eventid"];
            return $"{feedUrl}GetData.asp?SessionID={sessionId}&Method=GetEventResults&EventID={eventId}";
        }

        private string constructRunnerOddsUrl()
        {
            string sessionId = argList["sessionid"];
            string eventId = argList["eventid"];
            return $"{feedUrl}GetData.asp?SessionID={sessionId}&Method=GetRunnerOdds&EventID={eventId}";
        }

        private string constructEventScheduleUrl()
        {
            string sessionId = argList["sessionid"];
            string date = argList["date"];
            return $"{feedUrl}GetData.asp?SessionID={sessionId}&Method=GetEventSchedule&Date={date}&Types=R&Limit=999";
        }

        private string constructEventUrl()
        {
            string sessionId = argList["sessionid"];
            string eventId = argList["eventid"];
            return $"{feedUrl}GetData.asp?SessionID={sessionId}&Method=GetEvent&EventID={eventId}&Runners=true";
        }

        private string constructMeetingsUrl()
        {
            string sessionId = argList["sessionid"];
            string meetingId = argList["meetingid"];
            return $"{feedUrl}GetData.asp?SessionID={sessionId}&Method=GetMeeting&MeetingID={meetingId}&Runners=true";
        }

        private string constructAllMeetingsUrl()
        {
            string sessionId = argList["sessionid"];
            string date = argList["date"];
            return $"{feedUrl}GetData.asp?SessionID={sessionId}&Method=GetMeetingsAll&Date={date}&Types=R&Runners=true";
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