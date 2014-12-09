using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    ///  Product collection class
    /// </summary>
    public class AGMProducts : AGMEntityCollection<AGMProduct>
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="connection">connection</param>
        public AGMProducts(AGMConnection connection)
            : base(connection)
        {            
        }

        #region public methods 
        public static List<AGMProduct> GetProducts(AGMConnection conn)
        {
            var products = new List<AGMProduct>();

            string url = string.Format("{0}/agm/rest/domains/{1}/projects/{2}/products", conn.ServerName, conn.Domain, conn.Project);

            try
            {
                conn.Http.Method = "GET";
                conn.Http.ContentType = "application/xml";
                var status = conn.Http.Request(url);
                if (status >= 200 && status <= 204)
                {
                    if (!string.IsNullOrEmpty(conn.Http.Response))
                    {
                        var doc = new XmlDocument();
                        doc.LoadXml(conn.Http.Response);
                        XmlNodeList productlist = doc.SelectNodes(@"Entities/Entity");

                        foreach (XmlNode product in productlist)
                        {
                            var newProduct = new AGMProduct
                            {
                                Connection = conn,
                                Id = Convert.ToInt32(product.SelectSingleNode("./Fields/Field[@Name='id']").SelectSingleNode("Value").InnerText),
                                Name = product.SelectSingleNode("./Fields/Field[@Name='name']").SelectSingleNode("Value").InnerText,
                            };
                            products.Add(newProduct);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return products;
        }
        #endregion
    }

    /// <summary>
    /// requirement class
    /// </summary>
    public class AGMProduct : AGMEntity
    {
        #region private members
        #endregion       

        #region public Properties
        public string Name { get; set; }
        #endregion

        #region constructor
        #endregion

        #region public methods
        #endregion

        #region private methods
        #endregion
    }
}
