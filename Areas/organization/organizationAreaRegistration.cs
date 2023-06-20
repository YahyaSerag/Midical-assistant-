using System.Web.Mvc;

namespace WebApplication8.Areas.organization
{
    public class organizationAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "organization";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "organization_default",
                "organization/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}