using System.Web;
using System.Web.Optimization;

namespace Inventory
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.min.css",
                      "~/bower_components/font-awesome/css/font-awesome.min.css",
                      "~/bower_components/Ionicons/css/ionicons.min.css",
                      "~/Content/AdminLTE.min.css",
                      "~/dist/css/skins/skin-blue.min.css",
                      "~/bower_components/morris.js/morris.css",
                      "~/bower_components/jvectormap/jquery-jvectormap.css",
                      "~/bower_components/bootstrap-datepicker/dist/css/bootstrap-datepicker.min.css",
                      "~/bower_components/bootstrap-daterangepicker/daterangepicker.css",
                      "~/plugins/bootstrap-wysihtml5/bootstrap3-wysihtml5.min.css"));

            bundles.Add(new ScriptBundle("~/bundles/ejscripts").Include(
                           "~/Scripts/jsrender.min.js",
                           "~/Scripts/jquery.easing-1.3.min.js",
                            "~/Scripts/ej/ej.web.all.min.js",
                            "~/Scripts/ej/ej.unobtrusive.min.js"));

            bundles.Add(new StyleBundle("~/bundles/ejstyles").Include(
                      "~/ejThemes/flat-saffron/ej.web.all.min.css"));
        }
    }
}
