using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using Microsoft.AspNetCore.Http;
using myChatRoomZ_WebAPI.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace myChatRoomZ_WebAPI.Services
{
    public class S3Service: IS3Service
    {
        private readonly IAmazonS3 _s3client;
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUWest1;
        

        public S3Service(IAmazonS3 s3client)
        {
            _s3client  = new AmazonS3Client(bucketRegion); ;
        }

        public async Task<S3Response> CreateBucketAsync(string bucketName)
        {
            try
            {
                if(await AmazonS3Util.DoesS3BucketExistV2Async(_s3client, bucketName)==false)
                {
                    var putBucketRequest = new PutBucketRequest
                    {
                        BucketName = bucketName,
                        UseClientRegion = true

                    };

                    var response = await _s3client.PutBucketAsync(putBucketRequest);

                    return new S3Response
                    {
                        Message = response.ResponseMetadata.RequestId,
                        Status = response.HttpStatusCode
                    };
                }
            }
            catch (AmazonS3Exception e)
            {
                return new S3Response
                {
                    Message = e.Message,
                    Status = e.StatusCode
                };
            }
            catch (Exception e)
            {
                return new S3Response
                {
  
                    Status = HttpStatusCode.InternalServerError,
                    Message = e.Message
                };
            }

            return new S3Response
            {

                Status = HttpStatusCode.InternalServerError,
                Message = "Something went wrong"
            };
        }

        public async Task UploadfileAsync(IFormFile file, string bucketName)
        {
            try
            {
                using (var newMemoryStream = new MemoryStream())
                {
                    file.CopyTo(newMemoryStream);

                    var fileTransferUtility = new TransferUtility(_s3client);
                    TransferUtilityUploadRequest request = new TransferUtilityUploadRequest();
                    request.BucketName = bucketName;
                    request.Key = file.FileName;
                    request.InputStream = newMemoryStream;
                    request.CannedACL = S3CannedACL.PublicRead;

                    await fileTransferUtility.UploadAsync(request);
                    Console.WriteLine("Uploading file:" + file.FileName + " to bucket:" + bucketName);
                }
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountred on server.Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown Error encountred on server.Message:'{0}' when writing an object", e.Message);
            }
        }
    }
}
