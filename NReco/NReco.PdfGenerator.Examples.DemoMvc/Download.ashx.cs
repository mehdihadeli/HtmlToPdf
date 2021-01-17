using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace NReco.PdfGenerator.Examples.LiveStreamMvc
{
    /// <summary>
    /// Summary description for Download
    /// </summary>
    public class Download : IHttpHandler
    {
        private const string Meta = "<meta http-equiv='Content-Type' content='text/html; charset=utf-8'/>";
        private const string CssFileName = "StyleSheet1.css";

        public void ProcessRequest(HttpContext context)
        {
            var htmlToPdf = new HtmlToPdfConverter();
            htmlToPdf.Quiet = false;
            string htmlCode = @"
<html>
<head>
</head>
<body>

<p>تست</p>
<p>This is a paragraph.</p>
<p>This is a paragraph.</p>

</body>
</html>";


            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/Temp"));
            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/Temp/Fonts"));
            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/Temp/Css"));

            string path = HttpContext.Current.Server.MapPath("~/Temp/PDF" + Guid.NewGuid().ToString().Substring(0, 6) + ".html");

            htmlCode = $"{CreateStyleSheetLink()}{htmlCode}";
            LoadFonts();
            LoadStyleSheet();

            if (htmlCode.ToLower().Contains(Meta.ToLower()) == false)
            {
                htmlCode = $"{Meta.ToLower()}{htmlCode}";
            }

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(htmlCode);
                fs.Write(info, 0, info.Length);

            }

            htmlToPdf.Orientation = NReco.PdfGenerator.PageOrientation.Landscape;
            var pdf = htmlToPdf.GeneratePdfFromFile(path, null);



            //var pdf = htmlToPdf.GeneratePdf(htmlCode, null);
            string fileName = "HelloWorld.pdf";
            context.Response.ContentType = "application/pdf";
            context.Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            context.Response.BinaryWrite(pdf);
            context.Response.End();
        }

        public bool IsReusable
        {
            get { return false; }
        }

        private static void LoadStyleSheet()
        {
            var css = System.IO.File.ReadAllText(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "Contents", CssFileName));
            string path = HttpContext.Current.Server.MapPath("~/Temp/Css/" + CssFileName);
            if (File.Exists(path))
                return;
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(css);
                fs.Write(info, 0, info.Length);
            }
        }

        private static void LoadFonts()
        {
            foreach (string filePath in Directory.EnumerateFiles(
                Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "Contents", "Fonts")))
            {
                byte[] contents = File.ReadAllBytes(filePath);
                string fontPath = HttpContext.Current.Server.MapPath("~/Temp/Fonts/" + Path.GetFileName(filePath));
                if (File.Exists(fontPath))
                    continue;
                using (FileStream fs = new FileStream(fontPath, FileMode.Create))
                {
                    fs.Write(contents, 0, contents.Length);
                }
            }
        }

        private string CreateStyleSheetLink()
        {
            return $"<link rel='stylesheet' type='text/css' href='css/{CssFileName}'>";
        }
    }
}

