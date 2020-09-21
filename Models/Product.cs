using SQLite;
using System;
using System.Diagnostics.CodeAnalysis;

namespace AmmoScraper.Models
{
    public class Product : IEquatable<Product>
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

        public int GetHashCode([DisallowNull] Product obj)
        {
            int hashId = Id.GetHashCode();
            int hashBrand = Brand == null ? 0 : Brand.GetHashCode();
            int hashDesc = Description == null ? 0 : Description.GetHashCode();
            int hashPrice = Price.GetHashCode();
            int hashPPR = PricePerRound.GetHashCode();
            int hashHistoricLow = HistoricLow.GetHashCode();
            int hashHistoricDate = HistoricLowDate.GetHashCode();
            int hashURL = URL == null ? 0 : URL.GetHashCode();
            return hashId ^ hashBrand ^ hashDesc ^ hashPrice ^ hashPPR ^ hashHistoricLow ^ hashHistoricDate ^ hashURL;

        }

        public bool Equals([AllowNull] Product other)
        {
            //Check whether the compared object is null.
            if (ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal.
            return Id == other.Id && URL.Equals(other.URL);
        }
    }
}
