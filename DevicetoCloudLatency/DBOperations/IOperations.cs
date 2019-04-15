using Dataobjects;
using System;
using System.Collections.Generic;

namespace DBOperations
{
    public interface IOperations
    {
        void InsertTelemetryData(TelemetryData data);

        void UpdateAckDTTM(int id, DateTime ackDateTime);

        void UpdateReceivedDTTM(int id, DateTime receivedDTTM, string url);

        int GetMaxRowId();

        List<string> GeturlList();

        ChartData GetChartData();

        double GetAverageSentReceivedTimedifference();

        double GetMaxSentReceivedTimedifference();

        double GetMinSentReceivedTimedifference();

        Dictionary<string, int> GetTimeRangewithCount();

    }
}
