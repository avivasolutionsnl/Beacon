using System.Collections;
using System.Net;

using Beacon.Core;

namespace Beacon.Lights.WebServer
{
    public class WebServerLight : IBuildLight
    {
        private enum Color
        {
            Red,
            Green,
            Yellow,
            Grey
        }

        private readonly IDictionary colorMap = new Hashtable
        {
            { Color.Red, "#ff0000"},
            { Color.Green, "#00ff00"},
            { Color.Grey, "#aaaaaa"},
            { Color.Yellow, "#aaaaaa"},
        };
        
        private WebServer webServer = null;
        private Color color = Color.Green;
        
        public void Initialize()
        {
            if (webServer == null)
            {
                webServer = new WebServer(SendPage, "http://localhost:9999/status/");
                webServer.Run();    
            }
        }

        public void Dispose()
        {
            webServer.Stop();
        }

        private string SendPage(HttpListenerRequest request)
        {
            return $@"<html>
             <head>
                <style>
                    body {{
                        width: 100%;
                        height: 100%;
                        background-color: {colorMap[color]};
                        overflow: hidden;           
                    }}
                </style>
                <script>
                    setInterval(function() {{
                        window.location.reload();  
                    }}, 30000);
                </script>
            </head>
            <body></body>
            </html>";
        }

        public void Success()
        {
            color = Color.Green;
        }

        public void Investigate()
        {
            color = Color.Yellow;
        }

        public void Fail()
        {
            color = Color.Red;
        }

        public void Fixed()
        {
            color = Color.Green;
        }

        public void NoStatus()
        {
            color = Color.Grey;
        }
    }
}