namespace DataGenerator.Domain.Products.Models
{
    public class Product
    {
        public string Name { get; set; }

        public double DefaultVolume { get; set; }

        public double DefaultPeopleTime { get; set; }

        public long DefaultVolumeVariation { get; set; }

        public long VolumeVariationDivisor { get; set; }

        public long DefaultPeopleTimeVariation { get; set; }

        public long PeopleTimeVariationDivisor { get; set; }

        public Product()
        {
            Name = string.Empty;
        }
    }
}
