namespace MainBackend.Areas.Admin.ViewModels.ProductVMs
{
    public class GetAllProductVM
    {
        public int Id { get; set; }
        public string MainImage { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string CategoryName { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }
}
