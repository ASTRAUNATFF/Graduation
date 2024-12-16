using Microsoft.AspNetCore.Mvc;

namespace Kindergarten_school.ViewComponents
{
    [ViewComponent(Name = "FooterMain")]
    public class FooterComponent : ViewComponent
    {

        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
