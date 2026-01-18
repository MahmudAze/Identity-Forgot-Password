using MainBackend.Data;
using MainBackend.Services;
using MainBackend.ViewModels;
using MainBackend.ViewModels.BasketVMs;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MainBackend.ViewComponents
{
    public class HeaderViewComponent : ViewComponent
    {
        private readonly LayoutService _layoutService;

        public HeaderViewComponent(LayoutService layoutService)
        {
            _layoutService = layoutService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var basketProduct = HttpContext.Request.Cookies["basket"];
            int count = 0;
            decimal price = 0;

            if (basketProduct != null)
            {
                List<BasketVM> basket = JsonConvert.DeserializeObject<List<BasketVM>>(basketProduct);

                count = basket.Sum(m => m.Count);
                price = basket.Sum(m => m.Price * m.Count);
            }

            Dictionary<string, string> settings = await _layoutService.GetAllSetting();

            HeaderVM headerVM = new()
            {
                Settings = settings,
                BasketCount = count,
                BasketPrice = price
            };

            return await Task.FromResult(View(headerVM));
        }
    }
}
