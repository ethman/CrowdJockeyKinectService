using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Diagnostics;

namespace CrowdJockeyKinectService
{
    public class KinectInterface
    {
        #region privateVariables

        private bool _isOkay = true;
        private KinectSensor _sensor = null;
        private Body[] _bodies = null;
        private BodyFrameReader _bodyReader = null;
        private DataInterpreter _interpreter = null;
        private uint _frameCount;

        private const uint FRAMETHROWOUT = 4;

        #endregion

        #region Fields
        public bool IsKinectOkay 
        { 
            get { return _isOkay; } 
            private set { _isOkay = value; }
        }
        #endregion

        #region privateMethods
        private void KinectReady()
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor == null)
            {
                _isOkay = false;
                return;
            }

            _sensor.Open();

            if (!_sensor.IsOpen)
            {
                _isOkay = false;
                return;
            }

            _bodyReader = _sensor.BodyFrameSource.OpenReader();

            if (_bodyReader == null)
            {
                _isOkay = false;
            }
        }

        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (_bodies == null)
                    {
                        _bodies = new Body[bodyFrame.BodyCount];
                    }

                    bodyFrame.GetAndRefreshBodyData(_bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived && _frameCount == 0)
            {
                _interpreter.GatherData(_bodies.ToList());
            }
            _frameCount = _frameCount < FRAMETHROWOUT ? _frameCount++ : 0;
            //_frameCount = 0;
        }

        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            _isOkay = _sensor.IsAvailable;
        }

        public void CollectData()
        {
            if (_isOkay && _bodyReader != null)
            {
                _bodyReader.FrameArrived += this.Reader_FrameArrived;
            }
        }

        #endregion

        #region constructors
        public KinectInterface(DataInterpreter interpreter)
        {
            _interpreter = interpreter;
            _frameCount = 0;
            KinectReady();
        }

        ~KinectInterface()
        {
            if (_bodyReader != null)
            {
                _bodyReader.Dispose();
                _bodyReader = null;
            }

            if (_sensor != null)
            {
                _sensor.Close();
                _sensor = null;
            }
        }
        #endregion
    }
}
