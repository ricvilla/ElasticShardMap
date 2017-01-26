using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticShardMap
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = GetConnectionString(ConfigurationManager.AppSettings["ServerName"],"Ricardo");

            ShardMapManager shardMapManager;
            bool shardMapManagerExists = ShardMapManagerFactory.TryGetSqlShardMapManager(
                                                connectionString,
                                                ShardMapManagerLoadPolicy.Lazy,
                                                out shardMapManager);

            if (shardMapManagerExists)
            {
                Console.WriteLine("Shard Map Manager already exists");
            }
            else
            {
                // Create the Shard Map Manager. 
                ShardMapManagerFactory.CreateSqlShardMapManager(connectionString);
                Console.WriteLine("Created SqlShardMapManager");

                shardMapManager = ShardMapManagerFactory.GetSqlShardMapManager(
                    connectionString,
                    ShardMapManagerLoadPolicy.Lazy);

                // The connectionString contains server name, database name, and admin credentials 
                // for privileges on both the GSM and the shards themselves.
            }
        }

        public static string GetConnectionString(string serverName, string database)
        {
            SqlConnectionStringBuilder connStr = new SqlConnectionStringBuilder(GetCredentialsConnectionString());
            connStr.DataSource = serverName;
            connStr.InitialCatalog = database;
            return connStr.ToString();
        }

        public static string GetCredentialsConnectionString()
        {
            // Get User name and password from the app.config file. If they don't exist, default to string.Empty.
            string userId = ConfigurationManager.AppSettings["UserName"] ?? string.Empty;
            string password = ConfigurationManager.AppSettings["Password"] ?? string.Empty;

            // Get Integrated Security from the app.config file. 
            // If it exists, then parse it (throw exception on failure), otherwise default to false.
            string integratedSecurityString = ConfigurationManager.AppSettings["IntegratedSecurity"];
            bool integratedSecurity = integratedSecurityString != null && bool.Parse(integratedSecurityString);

            SqlConnectionStringBuilder connStr = new SqlConnectionStringBuilder
            {
                // DDR and MSQ require credentials to be set
                UserID = userId,
                Password = password,
                IntegratedSecurity = integratedSecurity,

                // DataSource and InitialCatalog cannot be set for DDR and MSQ APIs, because these APIs will
                // determine the DataSource and InitialCatalog for you.
                //
                // DDR also does not support the ConnectRetryCount keyword introduced in .NET 4.5.1, because it
                // would prevent the API from being able to correctly kill connections when mappings are switched
                // offline.
                //
                // Other SqlClient ConnectionString keywords are supported.

                ApplicationName = "ESC_SKv1.0",
                ConnectTimeout = 30
            };
            return connStr.ToString();
        }
    }

   
}
