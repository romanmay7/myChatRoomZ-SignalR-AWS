using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using myChatRoomZ_WebAPI.Services;

namespace myChatRoomZ_WebAPI.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IS3Service _s3Service;

        public UploadController(IS3Service s3Service)
        {
            _s3Service = s3Service;
        }

        [HttpPost, DisableRequestSizeLimit]
        [Route("api/UploadImageAWS")]
        public async Task<IActionResult> UploadAWS()
        {
            string bucketName = "my-chat-roomz-images";

            try
            {
                var file = Request.Form.Files[0];


                if (file.Length > 0)
                {

                    await _s3Service.UploadfileAsync( file ,bucketName);


                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }



        [HttpPost, DisableRequestSizeLimit]
        [Route("api/UploadImage")]
        public IActionResult Upload()
        {
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Resources", "Images");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    return Ok(new { dbPath });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
    }
    
}
