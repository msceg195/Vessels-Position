using System.Web.Optimization;

namespace Vessels_Position
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/Scripts").Include(
                        "~/Scripts/jquery-3.3.1.min.js",
                        "~/Scripts/bootstrap.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));

            BundleTable.EnableOptimizations = true;
        }
    }
}