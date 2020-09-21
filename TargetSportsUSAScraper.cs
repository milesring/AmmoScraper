using AmmoScraper.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace AmmoScraper
{
    public class TargetSportsUSAScraper
    {
        private string dbPath = "TSUSA.db";
        private DataStore DataStore { get; set; }
        //TSUSA robots.txt states 1 request per 10 seconds
        private int ScrapeRate { get; set; } = 60000;
        private int NumScrapes { get; set; }
        private List<SearchObject> Searches { get; set; }
        private GmailService GmailService { get; set; }
        private Timer ScrapeTimer { get; set; }
        private ConcurrentQueue<(SearchObject, WebDriver)> SearchQueue { get; set; }
        private List<Product> SavedInStockProducts { get; set; }

        //debug properties
        private bool UseLocal { get; set; } = false;
        private bool SendEmail { get; set; } = true;

        public TargetSportsUSAScraper()
        {
            Searches = LoadSearches();
            if(Searches == null)
            {
                Environment.Exit(-1);
            }
            GmailService = new GmailService();
            if (!GmailService.Configured)
            {
                Environment.Exit(-1);
            }
            DataStore = new DataStore(dbPath);
            SearchQueue = new ConcurrentQueue<(SearchObject, WebDriver)>();
            SavedInStockProducts = new List<Product>();
        }
        public void StartScrape()
        {
            Console.WriteLine("{0} - Starting Scrape", DateTime.Now);
            var webDriver = new WebDriver();
            foreach (var search in Searches)
            {
                //prep scrapes
                SearchQueue.Enqueue((search, webDriver));
            }
            var autoEvent = new AutoResetEvent(false);
            Console.WriteLine("{0} - Creating timer with a {1, 2} ms delay", DateTime.Now, ScrapeRate);
            ScrapeTimer = new Timer(TimerScrape, autoEvent, 0, ScrapeRate);
            autoEvent.WaitOne();
        }
        List<SearchObject> LoadSearches()
        {
            string file;
            try
            {
                file = File.ReadAllText(@".\TargetSportsSearches.txt");
            }catch(FileNotFoundException e)
            {
                Console.WriteLine("{0} - SCRAPING INFO NOT FOUND, PLACEHOLDER INFO ADDED TO TargetSportsSearches.txt AT ROOT");
                var searchObject = new SearchObject();
                searchObject.URL = $"https://www.PLACEURL1HERE.com";
                searchObject.SearchTerms = new string[] { "keyterm1", "keyterm2", "keyterm3" };
                searchObject.OmitTerms = new string[] { "omitterm1", "omitterm2" };


                List<SearchObject> tempSearches = new List<SearchObject>();
                tempSearches.Add(searchObject);
                searchObject = new SearchObject();
                searchObject.URL = $"https://www.PLACEURL2HERE.com";
                searchObject.SearchTerms = new string[] { "keyterm1", "keyterm2", "keyterm3" };
                searchObject.OmitTerms = new string[] { "omitterm1", "omitterm2" };
                tempSearches.Add(searchObject);

                var json = JsonConvert.SerializeObject(tempSearches, Formatting.Indented);
                File.WriteAllText(@".\TargetSportsSearches.txt", json);
                file = File.ReadAllText(@".\TargetSportsSearches.txt");
            }
            var searches = JsonConvert.DeserializeObject<List<SearchObject>>(file);
            var result = searches.Where(x => x.URL.Contains("PLACEURL"));
            if(result.Count() > 0)
            {
                Console.WriteLine("{0} - PLACEHOLDER SEARCH FOUND, PLEASE REMOVE ALL PLACEHOLDER INFO FROM TargetSportsSearches.txt AT ROOT");
                return null;
            }
            return searches;
        }
        void TimerScrape(Object stateInfo)
        {
            (SearchObject, WebDriver) dequeVal;
            var result = SearchQueue.TryDequeue(out dequeVal);
            if (!result)
            {
                return;
            }
            Console.WriteLine("{0} - {1} - Starting scrape {2}", DateTime.Now, NumScrapes, dequeVal.Item1.URL);
            //AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
            ScrapeCycle(dequeVal);
        }
        void ScrapeCycle((SearchObject,WebDriver) search)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            
            //use local file for debug or use live web
            if (UseLocal)
            {
                search.Item2.Navigate(@"C:\Users\Miles\source\repos\AmmoScraper\page.html");
                var htmlBody = search.Item2.PageSource();
                htmlDocument.LoadHtml(htmlBody);
            }
            else
            {
                try
                {
                    search.Item2.Navigate(search.Item1.URL);
                }catch(OpenQA.Selenium.WebDriverException e)
                {
                    Console.WriteLine("{0} - Exception thrown in scrape cycle from web driver navigate:\n{1}", DateTime.Now, e.ToString());
                }
                var htmlBody = search.Item2.PageSource();
                htmlDocument.LoadHtml(htmlBody);
            }

            //gather products
            var productList = htmlDocument.DocumentNode.SelectNodes("//*[@class = 'product-list']/*");

            //check for all in stock products
            try
            {
                CheckInStockProducts(search.Item1, productList);
            }
            catch(Exception e)
            {
                Console.WriteLine("{0} - Parsing in stock products for {1} failed. Returning from cycle\n Stack Trace: {2} ", DateTime.Now, search.Item1.URL, e.ToString());
                SearchQueue.Enqueue(search);
                return;
            }

            //filter in stock products
            try
            {
                FilterInStockProducts(search.Item1);
            }catch(Exception e)
            {
                Console.WriteLine("{0} - Filtering in stock products for {1} failed. Returning from cycle\n Stack Trace: {2} ", DateTime.Now, search.Item1.URL, e.ToString());
                return;
            }

            //send email
            if (SendEmail)
            {
                SendNotificationEmail(search.Item1);
            }
            else
            {
                if(search.Item1.MatchedProducts.Count > 0)
                {
                    Console.WriteLine("{0} - Matches in {1}: ", DateTime.Now, search.Item1.URL);
                    foreach (var match in search.Item1.MatchedProducts)
                    {
                        Console.WriteLine("\t\t{0}", match.ToString());
                    }
                }
                else
                {
                    Console.WriteLine("{0} - No Matches for {1}: ", DateTime.Now, search.Item1.URL);
                }

            }

            //requeue the search
            SearchQueue.Enqueue(search);
            NumScrapes++;
        }
        void CheckInStockProducts(SearchObject search, HtmlNodeCollection productList)
        {
            if(productList == null)
            {
                return;
            }
            for (int i = 0; i < productList.Count; i++)
            {
                //filter for specific key on for in stock notification
                if (productList[i].InnerHtml.Contains("add-to-cart", StringComparison.OrdinalIgnoreCase) && !productList[i].InnerHtml.Contains("add-to-cart stockNotify", StringComparison.OrdinalIgnoreCase))
                {
                    var product = new Product();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(productList[i].InnerHtml);

                    var url = doc.DocumentNode.SelectSingleNode("//a");
                    product.URL = "https://www.targetsportsusa.com" + url.Attributes[0].Value;
                    var result = DataStore.GetProduct(product.URL);
                    //product exists in database
                    if(result != null)
                    {
                        product = result;
                    }
                    //new product, add details
                    else
                    {
                        var brand = doc.DocumentNode.SelectSingleNode("//h2/strong");
                        product.Brand = brand.InnerText;

                        var descNode = doc.DocumentNode.SelectSingleNode("//h2");
                        descNode.RemoveChild(descNode.FirstChild);
                        product.Description = descNode.InnerText;
                    }

                    //always update pricing
                    var priceNode = doc.DocumentNode.SelectSingleNode("//div[@class = 'product-listing-price']");
                    var nfi = NumberFormatInfo.CurrentInfo;
                    string pattern;
                    pattern = "[0-9]+";
                    pattern += Regex.Escape(nfi.CurrencyDecimalSeparator);
                    pattern += "[0-9]+";
                    var matches = Regex.Matches(priceNode.InnerText, pattern);
                    product.Price = double.Parse(matches[0].ToString());
                    product.PricePerRound = double.Parse(matches[1].ToString());

                    //save product
                    DataStore.SaveProduct(product);

                    //get id of newly saved product
                    if(result == null)
                    {
                        result = DataStore.GetProduct(product.URL);
                        product.Id = result.Id;
                    }
                    //get all price history for 
                    var priceHistory = DataStore.GetPriceHistory(product.Id);
                    
                    //check if history at current price exists
                    var results = priceHistory.Where(x => x.PricePerRound == product.PricePerRound).FirstOrDefault();

                    //create new price record
                    if(results == null)
                    {
                        results = new PriceHistory();
                        results.ProductId = product.Id;
                        results.Price = product.Price;
                        results.PricePerRound = product.PricePerRound;
                        results.DateTime = DateTime.Now;
                    }
                    //update old price record with most recent occurance
                    else
                    {
                        results.DateTime = DateTime.Now;
                    }
                    DataStore.SavePriceHistory(results);

                    
                    var lowest = DataStore.GetLowestPrice(product.Id);
                    product.HistoricLow = lowest.PricePerRound;
                    product.HistoricLowDate = lowest.DateTime;

                    //update item with historic low property
                    DataStore.SaveProduct(product);

                    if (search.InStockProducts.Exists(x => !(x.Id == product.Id))){
                        search.InStockProducts.Add(product);
                    }
                }
            }
        }
        void FilterInStockProducts(SearchObject search)
        {
            if(search.InStockProducts == null || search.InStockProducts.Count < 1)
            {
                return;
            }
            search.MatchedProductsChanged = false;
            foreach (var term in search.SearchTerms)
            {

                var results = search.InStockProducts.Where(
                    x => x.Description.Contains(term, StringComparison.OrdinalIgnoreCase)
                    && !search.OmitTerms.Any(
                        n => x.Description.Contains(n, StringComparison.OrdinalIgnoreCase)
                        )
                    );
                if (results.Count() != 0)
                {

                    //check for new matched products
                    var newDifference = results.Except(search.MatchedProducts).ToList();

                    //check for previously in stock products that are now out of stock
                    var oldDifference = search.MatchedProducts.Except(results).ToList();

                    if(newDifference.Count() > 0)
                    {
                        search.MatchedProductsChanged = true;
                        //add current instock products to search object
                        search.MatchedProducts.AddRange(results);
                    }
                    if(oldDifference.Count() > 0)
                    {
                        search.MatchedProductsChanged = true;
                        //remove old products
                        foreach(var oldProduct in oldDifference)
                        {
                            search.MatchedProducts.Remove(oldProduct);
                        }
                    }
                }

            }
        }
        void SendNotificationEmail(SearchObject search)
        {
            if (search.MatchedProducts.Count > 0 && search.MatchedProductsChanged)
            {
                string message = "";
                
                foreach (var product in search.MatchedProducts)
                {
                    message += product.ToString();
                    message += "\n\n";
                }
                GmailService.SendEmail(message);
            }
        }
    }
}
