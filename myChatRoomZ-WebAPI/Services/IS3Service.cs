using Microsoft.AspNetCore.Http;
using myChatRoomZ_WebAPI.Data.Models;
using System.Threading.Tasks;

namespace myChatRoomZ_WebAPI.Services
{
    public interface IS3Service
    {
        public  Task<S3Response> CreateBucketAsync(string bucketName);
        public Task UploadfileAsync(IFormFile file ,string bucketName);
    }
}