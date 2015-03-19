using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace CrowdJockeyKinectService
{
    public class MainProgram
    {

        public static void Main(string[] args)
        {
            var interpreter = new DataInterpreter();
            var kinect = new KinectInterface(interpreter);
            var web = new WebBackendInterface(interpreter);
            
            if (!kinect.IsKinectOkay)
            {
                Debug.Print("Could not start Kinect! Quitting...");
                return;
            }

            Debug.Print("Should be connected to Kinect =)");
            kinect.CollectData();

            while (kinect.IsKinectOkay)
            {
                System.Threading.Thread.Sleep(1000);
                var compressTask = interpreter.StartDataCompression();
                var webTask = web.DoPostData();
            }
        }
    }
}
