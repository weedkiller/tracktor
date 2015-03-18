using System.Web;
using System.Web.Optimization;

namespace tracktor.web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/base").Include(
                        "~/scripts/jquery-{version}.js",
                        "~/scripts/modernizr-*",
                        "~/scripts/bootstrap.js",
                        "~/scripts/bootbox.js",
                        "~/scripts/respond.js",
                        "~/scripts/moment.js",
                        "~/scripts/bootstrap-datetimepicker.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/signin").Include(
                        "~/scripts/app/definitions.js",
                        "~/scripts/app/signin.js"));

            bundles.Add(new ScriptBundle("~/bundles/tracktor").Include(
                        "~/scripts/knockout-{version}.js",
                        "~/scripts/knockout.mapping-latest.js",
                        "~/scripts/app/definitions.js",
                        "~/scripts/app/helpers.js",
                        "~/scripts/app/model.js",
                        "~/scripts/app/tracktor.js"));

            bundles.Add(new StyleBundle("~/content/css").Include(
                      "~/content/bootstrap.css",
                      "~/content/bootstrap-datetimepicker.min.css",
                      "~/content/site.css"));
        }
    }
}
