using System;
using System.IO;
using System.Text;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KidsReminderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaskController : ControllerBase
    {
        private const string connectionSTring = "";
        private const string containerName = "tasks";
        private const string blobName = "taskBlob";
        private readonly ILogger<TaskController> _logger;

        public TaskController(ILogger<TaskController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("/api/tasks")]
        public Task Get()
        {
            try
            {
                var client = new BlobContainerClient(connectionSTring, containerName);
                var blob = client.GetBlobClient(blobName);
                if (!blob.Exists())
                {
                    return null;
                }

                using (var ms = new MemoryStream())
                {
                    blob.DownloadTo(ms);
                    long length = ms.Length;
                    ms.Position = 0;
                    var bytes = new byte[length];
                    ms.Read(bytes, 0, (int)length);
                    string content = Encoding.UTF8.GetString(bytes);
                    return JsonConvert.DeserializeObject<Task>(content);
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e, "Get");
                throw;
            }
        }

        [HttpPut]
        [Route("/api/tasks")]
        public string Add([FromBody]Task task)
        {
            try
            {
                string content = JsonConvert.SerializeObject(task);
                var client = new BlobContainerClient(connectionSTring, containerName);
                var blob = client.GetBlobClient(blobName);
                if (blob.Exists())
                {
                    return string.Empty;
                }

                using (var ms = new MemoryStream())
                {
                    var bytes = Encoding.UTF8.GetBytes(content);
                    ms.Write(bytes, 0, bytes.Length);
                    ms.Position = 0;
                    blob.Upload(ms);
                }

                return string.Empty;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e, "Add");
                return e.ToString();
            }
        }
    }
}
