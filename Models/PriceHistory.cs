using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmmoScraper.Models
{
    public class PriceHistory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public DateTime DateTime { get; set; }
        public double Price { get; set; }
        public double PricePerRound { get; set; }
    }
}
