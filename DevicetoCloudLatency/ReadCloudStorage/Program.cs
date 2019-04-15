using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using DBOperations;
using Dataobjects;

namespace ReadCloudStorage
{
    class Program
    {
        private static MongoOperations database = new MongoOperations("South India");
        static void Main(string[] args)
        {
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(.5);
            var timer = new System.Threading.Timer((e) =>
            {
                ReadCloudDataAndUpdate();
            }, null, startTimeSpan, periodTimeSpan);
            Console.ReadLine();
        }

        private static void ReadCloudDataAndUpdate()
        {
            List<string> urllist = GetBlobUrls();
            List<string> savedUrlList = database.GeturlList();
            List<string> finallist = urllist.Except(savedUrlList).ToList();
            foreach (var url in finallist)
            {
                try
                {
                    ReadBlobData(url);
                }
                catch (Exception ex)
                {
                    if (ex.Message == "One or more errors occurred. (No such host is known)")
                    {
                        continue;
                    }
                }
            }
        }



        private static void ReadBlobData(string url)
        {
            int enqueDateStartIndex = 0;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body.
                var data = response.Content.ReadAsStringAsync().Result;
                enqueDateStartIndex = data.IndexOf("enqueuedTime");
                while (enqueDateStartIndex > 0)
                {
                    var enqueDateEndIndex = enqueDateStartIndex + 13; // length of enqueuedTime8 is 13
                    var enqueDateTime = data.Substring(enqueDateEndIndex, 26); // 2019-03-10T10:58:48.8850000Z - read 19 characters from this value
                    var receivedUTCDTTM = DateTime.Parse(enqueDateTime);
                    var receivedISTDTTM = TimeZoneInfo.ConvertTimeFromUtc(receivedUTCDTTM, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    data = data.Substring(enqueDateEndIndex);
                    var uniqueidIndex = data.IndexOf("UniqueId");
                    var dataIndex = data.IndexOf("Data");
                    var uniqueId = data.Substring(uniqueidIndex + 10, dataIndex - (uniqueidIndex + 10) - 2); // {\"UniqueId\":4,\"Data\":\"Test\"
                    data = data.Substring(dataIndex);
                    var sentDTTMIndex = data.IndexOf("SentDTTM");
                    var strsentDTTM = data.Substring(sentDTTMIndex + 11, 26); //SentDTTM\":\"2019-03-10T16:28:31.6342778+05:30\
                    var sentDTTM = DateTime.Parse(strsentDTTM);
                    database.UpdateReceivedDTTM(int.Parse(uniqueId), receivedISTDTTM, url);
                    data = data.Substring(sentDTTMIndex);
                    enqueDateStartIndex = data.IndexOf("enqueuedTime");
                }
            }
        }

        private async static void GetBlobUrls5()
        {
            string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=sapayloadsi;AccountKey=J/Rdqboq72LlHvmJl/1W8dAYFXOU/SC5dtGf+8QaR/ci/vlaj2RKQtmK//fIztLjpauW0SNGNQzFLNlJ+P3NpQ==;EndpointSuffix=core.windows.net";
            try
            {
                // Check whether the connection string can be parsed.
                CloudStorageAccount storageAccount;
                if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
                {
                    CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                    // Create a container called 'quickstartblobs' and append a GUID value to it to make the name unique. 
                    CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("containerpayloadsi");

                    BlobContinuationToken blobContinuationToken = null;
                    do
                    {
                        var results = await cloudBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                        // Get the value of the continuation token returned by the listing call.
                        blobContinuationToken = results.ContinuationToken;
                        foreach (IListBlobItem item in results.Results)
                        {
                            Console.WriteLine(item.Uri);
                        }
                    } while (blobContinuationToken != null); // Loo

                }
            }
            catch (Exception exc)
            {
                var message = exc.Message;
            }
        }
        
        

