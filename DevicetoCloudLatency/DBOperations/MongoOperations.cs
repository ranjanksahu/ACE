using Dataobjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBOperations
{
    public class MongoOperations : IOperations
    {
        MongoClient client;
        IMongoDatabase database;
        const string databaseName = "ACE";
        const string collectionSouthIndia = "TelemetryMetadataSouthIndia";
        const string collectionWestUS = "TelemetryMetadataWestUS";
        IMongoCollection<BsonDocument> collection;

        public MongoOperations(string region)
        {
            client = new MongoClient();
            database = client.GetDatabase(databaseName);
            if (region == "South India")
            {
                collection = database.GetCollection<BsonDocument>(collectionSouthIndia);
            }
            else if (region == "West US")
            {
                collection = database.GetCollection<BsonDocument>(collectionWestUS);
            }
        }

        public void InsertTelemetryData(TelemetryData data)
        {
            var document = new BsonDocument
            {
                { "UniqueId", data.UniqueId},
                {"Data", data.Data },
                { "SentDTTM", data.SentDTTM.ToString()}
            };
            collection.InsertOne(document);
        }

        public void UpdateAckDTTM(int id, DateTime ackDateTime)
        {
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("UniqueId", id);
            var record = collection.Find(filter).FirstOrDefault();
            var currentdate = ackDateTime;
            var sentdate = record["SentDTTM"].AsString;
            DateTime sentDateTime = DateTime.Parse(sentdate);
            var acktimedifference = (currentdate - sentDateTime).TotalSeconds;
            var updoneresult = collection.UpdateOne(
                                Builders<BsonDocument>.Filter.Eq("UniqueId", id),
                                Builders<BsonDocument>.Update.Set("AcknowledgedDTTM", currentdate.ToString()).
                                Set("AckTimeDifference", acktimedifference));
        }

        public void UpdateReceivedDTTM(int id, DateTime receivedDTTM, string url)
        {
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("UniqueId", id);
            var record = collection.Find(filter).FirstOrDefault();
            DateTime sentDateTime = DateTime.Parse(record["SentDTTM"].AsString);
            double sentTimeDifference = (receivedDTTM - sentDateTime).TotalSeconds;
            var updoneresult = collection.UpdateOne(
                                Builders<BsonDocument>.Filter.Eq("UniqueId", id),
                                Builders<BsonDocument>.Update.Set("ReceivedDTTM", receivedDTTM.ToString())
                                .Set("URL", url)
                                .Set("SentTimeDifference", sentTimeDifference));
        }

        public int GetMaxRowId()
        {
            var sort = Builders<BsonDocument>.Sort.Descending("UniqueId"); //build sort object   
            var result = collection.Find(new BsonDocument()).Sort(sort).FirstOrDefault(); //apply it to collection
            if (result == null)
            {
                return 0;
            }
            return result["UniqueId"].AsInt32;
        }

        public List<string> GeturlList()
        {
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Ne("URL", BsonNull.Value);
            var item = Builders<BsonDocument>.Projection.Include(x => x["URL"]).Exclude(y => y["_id"]);
            var list = collection.Find(filter).Project<BsonDocument>(item).ToList();
            return list.Select(x => x["URL"].AsString).ToList();
        }

        public ChartData GetChartData()
        {
            ChartData chartdata = new ChartData();
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Ne("AckTimeDifference", BsonNull.Value) & builder.Ne("SentTimeDifference", BsonNull.Value);
            var item = Builders<BsonDocument>.Projection.Include(x => x["AckTimeDifference"]).Include(x => x["SentTimeDifference"]).Include(x => x["SentDTTM"]).Exclude(y => y["_id"]);
            var list = collection.Find(filter).Project<BsonDocument>(item).ToList();
            chartdata.Latency = list.Select(x => x["SentTimeDifference"].AsDouble).ToList();
            chartdata.AckTimeDifference = list.Select(x => x["AckTimeDifference"].AsDouble).ToList();
            chartdata.SentDTTM = list.Select(x => x["SentDTTM"].AsString).ToList();
            return chartdata;
        }

        public double GetAverageSentReceivedTimedifference()
        {
            var aggregation = collection.Aggregate<BsonDocument>().Group(new BsonDocument
                        {
                            { "_id", BsonNull.Value
                            },
                            {
                                "avg_value", new BsonDocument
                                {
                                    {
                                        "$avg", "$SentTimeDifference"
                                    }   
                            }
                }
            });
            var doc = aggregation.SingleOrDefault();
            BsonDocument result = doc.AsBsonDocument;
            var avg = result["avg_value"].AsDouble;
            return avg;
        }

        public double GetMaxSentReceivedTimedifference()
        {
            var aggregation = collection.Aggregate<BsonDocument>().Group(new BsonDocument
                        {
                            { "_id", BsonNull.Value
                            },
                            {
                                "max_value", new BsonDocument
                                {
                                    {
                                        "$max", "$SentTimeDifference"
                                    }
                            }
                }
            });
            var doc = aggregation.SingleOrDefault();
            BsonDocument result = doc.AsBsonDocument;
            var max = result["max_value"].AsDouble;
            return max;
        }

        public double GetMinSentReceivedTimedifference()
        {
            //var min = collection.Aggregate().SortBy(a => a["SentTimeDifference"]).First();
            //return min["SentTimeDifference"].AsDouble;
            var aggregation = collection.Aggregate<BsonDocument>().Group(new BsonDocument
                        {
                            { "_id", BsonNull.Value
                            },
                            {
                                "min_value", new BsonDocument
                                {
                                    {
                                        "$min", "$SentTimeDifference"
                                    }
                            }
                }
            });
            var doc = aggregation.SingleOrDefault();
            BsonDocument result = doc.AsBsonDocument;
            var min = result["min_value"].AsDouble;
            return min;
        }

        public Dictionary<string, int> GetTimeRangewithCount()
        {
            Dictionary<string, int> range = new Dictionary<string, int>();

            var builder = Builders<BsonDocument>.Filter;
            var filter0 = builder.Lt("SentTimeDifference", 1);
            var record = collection.Find(filter0).ToList().Count;
            range.Add("Less than 1 Second", record);

            var filter1 = builder.Gt("SentTimeDifference", 1) & builder.Lt("SentTimeDifference", 2);
            record = collection.Find(filter1).ToList().Count;
            range.Add("1 - 2 Seconds", record);

            var filter2 = builder.Gt("SentTimeDifference", 2) & builder.Lt("SentTimeDifference", 3);
            record = collection.Find(filter2).ToList().Count;
            range.Add("2 - 3 Seconds", record);

            var filter3 = builder.Gt("SentTimeDifference", 3) & builder.Lt("SentTimeDifference", 4);
            record = collection.Find(filter3).ToList().Count;
            range.Add("3 - 4 Seconds", record);

            var filter4 = builder.Gt("SentTimeDifference", 4) & builder.Lt("SentTimeDifference", 5);
            record = collection.Find(filter4).ToList().Count;
            range.Add("4 - 5 Seconds", record);

            var filter5 = builder.Gt("SentTimeDifference", 5) & builder.Lt("SentTimeDifference", 6);
            record = collection.Find(filter5).ToList().Count;
            range.Add("5 - 6 Seconds", record);

            var filter6 = builder.Gt("SentTimeDifference", 6) & builder.Lt("SentTimeDifference", 7);
            record = collection.Find(filter6).ToList().Count;
            range.Add("6 - 7 Seconds", record);

            var filter7 = builder.Gt("SentTimeDifference", 7) & builder.Lt("SentTimeDifference", 8);
            record = collection.Find(filter7).ToList().Count;
            range.Add("7 - 8 Seconds", record);

            var filter8 = builder.Gt("SentTimeDifference", 10);
            record = collection.Find(filter8).ToList().Count;
            range.Add("More than 10 Seconds", record);

            return range;
        }

    }
}
