using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Xml;
using DocumentDBServiceLibrary;

namespace DependencyHealthCheck
{
    class Program
    {
        static List<HealthCheckResult> results = new List<HealthCheckResult>();
        static XmlDocument configFileXml = new XmlDocument();
        static string redHealth = "RED";
        static string greenHealth = "GREEN";
        static HttpResponseMessage response = null;
        static string applicationName = "ERMS";
        static string dependencyType = "";

        static void Main(string[] args)
        {
            Console.WriteLine("READING CONFIG FILE.. PLEASE WAIT...");
            //Get Config file
            configFileXml = DependencyHealthChecker.GetConfigXmlDocument();
            Console.WriteLine("SUCCESFULLY READ CONFIG FILE");
            Console.WriteLine("PROCESSING IN PROGRESS.. PLEASE WAIT..");
            //Check health of web/REST services in app settings
            CheckAppSettings();
            //Check health of SQL DB connection strings
            CheckSqlConnectionStrings();
            Console.WriteLine("PARSING COMPLETED..");
            Console.WriteLine("DB WRITE IN PROGRESS..");
            DocumentDBManager.SaveData(results).Wait();
            Console.WriteLine("PROCESS COMPLETED..");
            //Console.WriteLine("Hit any key to continue..");
            //Console.Read();
        }

        static void CheckAppSettings()
        {
            XmlNodeList appsettingsList = configFileXml.SelectNodes("//appSettings");
            foreach (XmlNode xn in appsettingsList)
            {
                if (xn.HasChildNodes)
                {
                    foreach (XmlNode item in xn.ChildNodes)
                    {
                        if (item != null && item.Attributes != null && item.Attributes.Count == 2)
                        {
                            var health = redHealth;
                            try
                            {
                                using (var client = new HttpClient())
                                {
                                    //var uriStr = "www.google";
                                    client.BaseAddress = new Uri(item.Attributes[1].Value);

                                    response = client.GetAsync("").Result ??
                                               new HttpResponseMessage(HttpStatusCode.InternalServerError);

                                    if (response.StatusCode != HttpStatusCode.NotFound)
                                    {
                                        health = greenHealth;
                                    }
                                }
                            }
                            catch (UriFormatException e)
                            {
                                //Console.WriteLine("INVALID URL");
                                continue;
                            }
                            catch (Exception e)
                            {
                                health = redHealth;
                            }
                            dependencyType = "WebService / REST API";
                            results.Add(new HealthCheckResult
                            {
                                ApplicationName = applicationName,
                                Attribute = dependencyType,
                                Uri = item.Attributes[1].Value,
                                Health = health
                            });
                        }
                    }
                }
            }
        }

        static void CheckSqlConnectionStrings()
        {
            XmlNodeList connectionStringList = configFileXml.SelectNodes("//connectionStrings");
            foreach (XmlNode xn in connectionStringList)
            {
                if (xn.HasChildNodes)
                {
                    foreach (XmlNode item in xn.ChildNodes)
                    {
                        string name = string.Empty, providerName = string.Empty, connectionString = string.Empty;
                        if (item != null && item.Attributes != null && item.Attributes.Count == 3)
                        {
                            foreach (XmlAttribute attribute in item.Attributes)
                            {
                                if (attribute.Name == "providerName")
                                {
                                    providerName = attribute.Value;
                                }
                                else if (attribute.Name == "name")
                                {
                                    name = attribute.Value;
                                }
                                else if (attribute.Name == "connectionString")
                                {
                                    connectionString = attribute.Value;
                                }
                            }
                        }

                        if (providerName != "System.Data.SqlClient")
                        {
                            continue;
                        }

                        var conn = new SqlConnection(connectionString);
                        var health = redHealth;
                        try
                        {
                            conn.Open();
                            health = greenHealth;
                        }
                        catch
                        {

                        }
                        finally
                        {
                            conn.Close();
                        }
                        dependencyType = "DB Connection";
                        results.Add(new HealthCheckResult
                        {
                            ApplicationName = applicationName,
                            Attribute = dependencyType,
                            Uri = name,
                            Health = health
                        });
                    }
                }
            }
        }
    }
}
