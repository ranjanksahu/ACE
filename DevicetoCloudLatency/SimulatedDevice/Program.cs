using Dataobjects;
using DBOperations;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Text;

namespace SimulatedDevice
{
    class Program
    {
        private static DeviceClient s_deviceClient;
        private readonly static string s_connectionString = "HostName=RTN-SI.azure-devices.net;DeviceId=Device01;SharedAccessKey=0BzJ+B6mpWZi1UDrOyGhzQ+B3Tx52JI/DXaQvW5L9GE=";
        private static int id = 0;
        private static MongoOperations database = new MongoOperations("South India");
        static void Main(string[] args)
        {
            s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, TransportType.Http1);

            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(.5);
            SetMaxId();
            var timer = new System.Threading.Timer((e) =>
            {
                SendDeviceToCloudMessagesAsync();
            }, null, startTimeSpan, periodTimeSpan);

            //SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }

        private static void SetMaxId()
        {
            id = database.GetMaxRowId();
            database.GeturlList();
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {
            try
            {
                id = id + 1;
                var uniqueId = id;
                // Create JSON message
                var telemetry = new TelemetryData
                {
                    UniqueId = uniqueId,
                    Data = "Test" + uniqueId,
                    SentDTTM = DateTime.Now
                };
                var messageString = JsonConvert.SerializeObject(telemetry);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("level", "storage");
                // Send the telemetry message
                database.InsertTelemetryData(telemetry);
                await s_deviceClient.SendEventAsync(message);
                database.UpdateAckDTTM(telemetry.UniqueId, DateTime.Now);
            }
            catch (Exception exc)
            {
                string exception = exc.Message;
            }
        }
    }
}
