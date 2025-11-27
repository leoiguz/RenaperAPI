using System.Web.Optimization;

namespace RenaperWeb
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // JQUERY (para validaciones standard de MVC)
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            // BUNDLE DE Renaper.css
            bundles.Add(new StyleBundle("~/Content/renaper").Include(
                      "~/Content/Renaper.css"));
        }
    }
}