using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Net.Http;
using System.Web;
using System.Web.Script.Serialization;
using System.Diagnostics;

namespace CrowdJockeyKinectService
{
    public class WebBackendInterface
    {
        public DataInterpreter _interpreter;

        public WebBackendInterface(DataInterpreter interpreter)
        {
            _interpreter = interpreter;
        }

        async Task PostData()
        {
            MovementValue movementVal = _interpreter.MovementNum;
            try
            {
                string urlBase = "http://1-dot-crowd-jockey.appspot.com/crowdjockey";
                string rest = "?action=level&level=" + movementVal.CurVal.ToString();

                string fullUrl = urlBase + rest;
                /*var request = (HttpWebRequest)WebRequest.Create(fullUrl);

                var json = new JavaScriptSerializer().Serialize(movementVal);

                var response = (HttpWebResponse)request.GetResponse();*/

                //string response;
                using (var client = new HttpClient())
                {
                    var responseString = client.GetStringAsync(fullUrl);
                    Debug.WriteLine(responseString.Result);
                }

                //Debug.WriteLine(response);
                Debug.WriteLine(rest);

            }
            catch (Exception)
            {
                Debug.WriteLine("Could not post");
            }


            await Task.Yield();
        }


        public async Task DoPostData()
        {
            await PostData();
        }
    }
}
