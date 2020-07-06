using System;
using System.Collections.Generic;
using System.Net;

using Beacon.Core;

using RestSharp;

namespace Beacon.Lights.Shelly
{
    public class ShellyBulb
    {
        private string url { get; set; }

        public void Configure(string url)
        {
            this.url = url;
        }
        
        public void Set(byte red, byte green, byte blue)
        {
            var request = new RestRequest("light/0", Method.POST);

            request.AddParameter("mode", "color", ParameterType.GetOrPost);
            request.AddParameter("gain", 100, ParameterType.GetOrPost);
            request.AddParameter("turn", "on", ParameterType.GetOrPost);
            
            request.AddParameter("red", red, ParameterType.GetOrPost);
            request.AddParameter("green", green, ParameterType.GetOrPost);
            request.AddParameter("blue", blue, ParameterType.GetOrPost);
            
            Send(request);
        }

        public void Off()
        {
            var request = new RestRequest("settings", Method.POST);
            request.AddParameter("turn", "off", ParameterType.GetOrPost);
            
            Send(request);
        }

        private void Send(RestRequest request)
        {
            try
            {
                var client = new RestClient(url);
                var response = client.Execute(request);
                
                if (response.ErrorException != null)
                {
                    throw response.ErrorException;
                }

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Logger.WriteErrorLine($"Failed to send command to Shelly bulb (@ {url}): {response.StatusDescription}");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
    }
}