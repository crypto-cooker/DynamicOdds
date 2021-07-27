using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOFeed
{
    class MainEntry
    {
        static void Main(string[] args)
        {
            ArgumentHandler ah = new ArgumentHandler(args);
            Dictionary<string, string> argList = ah.ProcessArgs();
            WebFeed wf = new WebFeed(argList);
            OutHandler oh = new OutHandler(argList["outfile"]);
            //oh.Write(wf.ProcessFeed());
            wf.ProcessFeed();
        }
    }
}
