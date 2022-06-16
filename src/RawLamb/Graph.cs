using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Neo4j.Driver;

namespace RawLamb
{
    public class N4jGraph : IDisposable
    {
        private bool _disposed = false;
        private readonly IDriver _driver;

        ~N4jGraph()
        {
            Dispose(false);
        }

        public N4jGraph(string uri, string user, string password)
        {
            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password), o => 
            o.WithConnectionAcquisitionTimeout(TimeSpan.FromSeconds(2))
            );

        }

        [Obsolete]
        public string deprec_DoTransaction(string transaction)
        {
            using (var session = _driver.Session())
            {
                var result = session.WriteTransaction(tx =>
                {
                    var res = tx.Run(transaction);
                    return res.Single()[0].As<string>();
                });

                return result;
            }
        }

        public Dictionary<string, object> ToDictionary(IRecord record)
        {
            var dict = new Dictionary<string, object>();
            foreach (var key in record.Keys)
            {
                dict[key] = record[key];
            }

            return dict;
        }

        public Dictionary<string, object> ToDictionary(INode node)
        {
            var dict = new Dictionary<string, object>();
            dict["labels"] = node.Labels.ToList<string>();
            dict["properties"] = node.Properties.ToDictionary(x => x.Key, y=> y.Value);
            dict["id"] = node.Id;

            return dict;
        }

        public List<Dictionary<string, object>> DoTransaction(string transaction)
        {
            using (var session = _driver.Session())
            {
                var result = session.WriteTransaction(tx =>
                {
                    var res = tx.Run(transaction);
                    return res.Select(x => ToDictionary(x)).ToList<Dictionary<string, object>>();
                    //return res.Single()[0].As<string>();
                });

                return result;
            }
        }

        public void PrintGreeting(string message)
        {
            using (var session = _driver.Session())
            {
                var greeting = session.WriteTransaction(tx =>
                {
                    var result = tx.Run("MERGE (a:Greeting {name:'Tom'}) " +
                      "SET a.message = $message " +
                      "RETURN a.message + ', from node ' + id(a)",
                      new { message });
                    return result.Single()[0].As<string>();
                });
                Console.WriteLine(greeting);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _driver.Dispose();
            }

            _disposed = true;
        }
    }
}
