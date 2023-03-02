using ImageUploader.Data;
using ImageUploader.Models;
using ImageUploader.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ImageUploader.Controllers
{
    [Route("api/image")]
    [ApiController]
    public class ImageController : ControllerBase
    {

        private readonly IWebHostEnvironment _webhost;
        private readonly IFileDetailRepository _fileDetailRepository;
        public ImageController(IWebHostEnvironment webhost, IFileDetailRepository fileDetailRepository)
        {
            _webhost = webhost;
            _fileDetailRepository = fileDetailRepository;
        }
        public IActionResult Get()
        {
            List<FileDetail> fileDetails = new List<FileDetail>();
            fileDetails = _fileDetailRepository.GetFileDetails();
            return Ok(fileDetails);
        }
        [HttpPost]
        public async Task<IActionResult> Post(IFormFile file)
        {

            var image = await new ImageService(_webhost).SaveFile(file);
            if (image == "Not Ok")
            {
                return BadRequest();

            }
            else
            {
                var dict = new Dictionary<string, string>()
                {
                    {"fileName",file.FileName },
                    {"folderName", _webhost.WebRootPath+ "images" },
                    {"fileSize",file.Length+" bytes" }
                };
                string json =JsonConvert.SerializeObject(dict);
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://imageu.azurewebsites.net/api/ImageUpload?code=ca-4OOnBTnY7UaJiepi-iO-QB8yA8iZiz3ZawalxPnDqAzFuk_XhoA==");
                req.Method = "POST";
                req.ContentType = "application/json";
                Stream stream = req.GetRequestStream();
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                stream.Write(buffer, 0, buffer.Length);
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                string response;
                using (var reader = new StreamReader(res.GetResponseStream(), Encoding.ASCII))
                {
                    response = reader.ReadToEnd();
                }
                return Ok(response);
            }
        }
    }
}
