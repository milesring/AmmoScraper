using AmmoScraper.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AmmoScraper
{
    public class DataStore
    {
        private SQLiteConnection LocalDB { get; set; }
        public DataStore(string db)
        {
            LocalDB = new SQLiteConnection(string.Format("{0}", db));
            LocalDB.CreateTable<Product>();
            LocalDB.CreateTable<PriceHistory>();
        }


        public int SaveProduct(Product p)
        {
            if(p == null)
            {
                return 0;
            }
            if(p.Id != 0)
            {
                return LocalDB.Update(p);
            }
            else
            {
                return LocalDB.Insert(p);
            }
        }

        public Product GetProduct(int id)
        {
            return LocalDB.Table<Product>().Where(i => i.Id == id).FirstOrDefault();
        }

        public Product GetProduct(string url)
        {
            return LocalDB.Table<Product>().Where(i => i.URL.Equals(url)).FirstOrDefault();
        }

        public int DeleteProduct(Product p)
        {
            return LocalDB.Delete(p);
        }

        public int SavePriceHistory(PriceHistory priceHistory)
        {
            if(priceHistory == null)
            {
                return 0;
            }

            if(priceHistory.Id != 0)
            {
                return LocalDB.Update(priceHistory);
            }
            else
            {
                return LocalDB.Insert(priceHistory);
            }
        }

        public List<PriceHistory> GetPriceHistory(int id)
        {
            return LocalDB.Table<PriceHistory>().Where(i => i.ProductId == id).Select(i => i).ToList();
        }

        public PriceHistory GetLowestPrice(int id)
        {
            var result = LocalDB.Table<PriceHistory>().Where(x => x.ProductId == id);
            result.OrderBy(x => x.PricePerRound);
            return result.First();
        }
    }
}
