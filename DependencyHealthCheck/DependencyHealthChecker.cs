using System;
using System.Configuration;
using System.Net;
using System.Xml;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;


namespace DependencyHealthCheck
{
    static class DependencyHealthChecker
    {
        //
        private static readonly string _loginId = ConfigurationManager.AppSettings["loginId"];
        private static readonly string _password = ConfigurationManager.AppSettings["password"];
        private static readonly string _tfsUri = ConfigurationManager.AppSettings["tfsUri"];
        private static readonly string _configPath = ConfigurationManager.AppSettings["configPath"];

        public static XmlDocument GetConfigXmlDocument()
        {
            var netCred = new NetworkCredential(_loginId, _password);
            var basicCred = new BasicAuthCredential(netCred);
            var tfsCred = new TfsClientCredentials(basicCred) { AllowInteractive = false };
            var document = new XmlDocument();
            var tpc = new TfsTeamProjectCollection(new Uri(_tfsUri), tfsCred);

            tpc.Authenticate();

            var version = tpc.GetService(typeof(VersionControlServer)) as VersionControlServer;
            if (version != null)
            {
                var config = version.GetItem(_configPath);
                
                using (var stm = config.DownloadFile())
                {
                    using (var reader = XmlReader.Create(stm))
                    {
                        document.Load(reader);
                    }
                }
            }
            return document;
        }
    }
}
