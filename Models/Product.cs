using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace AmmoScraper.Models
{
    public class Product
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Description { get; set; }

        public double Price { get; set; }
        public double PricePerRound { get; set; }

        public double HistoricLow { get; set; }
        public DateTime HistoricLowDate { get; set; }

        public string URL { get; set; }

        public override string ToString()
        {
            return string.Format("{0}\n{1}\n{2}\n${3} - ${4:0.00} Per Round (${5:0.00} for members)\nHistoric Low: ${6:0.00} on {7}",
                Brand,
                Description,
                URL,
                Price,
                PricePerRound,
                PricePerRound*.92,
                HistoricLow,
                HistoricLowDate);
        }
    }
}
