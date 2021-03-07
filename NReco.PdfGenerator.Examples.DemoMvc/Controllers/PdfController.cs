using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Hosting;
using NReco.PdfGenerator;
using System.Web;

namespace NReco.PdfGenerator.Examples.LiveStreamMvc
{

    public class PdfController : Controller
    {
        private const string CssFileName = "Print.css";
        private const string Meta = "<meta http-equiv='Content-Type' content='text/html; charset=utf-8'/>";
        private static readonly List<string> FontList = new List<string> { "BNazanin", "SGKara-Light", "SGKara-Regular" };
        private static readonly List<string> FontExtentionList = new List<string> { ".eot", ".ttf", ".woff" };

        public ActionResult DemoPage()
        {
            return View();
        }

        [ValidateInput(false)]
        public ActionResult GeneratePdf(string htmlContent, string htmlUrl)
        {
            //htmlToPdf.LogReceived += (sender, e) => { Console.WriteLine("WkHtmlToPdf Log: {0}", e.Data); };
            //var footer = System.IO.File.ReadAllText(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "Contents", "Footer.html"));
            //var header = System.IO.File.ReadAllText(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "Contents", "Header.html"));

            ////var imagePath = Server.MapPath("~/Contents/11.jpg");
            ////htmlToPdf.PageHeaderHtml = $"<img src='{imagePath}' >";
            //htmlToPdf.PageHeaderHtml = header;
            //htmlToPdf.PageFooterHtml = footer;
            //htmlToPdf.Orientation = PageOrientation.Portrait;
            //htmlToPdf.Size = PageSize.A4;
            ////htmlToPdf.Margins.Top = 15;
            ////https://wkhtmltopdf.org/usage/wkhtmltopdf.txt
            //htmlToPdf.CustomWkHtmlArgs = "--margin-top 15"; //this is equivalent with htmlToPdf.Margins.Top = 15;
            //htmlToPdf.CustomWkHtmlPageArgs = "--page-offset 3";


            //var pdfContentType = "application/pdf";
            //if (string.IsNullOrEmpty(htmlContent) && string.IsNullOrEmpty(htmlUrl))
            //{
            //    Response.AddHeader("Content-Disposition", "inline; filename=" + "test.pdf");
            //    return File(GeneratePDFWithHeaderAndFooter(htmlToPdf), pdfContentType);
            //}
            //else if (!IsNullOrEmpty(htmlUrl))
            //{

            //    return File(htmlToPdf.GeneratePdfFromFile(htmlUrl, null), pdfContentType);
            //}
            //else
            //{
            //    //Download Pdf File
            //    //Show Pdf Directly in browser
            //    //https://stackoverflow.com/questions/14714486/how-can-i-open-a-pdf-file-directly-in-my-browser
            //    Response.AddHeader("Content-Disposition", "inline; filename=" + "test.pdf");
            //    return File(htmlToPdf.GeneratePdf(htmlContent, null), pdfContentType);
            //}

            var showHeaderAndFooter = true;

            //var footerWithoutBanner = new StreamReader(Assembly.Load("NReco.PdfGenerator.Examples.LiveStreamMvc").GetManifestResourceStream("NReco.PdfGenerator.Examples.LiveStreamMvc.Print.Templates.FooterWithoutBanner.html"));

            //var headerWithoutBanner = new StreamReader(Assembly.Load("SysteNReco.PdfGenerator.Examples.LiveStreamMvc")
            //    .GetManifestResourceStream("NReco.PdfGenerator.Examples.LiveStreamMvc.Print.Templates.HeaderWithoutBanner.html"));

            var headerWithBanner = GetHeaderBanner();

            var footerWithBanner = GetFooterBanner();

            var withoutHeadAndFooterMargin = new PageMargins()
            {
                Top = 31,
                Bottom = 20,
                Left = 23,
                Right = 23,
            };

            var witHeadAndFooterMargin = new PageMargins()
            {
                Top = 25,
                Bottom = 25,
                Left = 20,
                Right = 20,
            };


            var htmlToPdf = new HtmlToPdfConverter
            {
                Orientation = PageOrientation.Portrait,
                Size = PageSize.A4,
                Margins = witHeadAndFooterMargin,
                PageFooterHtml = footerWithBanner,
                PageHeaderHtml = headerWithBanner,
            };
            Directory.CreateDirectory(System.Web.HttpContext.Current.Server.MapPath("~/PrintTemp"));
            Directory.CreateDirectory(System.Web.HttpContext.Current.Server.MapPath("~/PrintTemp/Fonts"));
            Directory.CreateDirectory(System.Web.HttpContext.Current.Server.MapPath("~/PrintTemp/Css"));

            LoadFonts();
            LoadStyleSheet();

            string htmlTempPath = System.Web.HttpContext.Current.Server.MapPath("~/PrintTemp/" + Guid.NewGuid().ToString().Substring(0, 6) + ".html");

            var html = LoadHtmlContent(htmlTempPath);

            html = $"{CreateStyleSheetLink()}{html}";

            if (html.ToLower().Contains(Meta.ToLower()) == false)
            {
                html = $"{Meta.ToLower()}{html}";
            }

            using (FileStream fs = new FileStream(htmlTempPath, FileMode.Create))
            {
                byte[] info = new UTF8Encoding(false).GetBytes(html);
                fs.Write(info, 0, info.Length);

            }
            htmlToPdf.Orientation = PageOrientation.Portrait;
            byte[] pdf = htmlToPdf.GeneratePdfFromFile(htmlTempPath, null);
            System.IO.File.Delete(htmlTempPath);
            var pdfContentType = "application/pdf";

            Response.AddHeader("Content-Disposition", "inline; filename=" + "test.pdf");
            return File(pdf, pdfContentType);
        }

