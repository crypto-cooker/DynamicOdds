using System.Collections.Generic;
using NDesk.Options;

namespace DOFeed
{
    internal class ArgumentHandler
    {
        private string[] args;
        Dictionary<string, string> argList;

        public ArgumentHandler(string[] args)
        {
            this.args = args;
            argList = new Dictionary<string, string>();
            argList["outfile"] = "";
            argList["sessionid"] = "";
            argList["method"] = "";
            argList["date"] = "";
            argList["user"] = "";
            argList["pass"] = "";
            argList["id"] = "";
            argList["eventid"] = "";
            argList["exotictype"] = "";
            argList["baid"] = "";
            argList["timestamp"] = "0";
            argList["official"] = "";
        }

        internal Dictionary<string, string> ProcessArgs()
        {
            if (args == null)
            {
                return argList;
            }
            var p = new OptionSet()
            {
                {
                    "f|file=", "the output file name with path.",
                        v => argList["outfile"] = v
                },
                {
                    "m|method=", "the method or function to call.",
                        v => argList["method"] = v
                },
                {
                    "s|sessionid=", "the login session.",
                        v => argList["sessionid"] = v
                },
                {
                    "d|date=", "date of the meeting (yyyy-mm-dd).",
                        v => argList["date"] = v
                },
                {
                    "p|pass=", "login password.",
                        v => argList["pass"] = v
                },
                {
                    "u|user=", "login user.",
                        v => argList["user"] = v
                },
                {
                    "i|id=", "ID of a particular meeting.",
                        v => argList["id"] = v
                },
                {
                    "e|eventid=", "ID of a particular event (race).",
                        v => argList["eventid"] = v
                },
                {
                    "x|exotictype=", "Use QQ or EX to indicate whether to display Quinella or Exacta odds.",
                        v => argList["exotictype"] = v
                },
                {
                    "b|baid=", "Betting Agency Identifier - list can be retrieved via the GetBettingAgencies method",
                        v => argList["baid"] = v
                },
                {
                    "t|timestamp=", "the latest timestamp.",
                        v => argList["timestamp"] = v
                },
                {
                    "o|official=", "the official prices.",
                        v => argList["official"] = v
                },

            };
            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException)
            {
                argList["outfile"] = "";
                argList["method"] = "";
                argList["sessionid"] = "";
                argList["date"] = "";
                argList["user"] = "";
                argList["pass"] = "";
                argList["id"] = "";
                argList["eventid"] = "";
                argList["exotictype"] = "";
                argList["baid"] = "";
                argList["timestamp"] = "0";
                argList["officail"] = "";
            }
            return argList;
        }
    }
}