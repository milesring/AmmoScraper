using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AmmoScraper
{
    public class WebDriver
    {
        ChromeDriver Driver { get; set; }
        public WebDriver()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");
            Driver = new ChromeDriver(chromeOptions);
        }

        public void Navigate(string URL)
        {
            Driver.Url = URL;
            Driver.Navigate();
        }

        public string PageSource()
        {
            return Driver.PageSource;
        }
    }
}
