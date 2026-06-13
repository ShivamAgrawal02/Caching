namespace Caching.Model.DTOs
{
    public class ProductRequestDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }

        public int Stock { get; set; }
    }
}
