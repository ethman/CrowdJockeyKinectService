using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Diagnostics;

namespace CrowdJockeyKinectService
{
    public struct MovementValue
    {
        public int CurVal, PrevVal, Diff;

        DateTime timeStamp;

        public const string CurValName = "CurrentMovementValue", 
            PrevValName = "PreviousMovementValue", DiffName = "Diff", TimestampName = "Timestamp";

        public void AddVal(int val) {
            if (val < 50)
            {
                val = 0;
            }
            else if (val < 100)
            {
                val = 1;
            }
            else 
            {
                val = 2;
            } 


            PrevVal = CurVal;
            CurVal = val;
            Diff = CurVal - PrevVal;
            timeStamp = DateTime.Now;
        }
    }

    public class DataInterpreter
    {
        private struct MinimizedBody
        {
            public CameraSpacePoint Head, ShoulderTop, ShoulderBottom;

            public MinimizedBody(CameraSpacePoint h, CameraSpacePoint t, CameraSpacePoint b)
            {
                Head = h;
                ShoulderTop = t;
                ShoulderBottom = b;
            }
        }

        private Dictionary<UInt64, List<MinimizedBody>> _prevBodies = new Dictionary<UInt64, List<MinimizedBody>>();
        public Object Lock = new Object();

        private JointType[] LowerBody = {JointType.AnkleLeft, JointType.AnkleRight, JointType.FootLeft, JointType.FootRight,
                                         JointType.KneeLeft, JointType.KneeRight, JointType.HipLeft, JointType.HipRight, JointType.SpineBase};

        private JointType[] UpperBody = {JointType.SpineMid, JointType.SpineShoulder, JointType.ShoulderLeft, JointType.ShoulderRight,
                                              JointType.ElbowLeft, JointType.ElbowRight, JointType.WristLeft, JointType.WristRight, 
                                              JointType.HandLeft, JointType.HandRight, JointType.ThumbLeft, JointType.ThumbRight,
                                              JointType.HandTipLeft, JointType.HandTipRight};

        private MovementValue _movementVal;

        private int _prevMoveNum;

        public MovementValue MovementNum
        {
            get
            {
                lock (Lock)
                {
                    return _movementVal;
                }
            }
        }

        public void GatherData(List<Body> bodies)
        {
            lock (Lock)
            {
                foreach (var body in bodies)
                {
                    if (body.IsTracked)
                    {
                        if (!_prevBodies.ContainsKey(body.TrackingId))
                        {
                            _prevBodies[body.TrackingId] = new List<MinimizedBody>();
                        }
                        try
                        {
                            var pos = new MinimizedBody(body.Joints[JointType.Head].Position,
                                body.Joints[JointType.SpineShoulder].Position,
                                body.Joints[JointType.SpineBase].Position);

                            _prevBodies[body.TrackingId].Add(pos);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }

        }

        async Task CompressDataAsync()
        {
            lock (Lock)
            {
                int perPersonAvg = 0;
                string output;

                // All different bodies
                foreach (var bodyList in _prevBodies.Values)
                {
                    double moveAvg;
                    // Same bodies but different times
                    for (int i = 1; i<bodyList.Count; i++)
                    {
                        if (bodyList.Count < 2)
                        {
                            break;
                        }

                        var curBody = bodyList.ElementAt<MinimizedBody>(i);
                        var prevBody = bodyList.ElementAt<MinimizedBody>(i - 1);

                        moveAvg = 0;

                        moveAvg += GetDelta(curBody.Head, prevBody.Head);
                        moveAvg += GetDelta(curBody.ShoulderTop, prevBody.ShoulderTop);
                        moveAvg += GetDelta(curBody.ShoulderBottom, prevBody.ShoulderBottom);

                        moveAvg *= 1000;
                        perPersonAvg = (int)moveAvg;
                    }

                   
                }

                if (_prevBodies.Count > 0)
                {
                    perPersonAvg /= _prevBodies.Count;
                    output = perPersonAvg.ToString() + " " + _prevBodies.Count.ToString();
                    Debug.WriteLine(output);

                    _movementVal.AddVal(perPersonAvg);
                    
                }
                _prevBodies.Clear();
            }

            await Task.Yield();
        }

        public async Task StartDataCompression()
        {
            await CompressDataAsync();
        }

        private double GetDelta(CameraSpacePoint a, CameraSpacePoint b)
        {
            var delX = a.X - b.X;
            var delY = a.Y - b.Y;
            var delZ = a.Z - b.Z;

            return Math.Sqrt(delX * delX + delY * delY + delZ * delZ);
        }
    }
}
