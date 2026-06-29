using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTool.Utility
{
    public static class UTrack
    {
        private static Dictionary<string, TrackData> trackingData = new Dictionary<string, TrackData>();

        public static void Start(string trackingName, bool showStartMessage = true)
        {
            if (UpdateTrack(trackingName))
                return;

            TrackData newTrackData = new TrackData();
            newTrackData.trackingName = trackingName;
            newTrackData.SetTimeStamp(isMainTimeStamp: true);

            trackingData.Add(trackingName, newTrackData);

            if (showStartMessage)
            {
                string message = "// UTrack ||";
                Debug.Log($"{message} Tracking : '{trackingName}'");
            }
        }

        public static void Stop(string trackingName)
        {
            if (!UpdateTrack(trackingName, closeTrack: true))
                Debug.LogWarning($"Tracking Name '{trackingName}' Has Not Been Started.");
        }

        private static bool UpdateTrack(string trackingName, bool closeTrack = false)
        {
            if (!trackingData.ContainsKey(trackingName))
                return false;

            TrackData trackData = trackingData[trackingName];
            trackData.SetTimeStamp();

            TimeSpan totalDifference = trackData.GetDifferenceFromMain();
            TimeSpan difference = trackData.GetDifference();

            string message = closeTrack ? "|| UTrack ||" : "// UTrack //";

            message += $" {trackingName} : {difference.ToMinSecMill()}";
            if (trackData.timestampCount > 1)
                message += $" || Total : {totalDifference.ToMinSecMill()}";

            Debug.Log(message);

            if (closeTrack)
                trackingData.Remove(trackingName);

            return true;
        }

        private class TrackData
        {
            public string trackingName;

            public DateTime mainTimestamp;
            public DateTime timestamp;
            public DateTime previousTimeStamp;

            public int timestampCount = -1;

            public void SetTimeStamp(bool isMainTimeStamp = false)
            {
                if (timestamp != null)
                    previousTimeStamp = timestamp;
                timestamp = DateTime.Now;

                if (isMainTimeStamp)
                    mainTimestamp = timestamp;

                timestampCount++;
            }

            public TimeSpan GetDifferenceFromMain()
            {
                return timestamp - mainTimestamp;
            }

            public TimeSpan GetDifference()
            {
                return timestamp - previousTimeStamp;
            }
        }
    }
}