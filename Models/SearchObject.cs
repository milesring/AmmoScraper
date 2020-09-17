using AmmoScraper.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmmoScraper.Models
{
    public class SearchObject
    {
        private List<Product> instockProducts;
        private List<Product> matchedProducts;
        public string URL { get; set; }

        public bool MatchedProductsChanged { get; set; }
        public string[] SearchTerms { get; set; }
        public string[] OmitTerms { get; set; }
        public List<Product> InStockProducts
        {
            get
            {
                if(instockProducts == null)
                {
                    instockProducts = new List<Product>();
                }
                return instockProducts;
            }
            set
            {
                instockProducts = value;
            }
        }
        public List<Product> MatchedProducts
        {
            get
            {
                if (matchedProducts == null)
                {
                    matchedProducts = new List<Product>();
                }
                return matchedProducts;
            }
            set
            {
                matchedProducts = value;
            }
        }
    }
}
