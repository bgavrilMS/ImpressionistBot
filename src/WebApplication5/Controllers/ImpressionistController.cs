using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;
using System.IO;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc.Rendering;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication5.Controllers
{
    public class ImpressionistController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        private IHostingEnvironment _environment;

        public ImpressionistController(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost]
        public async Task<IActionResult> PostImage(ICollection<IFormFile> files)
        {
            var uploads = Path.Combine(_environment.WebRootPath, "uploads");
            var formFile = files.ElementAt(0);

            string fileName = Guid.NewGuid() + ".jpg";
            formFile.SaveAs(fileName);

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            var result = UploadImage(filePath);

            using (FileStream fs = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "bla.jpg"), FileMode.Create))
            {
                await (result.Content as StreamContent).CopyToAsync(fs);
            }

            //HttpResponseMessage response = new HttpResponseMessage();
            //response.StatusCode = HttpStatusCode.OK;
            //response.Content = result.Content;
            //(response.Content as StreamContent).
            //response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            //{
            //    FileName = "foo.jpg"
            //};
            //response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            //return response;
            return View();
        }

        //private string SaveResponseImage(HttpResponseMessage response)
        //{
        //    var fileStream = System.IO.File.Create(Guid.NewGuid().ToString() + ".jpg");
        //    var stream = (response.Content as StreamContent);
        //    stream.Seek(0, SeekOrigin.Begin);
        //    myOtherObject.InputStream.CopyTo(fileStream);
        //    fileStream.Close();
        //}

        private HttpResponseMessage UploadImage(string file)
        {
            using (var client = new HttpClient())
            {
                var requestContent = new MultipartFormDataContent();
                //    here you can specify boundary if you need---^
                var imageContent = new ByteArrayContent(ImageToByteArray(file));
                imageContent.Headers.ContentType =
                    MediaTypeHeaderValue.Parse("image/jpeg");

                requestContent.Add(imageContent, "image", "image.jpg");
                string url = "https://functionsa2c4866e.azurewebsites.net/api/HttpTriggerCSharp1?code=jlnvuhy9vwz9hfwnogxa2lnmizy9cucy41rq15bu8yuz4u0udid7bnlwxyzg8b73zg06dewxw29";
                var result = client.PostAsync(url, requestContent).Result;

                return result;
            }
        }

        private static byte[] ImageToByteArray(string imageLocation)
        {
            byte[] imageData = null;
            FileInfo fileInfo = new FileInfo(imageLocation);
            long imageFileLength = fileInfo.Length;
            FileStream fs = new FileStream(imageLocation, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            imageData = br.ReadBytes((int)imageFileLength);
            return imageData;
        }
    }

    public static class HtmlHelperExtensions
    {
        private static string GetFileContentType(string path)
        {
            if (path.EndsWith(".JPG", StringComparison.OrdinalIgnoreCase) == true)
            {
                return "image/jpeg";
            }
            else if (path.EndsWith(".GIF", StringComparison.OrdinalIgnoreCase) == true)
            {
                return "image/gif";
            }
            else if (path.EndsWith(".PNG", StringComparison.OrdinalIgnoreCase) == true)
            {
                return "image/png";
            }

            throw new ArgumentException("Unknown file type");
        }

        public static HtmlString InlineImage(this IHtmlHelper html, string path, object attributes = null)
        {
            var contentType = GetFileContentType(path);
            var env = html.ViewContext.HttpContext.ApplicationServices.GetService(typeof(IHostingEnvironment)) as IHostingEnvironment;

            using (var stream = env.WebRootFileProvider.GetFileInfo(path).CreateReadStream())
            {
                var array = new byte[stream.Length];
                stream.Read(array, 0, array.Length);

                var base64 = Convert.ToBase64String(array);

                //var props = (attributes != null) ? attributes.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(attributes)) : null;

                //var attrs = (props == null)
                //    ? string.Empty
                //    : string.Join(" ", props.Select(x => string.Format("{0}=\"{1}\"", x.Key, x.Value)));

                var img = $"<img src=\"data:{contentType};base64,{base64}\" {string.Empty}/>";

                return new HtmlString(img);
            }
        }
    }
}
