using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace DocumentDBServiceLibrary
{
    public static class DocumentDBManager
    {
        private static DocumentClient client;
        private static readonly string endpointUrl = ConfigurationManager.AppSettings["EndPointUrl"];
        private static readonly string authorizationKey = ConfigurationManager.AppSettings["AuthorizationKey"];
        private static readonly string databaseId = ConfigurationManager.AppSettings["DatabaseId"];

        //The AppSettings["CollectionId"] should be changed in App settings according to the diffrent applications
        //eg. dependencyhealthresultscollection, peroformancetestresultscollection
        private static readonly string databaseCollectionId = ConfigurationManager.AppSettings["CollectionId"];

        public static async Task SaveData(object obj)
        {
            if (obj == null)
                return;

            try
            {
                using (client = new DocumentClient(new Uri(endpointUrl), authorizationKey))
                {
                    Database database = client.CreateDatabaseQuery().Where(db => db.Id == databaseId).AsEnumerable().FirstOrDefault();
                    if (database == null)
                    {
                        database = await client.CreateDatabaseAsync(new Database { Id = databaseId });
                    }
                    DocumentCollection documentCollection = client.CreateDocumentCollectionQuery(database.CollectionsLink).Where(c => c.Id == databaseCollectionId).AsEnumerable().FirstOrDefault();
                    if (documentCollection == null)
                    {
                        documentCollection = await client.CreateDocumentCollectionAsync(database.CollectionsLink, new DocumentCollection { Id = databaseCollectionId });
                    }
                    Document document = await client.CreateDocumentAsync(documentCollection.DocumentsLink, new { results = obj });
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }

    public class HealthCheckResult
    {
        public string ApplicationName { get; set; }
        public string Attribute { get; set; }
        public string Uri { get; set; }
        public string Health { get; set; }
    }
}