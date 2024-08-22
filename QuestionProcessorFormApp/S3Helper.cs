using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace QuestionProcessorFormApp
{
    public class S3Helper
    {
        public class S3Config
        {
            public string S3BucketName = "";
            public string S3AccessKeyAWS = "";
            public string S3SecretKeyAWS = "";
            public S3Config()
            {
                S3BucketName = ConfigHelper.GetConfig("S3BucketNameAWS");
                S3AccessKeyAWS = ConfigHelper.GetConfig("S3AccessKeyAWS");
                S3SecretKeyAWS = ConfigHelper.GetConfig("S3SecretKeyAWS");
            }
        }
        public static string GetFile3S(S3Config s3Config, string subDirectoryInBucket, string fileId, string fileExt, string folder, ref string log)
        {
            string rs = "";
            log += "GetFile3S";
            string localFile = folder.TrimEnd('\\') + Path.DirectorySeparatorChar + fileId;
            if (localFile.IndexOf(".doc") < 0)
            {
                localFile += fileExt;
            }
            try
            {
                log = CLogger.Append(log, "AmazonS3Client");
                IAmazonS3 client = new AmazonS3Client(s3Config.S3AccessKeyAWS, s3Config.S3SecretKeyAWS, RegionEndpoint.APSoutheast1);
                GetObjectRequest requestDL = new GetObjectRequest();
                //requestDL.BucketName = s3Config.S3BucketName;
                if (string.IsNullOrEmpty(subDirectoryInBucket))
                {
                    requestDL.BucketName = s3Config.S3BucketName; //no subdirectory just bucket name  
                }
                else
                {   // subdirectory and bucket name  
                    requestDL.BucketName = s3Config.S3BucketName + @"/" + subDirectoryInBucket;
                }
                requestDL.Key = fileId; // tên file trên aws
                log = CLogger.Append(log, "GetObject");
                GetObjectResponse response = client.GetObject(requestDL);
                if (response.HttpStatusCode.ToString() == "OK")
                {
                    log = CLogger.Append(log, "response.HttpStatusCode.ToString()", response.HttpStatusCode.ToString(), "WriteResponseStreamToFile");
                    response.WriteResponseStreamToFile(localFile);
                    rs = localFile;
                    log = CLogger.Append(log, "localFile", localFile);
                }
                else
                {
                    log = CLogger.Append(log, "response.HttpStatusCode.ToString()", response.HttpStatusCode.ToString());
                }
                CLogger.WriteLogSuccess(log);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                log = CLogger.Append(log, "AmazonS3Exception:", amazonS3Exception.ToString());
                CLogger.WriteLogException(log);
            }
            catch (Exception ex)
            {
                log = CLogger.Append(log, "Exception", ex.Message);
                CLogger.WriteLogException(log);
            }
            return rs;
        }
        public static bool SaveFile3S(S3Config s3Config, string subDirectoryInBucket, string s3FileName, string localFilePath, ref string log)
        {
            bool rs = false;
            log += "SaveFile3S";
            try
            {
                log = CLogger.Append(log, "AmazonS3Client");
                IAmazonS3 client = new AmazonS3Client(s3Config.S3AccessKeyAWS, s3Config.S3SecretKeyAWS, RegionEndpoint.APSoutheast1);
                TransferUtility utility = new TransferUtility(client);
                TransferUtilityUploadRequest request = new TransferUtilityUploadRequest();
                if (string.IsNullOrEmpty(subDirectoryInBucket))
                {
                    request.BucketName = s3Config.S3BucketName; //no subdirectory just bucket name  
                }
                else
                {   // subdirectory and bucket name  
                    request.BucketName = s3Config.S3BucketName + @"/" + subDirectoryInBucket;
                }

                FileStream fStream = new FileStream(localFilePath, FileMode.Open);
                request.Key = s3FileName; //file name up in S3  
                request.InputStream = fStream;
                log = CLogger.Append(log, "Upload");
                utility.Upload(request); //commensing the transfer 
                rs = true;
                CLogger.WriteLogSuccess(log);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                log = CLogger.Append(log, "AmazonS3Exception:", amazonS3Exception.ToString());
                CLogger.WriteLogException(log);
            }
            catch (Exception ex)
            {
                log = CLogger.Append(log, "Exception:", ex.Message);
                CLogger.WriteLogException(log);
            }
            return rs;
        }
        public static bool SaveFolder3S(S3Config s3Config, string subDirectoryInBucket, string localDirPath, ref string log)
        {
            bool rs = false;
            log += "SaveFile3S";
            try
            {
                log = CLogger.Append(log, "AmazonS3Client");
                IAmazonS3 client = new AmazonS3Client(s3Config.S3AccessKeyAWS, s3Config.S3SecretKeyAWS, RegionEndpoint.APSoutheast1);
                var directoryTransferUtility = new TransferUtility(client);
                TransferUtilityUploadDirectoryRequest request = new TransferUtilityUploadDirectoryRequest();
                if (string.IsNullOrEmpty(subDirectoryInBucket))
                {
                    request.BucketName = s3Config.S3BucketName; //no subdirectory just bucket name  
                }
                else
                {   // subdirectory and bucket name  
                    request.BucketName = s3Config.S3BucketName + @"/" + subDirectoryInBucket;
                }

                //create folder
                PutObjectRequest requestFolder = new PutObjectRequest()
                {
                    BucketName = request.BucketName,
                    Key = subDirectoryInBucket // <-- in S3 key represents a path  
                };

                PutObjectResponse response = client.PutObject(requestFolder);

                request.SearchOption = SearchOption.AllDirectories;
                request.Directory = localDirPath;
                directoryTransferUtility.UploadDirectory(request);
                directoryTransferUtility.Dispose();
                rs = true;
                CLogger.WriteLogSuccess(log);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                log = CLogger.Append(log, "AmazonS3Exception:", amazonS3Exception.ToString());
                CLogger.WriteLogException(log);
            }
            catch (Exception ex)
            {
                log = CLogger.Append(log, "Exception:", ex.Message);
                CLogger.WriteLogException(log);
            }
            return rs;
        }

    }
}