        private static List<string> GetBlobUrls()
        {
            // Get a reference to a container that's available for anonymous access.
            CloudBlobContainer container = new CloudBlobContainer(new Uri(@"https://sapayloadsi.blob.core.windows.net/containerpayloadsi?restype=container&comp=list&include=snapshot"));
            // List blobs in the container.
            List<string> urllist = new List<string>();
            foreach (IListBlobItem blobItem in container.ListBlobs(null, true, BlobListingDetails.All))
            {
                urllist.Add(blobItem.Uri.ToString());               
            }
            return urllist;
        }

        private static void GetBlobURLs0()
        {
            //var account = new CloudStorageAccount(new StorageCredentials("sapayloadsi", "J/Rdqboq72LlHvmJl/1W8dAYFXOU/SC5dtGf+8QaR/ci/vlaj2RKQtmK//fIztLjpauW0SNGNQzFLNlJ+P3NpQ=="),true);
            var account = new CloudStorageAccount(new StorageCredentials("sapayloadsi", "J/Rdqboq72LlHvmJl/1W8dAYFXOU/SC5dtGf+8QaR/ci/vlaj2RKQtmK//fIztLjpauW0SNGNQzFLNlJ+P3NpQ=="), true);
            var container = account.CreateCloudBlobClient().GetContainerReference("containerpayloadsi");
            var blobs = container.ListBlobs(prefix: "Folder/Folder", useFlatBlobListing: true);
            foreach (IListBlobItem item in blobs)
            {

            }
        }


        private static void GetBlobURLs1()
        {
            string containerURL = "https://sapayloadsi.blob.core.windows.net/containerpayloadsi?retype=container&comp=list";
            CloudBlobContainer cloudBlobContainer = new CloudBlobContainer(new Uri(containerURL));
            //var account = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials())
            var results = cloudBlobContainer.ListBlobs();
            var urllist = results.OfType<CloudBlockBlob>().Select(b => b.Uri).ToList();
            // Get the value of the continuation token returned by the listing call.
            foreach (var item in urllist)
            {

            }
        }

        private static void GetBlobUrls2()
        {
            CloudStorageAccount backupStorageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=sapayloadsi;AccountKey=J/Rdqboq72LlHvmJl/1W8dAYFXOU/SC5dtGf+8QaR/ci/vlaj2RKQtmK//fIztLjpauW0SNGNQzFLNlJ+P3NpQ==;EndpointSuffix=core.windows.net");
            var backupBlobClient = backupStorageAccount.CreateCloudBlobClient();
            var backupContainer = backupBlobClient.GetContainerReference("containerpayloadsi");
            var blobs = backupContainer.ListBlobs().OfType<CloudBlockBlob>().ToList();
            foreach (var blob in blobs)
            {
                string bName = blob.Name;
                long bSize = blob.Properties.Length;
                string bModifiedOn = blob.Properties.LastModified.ToString();
            }
        }

        private static void GetBlobUrls3()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=sapayloadsi;AccountKey=J/Rdqboq72LlHvmJl/1W8dAYFXOU/SC5dtGf+8QaR/ci/vlaj2RKQtmK//fIztLjpauW0SNGNQzFLNlJ+P3NpQ==;EndpointSuffix=core.windows.net");

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("containerpayloadsi");

            // Loop over items within the container and output the length and URI.
            foreach (IListBlobItem item in container.ListBlobs(null, false))
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    Console.WriteLine("Block blob of length {0}: {1}", blob.Properties.Length, blob.Uri);
                }
                else if (item.GetType() == typeof(CloudPageBlob))
                {
                    CloudPageBlob pageBlob = (CloudPageBlob)item;
                    Console.WriteLine("Page blob of length {0}: {1}", pageBlob.Properties.Length, pageBlob.Uri);
                }
                else if (item.GetType() == typeof(CloudBlobDirectory))
                {
                    CloudBlobDirectory directory = (CloudBlobDirectory)item;
                    Console.WriteLine("Directory: {0}", directory.Uri);
                }
            }
        }
    }
}
