using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Controllers
{
    public class AppleController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AppleController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        //public FileResult File1()
        //{
        //    //string webRootPath = _webHostEnvironment.WebRootPath;
        //    //string contentRootPath = _webHostEnvironment.ContentRootPath;

        //    //const string path = "/apple-app-site-association";
        //    //if (string.IsNullOrEmpty(path)) return null;

        //    //var vFullFileName = HostingEnvironment.MapPath(path);
        //    //if (vFullFileName == null) return null;

        //    //var fileBytes = System.IO.File.ReadAllBytes(vFullFileName);
        //    //// System.Net.Mime.MediaTypeNames.Application.Octet
        //    //return File(fileBytes, "application/json", "apple-app-site-association");
        //}
    }
}
