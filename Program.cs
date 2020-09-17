using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System;
using OpenQA.Selenium.Chrome;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.IO;

namespace AmmoScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            TargetSportsUSAScraper targetSportsUSAScraper = new TargetSportsUSAScraper();
            targetSportsUSAScraper.StartScrape();

        }
    }
}
