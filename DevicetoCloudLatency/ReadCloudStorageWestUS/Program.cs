using DBOperations;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace ReadCloudStorageWestUS
{
    class Program
    {
        private static MongoOperations database = new MongoOperations("West US");
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

        private static List<string> GetBlobUrls()
        {
            // Get a reference to a container that's available for anonymous access.
            CloudBlobContainer container = new CloudBlobContainer(new Uri(@"https://sapayloadwus.blob.core.windows.net/containerpayloadwus?restype=container&comp=list&include=snapshot"));
            // List blobs in the container.
            List<string> urllist = new List<string>();
            foreach (IListBlobItem blobItem in container.ListBlobs(null, true, BlobListingDetails.All))
            {
                try
                {
                    urllist.Add(blobItem.Uri.ToString());
                }
                catch (Exception ex)
                {
                    if (ex.Message == "No such host is known")
                    {
                        continue;
                    }
                }
            }
            return urllist;
        }

    }
}
