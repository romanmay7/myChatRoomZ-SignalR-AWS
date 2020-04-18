using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace myChatRoomZ_WebAPI.Data.Models
{
    public class S3Response
    {
        public HttpStatusCode Status { get; set; }
        public string Message { get; set; }
    }
}
