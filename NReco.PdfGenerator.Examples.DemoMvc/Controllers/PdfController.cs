using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.IO;
using System.Net;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.UI;

using System.Web.Security;

using NReco.PdfGenerator;
using static System.String;

namespace Controllers
{

    public class PdfController : Controller
    {

        public ActionResult DemoPage()
        {
            return View();
        }

        [ValidateInput(false)]
        public ActionResult GeneratePdf(string htmlContent, string htmlUrl)
        {
            var htmlToPdf = new HtmlToPdfConverter();

            htmlToPdf.LogReceived += (sender, e) => { Console.WriteLine("WkHtmlToPdf Log: {0}", e.Data); };
            var footer = System.IO.File.ReadAllText(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "Contents", "Footer.html"));
            var header = System.IO.File.ReadAllText(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "Contents", "Header.html"));

            //var imagePath = Server.MapPath("~/Contents/11.jpg");
            //htmlToPdf.PageHeaderHtml = $"<img src='{imagePath}' >";
            htmlToPdf.PageHeaderHtml = header;
            htmlToPdf.PageFooterHtml = footer;
            htmlToPdf.Orientation = PageOrientation.Portrait;
            htmlToPdf.Size = PageSize.A4;
            //htmlToPdf.Margins.Top = 15;
            //https://wkhtmltopdf.org/usage/wkhtmltopdf.txt
            htmlToPdf.CustomWkHtmlArgs = "--margin-top 15"; //this is equivalent with htmlToPdf.Margins.Top = 15;
            htmlToPdf.CustomWkHtmlPageArgs = "--page-offset 3";
            

            var pdfContentType = "application/pdf";
            if (string.IsNullOrEmpty(htmlContent) && string.IsNullOrEmpty(htmlUrl))
            {
                Response.AddHeader("Content-Disposition", "inline; filename=" + "test.pdf");
                return File(GeneratePDFWithHeaderAndFooter(htmlToPdf), pdfContentType);
            }
            else if (!IsNullOrEmpty(htmlUrl))
            {

                return File(htmlToPdf.GeneratePdfFromFile(htmlUrl, null), pdfContentType);
            }
            else
            {
                //Download Pdf File
                //Show Pdf Directly in browser
                //https://stackoverflow.com/questions/14714486/how-can-i-open-a-pdf-file-directly-in-my-browser
                Response.AddHeader("Content-Disposition", "inline; filename=" + "test.pdf");
                return File(htmlToPdf.GeneratePdf(htmlContent, null), pdfContentType);
            }
        }


        private byte[] GeneratePDFWithHeaderAndFooter(HtmlToPdfConverter htmlToPdf)
        {
            LoadImage();
            var htmlPath = LoadHtmlContent();
            htmlToPdf.Orientation = PageOrientation.Landscape;
            var file = htmlToPdf.GeneratePdfFromFile(htmlPath, null);
            return file;
        }

        private string LoadHtmlContent()
        {
            Directory.CreateDirectory(Server.MapPath("~/Temp"));

            string path = Server.MapPath("~/Temp/PDF" + Guid.NewGuid().ToString().Substring(0, 6) + ".html");
            var html = System.IO.File.ReadAllText(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "Contents", "HTMLPage1.html"));
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(html);
                fs.Write(info, 0, info.Length);
            }
            return path;
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
