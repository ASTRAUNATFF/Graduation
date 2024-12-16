using Microsoft.AspNetCore.Mvc;

namespace Kindergarten_school.ViewComponents
{
    [ViewComponent(Name = "HeaderMain")]
    public class MenuComponent : ViewComponent
    {

        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