        private static string GetFooterBanner()
        {
            var logoStream = Assembly.Load("NReco.PdfGenerator.Examples.LiveStreamMvc")
               .GetManifestResourceStream($"NReco.PdfGenerator.Examples.LiveStreamMvc.Contents.footer.png");
            string base64Image;
            using (var memoryStream = new MemoryStream())
            {
                logoStream?.CopyTo(memoryStream);
                base64Image = Convert.ToBase64String(memoryStream.ToArray());
            }

            var footerWithBanner = new StreamReader(Assembly.Load("NReco.PdfGenerator.Examples.LiveStreamMvc")
                .GetManifestResourceStream("NReco.PdfGenerator.Examples.LiveStreamMvc.Contents.Footer.html") ??
                                                       new MemoryStream()).ReadToEnd();
            footerWithBanner = footerWithBanner.Replace("[BannerAddress]", $"data:image/png;base64,{base64Image}");
            return footerWithBanner;
        }

        private static string GetHeaderBanner()
        {
            var logoStream = Assembly.Load("NReco.PdfGenerator.Examples.LiveStreamMvc")
                .GetManifestResourceStream($"NReco.PdfGenerator.Examples.LiveStreamMvc.Contents.header.png");
            string base64Image;
            using (var memoryStream = new MemoryStream())
            {
                logoStream?.CopyTo(memoryStream);
                base64Image = Convert.ToBase64String(memoryStream.ToArray());
            }

            var headerWithBanner = new StreamReader(Assembly.Load("NReco.PdfGenerator.Examples.LiveStreamMvc")
              .GetManifestResourceStream("NReco.PdfGenerator.Examples.LiveStreamMvc.Contents.Header.html") ??

                                                     new MemoryStream()).ReadToEnd();
            headerWithBanner = headerWithBanner.Replace("[BannerAddress]", $"data:image/png;base64,{base64Image}");

            return headerWithBanner;
        }


        private static void LoadStyleSheet()
        {
            var cssPath = System.Web.HttpContext.Current.Server.MapPath($"~/PrintTemp/Css/{CssFileName}");
            if (System.IO.File.Exists(cssPath))
                return;
            var file = Assembly.Load("NReco.PdfGenerator.Examples.LiveStreamMvc").GetManifestResourceStream($"NReco.PdfGenerator.Examples.LiveStreamMvc.Contents.{CssFileName}");
            if (file == null)
                return;
            var cssStream = System.IO.File.Create(cssPath);
            file.Seek(0, SeekOrigin.Begin);
            file.CopyTo(cssStream);
            cssStream.Close();
        }

        private static void LoadFonts()
        {
            foreach (string font in FontList)
            {
                foreach (var extention in FontExtentionList)
                {
                    var fontPath = System.Web.HttpContext.Current.Server.MapPath($"~/PrintTemp/Fonts/{font}{extention}");
                    if (System.IO.File.Exists(fontPath))
                        continue;

                    var file = Assembly.Load("NReco.PdfGenerator.Examples.LiveStreamMvc").GetManifestResourceStream($"NReco.PdfGenerator.Examples.LiveStreamMvc.Contents.Fonts.{font}{extention}");
                    if (file == null)
                        continue;

                    var fontStream = System.IO.File.Create(fontPath);
                    file.Seek(0, SeekOrigin.Begin);
                    file.CopyTo(fontStream);
                    fontStream.Close();
                }
            }
        }

        private static string CreateStyleSheetLink()
        {
            return $"<link rel='stylesheet' type='text/css' href='Css/{CssFileName}'>";
        }

        //private byte[] GeneratePDFWithHeaderAndFooter(HtmlToPdfConverter htmlToPdf)
        //{
        //    LoadImage();
        //    var htmlPath = LoadHtmlContent();
        //    htmlToPdf.Orientation = PageOrientation.Landscape;
        //    var file = htmlToPdf.GeneratePdfFromFile(htmlPath, null);
        //    return file;
        //}

        private string LoadHtmlContent(string path)
        {
            Directory.CreateDirectory(Server.MapPath("~/Temp"));
            
            var html = System.IO.File.ReadAllText(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "Contents", "HTMLPage1.html"));
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(html);
                fs.Write(info, 0, info.Length);
            }
            return html;
        }

        private void LoadImage()
        {
            Directory.CreateDirectory(Server.MapPath("~/Temp/Images"));

            var imageName = "test2.png";
            var image = System.IO.File.ReadAllBytes(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "Contents", imageName));

            string path = Server.MapPath("~/Temp/Images/" + imageName);
            if (System.IO.File.Exists(path))
                return;
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                fs.Write(image, 0, image.Length);
            }
        }
    }
}
