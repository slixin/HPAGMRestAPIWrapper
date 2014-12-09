using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace HPAGMRestAPIWrapper
{
    /// <summary>
    /// Abstract class for AGMEntity Collection, be inherited by each Entity class, for example: Defect, Test, Requirement
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AGMEntityCollection<T> where T : AGMEntity, new()
    {
        #region Priavte Members
        #endregion

        #region Public Properties
        /// <summary>
        /// Connection object of AGM connection
        /// </summary>
        public AGMConnection Connection { get; set; }
        /// <summary>
        /// Entity fields 
        /// </summary>
        public List<AGMField> EntityFields { get; set; }
        /// <summary>
        /// Types
        /// </summary>
        public List<AGMType> Types { get; set; }
        /// <summary>
        /// Users
        /// </summary>
        public List<AGMUser> Users { get; set; }
        /// <summary>
        /// Releases
        /// </summary>
        public List<AGMRelease> Releases { get; set; }
        /// <summary>
        /// Release cycle - Sprint
        /// </summary>
        public List<AGMReleaseCycle> ReleaseCycles { get; set; }
        /// <summary>
        /// Products
        /// </summary>
        public List<AGMProduct> Applications { get; set; }
        /// <summary>
        /// Entity Rest API URL
        /// </summary>
        public string RestUrl { get; set; }
        /// <summary>
        /// Entity: defect, requirement, test, run, defect-link, test-set, test-folder, testset-folder
        /// </summary>
        public string Entity { get; set; }

        public int? EntityType { get; set; }

        #endregion

        #region constructor
        /// <summary>
        /// Entity Collection
        /// </summary>
        /// <param name="connection"></param>
        public AGMEntityCollection(AGMConnection connection)
        {
            Connection = connection;
            Entity = Common.GetEntityType(typeof(T).Name).Trim().Replace("\"", "").Substring(0, Common.GetEntityType(typeof(T).Name).Trim().Length - 1); // Get entity type, for example: defect/test/run
            RestUrl = string.Format("{0}/agm/rest/domains/{1}/projects/{2}/{3}s", connection.ServerName, connection.Domain, connection.Project, Entity); // Get Rest URL for different entity type.
            EntityFields = new AGMField().GetFields(connection, Entity, EntityType); // Get all fields definitions from different entity type, each project or version with different fields definitions.
            Types = AGMType.GetTypes(connection, Entity); // Get all types definition from different entity type, not each entity with sub-types.
            Users = AGMUser.GetUsers(connection); // Get users
            Releases = AGMReleases.GetReleases(connection);
            ReleaseCycles = AGMReleaseCycles.GetReleaseCycles(connection);
            Applications = AGMProducts.GetProducts(connection);
        }
        #endregion        

        #region public methods
        /// <summary>
        /// Get entity object by its ID
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="fields">Return Fields for the entity</param>
        /// <returns>Entity</returns>
        public virtual T Get(int entityId, List<AGMField> returnFields, List<AGMField> sortFields)
        {
            string sortFieldsString = null;
            string returnFieldsString = null;

            var urlSb = new StringBuilder();
            try
            {
                #region Build Query string and Field string

                if (returnFields != null)
                    returnFieldsString = Common.BuildReturnFieldsString(returnFields);
                if (sortFields != null)
                    sortFieldsString = Common.BuildSortFieldsString(sortFields);

                urlSb.Append(string.Format("{0}/{1}", RestUrl, entityId));
                if (!string.IsNullOrEmpty(returnFieldsString))
                    urlSb.Append(string.Format("?{0}", returnFieldsString));

                if (!string.IsNullOrEmpty(sortFieldsString))
                    urlSb.Append(string.Format("{0}", sortFieldsString));
                #endregion

                #region Request to get entity
                Connection.Http.Method = "GET";
                Connection.Http.ContentType = "application/xml";
                var status = Connection.Http.Request(urlSb.ToString());
                if (status >= 200 && status <= 204)
                {
                    if (!string.IsNullOrEmpty(Connection.Http.Response))
                    {
                        var doc = new XmlDocument();
                        doc.LoadXml(Connection.Http.Response);
                        T entityItem = LoadEntityFromXml(doc.SelectSingleNode("Entity"));
                        if (entityItem != null)
                        {
                            try
                            {
                                var audits = entityItem.GetEntityAudits(Entity);
                                entityItem.Audits = audits == null ? new List<AGMAudit>() : audits;
                            }
                            catch { }
                        }                            

                        return entityItem;
                    }
                }
                #endregion

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Get entity collection by the parent Id
        /// </summary>
        /// <param name="parentId">Parent Id for entity collection</param>
        /// <param name="fields">Return Fields for the entity</param>
        /// <returns>List of Entity</returns>
        public virtual List<T> GetCollection(int? parentId, List<AGMField> returnFields, List<AGMField> sortFields)
        {
            if (parentId == null)  throw new Exception("Parent Id for Entity cannot be empty.");
            if (EntityFields.Where(o=>o.Name.Equals("parent-id", StringComparison.InvariantCultureIgnoreCase)).Count() != 1) 
                throw new Exception("The entity type without the field 'parent-id'."); // Check if there is a field which named 'parent-id' in the entity fields definitions, if not, try second GetCollection methods.

            var queryFields = new List<AGMField> { new AGMField { Name = "parent-id", Value = parentId.ToString() } };

            return GetCollection(queryFields, returnFields, sortFields);
        }

        /// <summary>
        /// Get entity collection by the filter conditions
        /// </summary>
        /// <param name="queryFields">Filter conditions for the entity collection</param>
        /// <param name="returnFields">Return Fields for the entity</param>
        /// <returns>List of Entity</returns>
        public virtual List<T> GetCollection(List<AGMField> queryFields, List<AGMField> returnFields, List<AGMField> sortFields)
        {
            string sortFieldsString = null;
            string queryFieldsString = null;
            string returnFieldsString = null;

            var collection = new List<T>();
            var urlSb = new StringBuilder();

            try
            {
                #region Build Query string and Field string
                queryFieldsString = Common.BuildQueryFieldsString(queryFields);
                if (returnFields != null)
                    returnFieldsString = Common.BuildReturnFieldsString(returnFields);
                if (sortFields != null)
                    sortFieldsString = Common.BuildSortFieldsString(sortFields);

                urlSb.Append(string.Format("{0}/?page-size=2000&", RestUrl));
                if (!string.IsNullOrEmpty(queryFieldsString))
                    urlSb.Append(string.Format("{0}&", queryFieldsString));

                if (!string.IsNullOrEmpty(returnFieldsString))
                    urlSb.Append(string.Format("{0}&", returnFieldsString));

                if (!string.IsNullOrEmpty(sortFieldsString))
                    urlSb.Append(string.Format("{0}", sortFieldsString));

                #endregion

                #region Request to Get Collection
                Connection.Http.Method = "GET";
                Connection.Http.ContentType = "application/xml";
                var status = Connection.Http.Request(urlSb.ToString());

                if (status >= 200 && status <= 204)
                {
                    var response = Connection.Http.Response;
                    if (!string.IsNullOrEmpty(response))
                    {
                        #region Load entity info to object list
                        var doc = new XmlDocument();
                        doc.LoadXml(response);
                        var entityNodes = doc.SelectNodes("Entities/Entity");

                        foreach (XmlNode entityNode in entityNodes)
                        {
                            T entityItem = LoadEntityFromXml(entityNode);
                            if (entityItem != null)
                                entityItem.Audits = entityItem.GetEntityAudits(Entity);

                            if (entityItem.Fields.Count > 0)
                                collection.Add(entityItem);
                        }

                        #endregion
                    }
                #endregion
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return collection;
        }

        /// <summary>
        /// Check entity is exists.
        /// </summary>
        /// <param name="entityId">Entity Id</param>
        /// <returns>true of false</returns>
        public virtual bool IsExists(int entityId)
        {
            var result = false;

            try
            {
                var t = Get(entityId, null, null);
                if (t != null)
                    result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Create entity
        /// </summary>
        /// <param name="entityFields">Entity Fields which will be used to create.</param>
        /// <returns>New Entity</returns>
        public virtual T Create(List<AGMField> entityFields)
        {
            if (entityFields.Count == 0)   throw new Exception("Entity fields cannot be empty.");

            CheckRequiredFieldsAreReady(entityFields);

            var urlSb = new StringBuilder();
            urlSb.Append(string.Format("{0}", RestUrl));

            try
            {
                var xmlContent = Common.BuildEntityXML(entityFields, Entity);
                
                #region Request to Create Entity
                Connection.Http.Method = "POST";
                Connection.Http.ContentType = "application/xml";
                Connection.Http.Accept = "application/xml";
                Connection.Http.PostData = xmlContent;
                var status = Connection.Http.Request(urlSb.ToString());
                if (status >= 200 && status <= 204)
                {
                    var response = Connection.Http.Response;
                    if (!string.IsNullOrEmpty(response))
                    {
                        var doc = new XmlDocument();
                        doc.LoadXml(response);

                        return LoadEntityFromXml(doc.SelectSingleNode("Entity"));
                    }
                }
                #endregion

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Update Entity
        /// </summary>
        /// <param name="entityId">Entity Id which need to be updated.</param>
        /// <param name="entityFields">Entity fields which need to be updated.</param>
        /// <returns>Updated Entity</returns>
        public virtual T Update(int entityId, List<AGMField> entityFields)
        {
            if (entityFields.Count == 0) throw new Exception("Entity fields cannot be empty.");

            var urlSb = new StringBuilder();
            urlSb.Append(string.Format(@"{0}/{1}", RestUrl, entityId));

            try
            {
                var xmlContent = Common.BuildEntityXML(entityFields, Entity);

                #region Request to Update entity
                Connection.Http.Method = "PUT";
                Connection.Http.ContentType = "application/xml";
                Connection.Http.Accept = "application/xml";
                Connection.Http.PostData = xmlContent;
                var status = Connection.Http.Request(urlSb.ToString());

                if (status >= 200 && status <= 204)
                {
                    var response = Connection.Http.Response;
                    if (!string.IsNullOrEmpty(response))
                    {
                        var doc = new XmlDocument();
                        doc.LoadXml(response);

                        return LoadEntityFromXml(doc.SelectSingleNode("Entity"));
                    }
                }
                #endregion

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Delete Entity
        /// </summary>
        /// <param name="entityId">Entity Id which need to be deleted</param>
        public virtual void Delete(int entityId)
        {
            try
            {
                var query = string.Format(@"{0}/{1}", RestUrl, entityId);

                #region Request to Delete Entity
                Connection.Http.Method = "DELETE";
                Connection.Http.Request(query);
                #endregion
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Find folder by path
        /// </summary>
        /// <param name="folderPath">Folder path, must be start with '\', example: \Requirement\Release1\Create</param>
        /// <returns>Folder Entity</returns>
        public virtual T FindFolderByPath(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath)) throw new Exception("The folder path cannot be empty.");
            if (!folderPath.StartsWith(@"\"))   throw new Exception(@"The folder path must be start with \ .");

            T tFolder = null;

            var parentId = -1;
            foreach (var foldername in folderPath.Remove(0, 1).Split(new[] { '\\' }))
            {
                tFolder = GetFolder(foldername, parentId);
                if (tFolder == null)
                    throw new Exception(string.Format("Cannot find the folder: {0} ", foldername));

                parentId = tFolder.Id.Value;
            }

            return tFolder;
        }

        /// <summary>
        /// Get optionals for Entity
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetOptionals(List<AGMField> queryFields)
        {
            Dictionary<string, string> options = new Dictionary<string, string>();
            List<AGMField> returnFields = new List<AGMField>();
            returnFields.Add(new AGMField { Name = "id" });
            returnFields.Add(new AGMField { Name = "name" });
            List<T> entities = GetCollection(queryFields, returnFields, null);
            foreach (T entity in entities)
            {
                options.Add(entity.GetField("id").Value, entity.GetField("name").Value);
            }

            return options;
        }
        
        public AGMField NormalizeField(List<AGMField> EntityFields, AGMField field)
        {
            AGMField nField = null;  
        
            // If the input field name is the actual field name
            if (EntityFields.Where(o => o.Name.Equals(field.Name, StringComparison.InvariantCultureIgnoreCase) && o.Entity.Equals(field.Entity, StringComparison.InvariantCultureIgnoreCase)).Count() == 1)
            {
                nField = new AGMField { Name = field.Name, Value = field.Value, Entity = field.Entity };
            } // If the input field name is the field label
            else if (EntityFields.Where(o => o.Label.Trim().Equals(field.Name, StringComparison.InvariantCultureIgnoreCase) && o.Entity.Equals(field.Entity, StringComparison.InvariantCultureIgnoreCase)).Count() == 1)
            {
                var currentField = EntityFields.Where(o => o.Label.Trim().Equals(field.Name, StringComparison.InvariantCultureIgnoreCase) && o.Entity.Equals(field.Entity, StringComparison.InvariantCultureIgnoreCase)).Single() as AGMField;
                nField = new AGMField { Name = currentField.Name, Value = field.Value, Entity = field.Entity };
            }

            return nField;
        }

        public List<AGMField> NormalizedFields(List<AGMField> fields, List<AGMField> backlogItemEntityFields)
        {
            List<AGMField> normalizedFields = new List<AGMField>();

            if (fields == null)
                return null;

            foreach (AGMField field in fields)
            {
                if (field.Entity.Equals("release-backlog-item"))
                {
                    normalizedFields.Add(NormalizeField(backlogItemEntityFields, field));
                }
                else
                {
                    normalizedFields.Add(NormalizeField(EntityFields, field));
                }
            }

            return normalizedFields;
        }
        #endregion

        #region private methods
        private void CheckRequiredFieldsAreReady(List<AGMField> fields)
        {
            foreach (AGMField field in fields)
            {
                if (!string.IsNullOrEmpty(field.Name))
                {
                    if (EntityFields.Where(o => o.Name.Equals(field.Name, StringComparison.InvariantCultureIgnoreCase) && o.Required).Count() == 0)
                        throw new Exception(string.Format("Field '{0}' for '{1}' is required, please fill in.", field.Name, typeof(T).Name));
                }
            }
        }

        private T LoadEntityFromXml(XmlNode node)
        {
            var t = new T 
            {
                EntityXml = node.OuterXml,
                RestUrl = RestUrl,
                Connection = Connection,
                Fields = new List<AGMField>(),
                RelatedEntities = new Dictionary<string, List<AGMField>>(),
            };

            foreach (AGMField field in EntityFields)
            {
                t.Fields.Add(field.DeepCopy());
            }
            
            t.Load();

            return t; 
        }

        private T GetFolder(string foldername, int parentId)
        {
            if (!EntityType.Equals("test-folder") && !EntityType.Equals("testset-folder"))
                throw new Exception("Only Test Folder and TestSet Folder ared supported.");

            var queries = new List<AGMField>();
            queries.Add(new AGMField { Name = "name", Value = foldername });
            
            if (parentId >= 0)
                queries.Add(new AGMField { Name = "parent-id", Value = parentId.ToString() });


            var folderlist = GetCollection(queries, null, null);

            if (folderlist.Count == 0)  throw new Exception(string.Format("Cannot find the folder: {0} ", foldername));
            if (folderlist.Count > 1)  throw new Exception(string.Format("Found multiple folders: {0} ", foldername));

            return folderlist[0];
        }        
        #endregion
    }

    /// <summary>
    /// Entity class, be inherited by specified entity, for example: Defect/Requirement/Test
    /// </summary>
    public class AGMEntity
    {
        #region private members
        #endregion

        #region Public properties
        /// <summary>
        /// Store the Rest URL for the entity
        /// </summary>
        public string RestUrl { get; set; }

        /// <summary>
        /// Store the Connection for the entity
        /// </summary>
        public AGMConnection Connection { get; set; }

        /// <summary>
        /// Store the response XML content for the entity.
        /// </summary>
        public string EntityXml { get; set; }

        /// <summary>
        /// Entity Id, when it is null, means for searching operation or create operation.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Store all fields and values for the entity
        /// </summary>
        public List<AGMField> Fields { get; set; }

        public Dictionary<string, List<AGMField>> RelatedEntities { get; set; }

        /// <summary>
        /// Audits
        /// </summary>
        public List<AGMAudit> Audits { get; set; }
        #endregion

        #region constructor

        #endregion

        #region public method
        #region Attachment Related Methods
        /// <summary>
        /// Upload attachment
        /// </summary>
        /// <param name="file">upload attachment file path</param>
        public void UploadAttachment(string file)
        {
            var query = string.Format(@"{0}/{1}/attachments", RestUrl, Id);

            if (!File.Exists(file))
                throw new Exception(string.Format("File {0} does not existed.", file));

            try
            {
                Connection.Http.PostFilePath = file;
                Connection.Http.Method = "POST";
                Connection.Http.ContentType = "application/octet-stream";
                if (Connection.Http.RequestHeaders.ContainsKey("Slug"))
                    Connection.Http.RequestHeaders["Slug"] = Path.GetFileName(file);
                else
                    Connection.Http.RequestHeaders.Add("Slug", Path.GetFileName(file));
                Connection.Http.Request(query);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Download attachment from AGM
        /// </summary>
        /// <param name="attachmentName">attachment file name in AGM</param>
        /// <param name="file">local file path</param>
        public void DownloadAttachment(string attachmentName, string file)
        {
            var query = string.Format(@"{0}/{1}/attachments/{2}", RestUrl, Id, attachmentName);

            try
            {
                Connection.Http.DownloadFile(query, file);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Download Attachment Fail! {0}", ex.InnerException.Message));
            }
        }

        /// <summary>
        /// Delete Attachment in AGM
        /// </summary>
        /// <param name="attachmentName">attachment file name</param>
        public void DeleteAttachment(string attachmentName)
        {
            var query = string.Format(@"{0}/{1}/attachments/{2}", RestUrl, Id, attachmentName);

            try
            {
                Connection.Http.Method = "DELETE";
                Connection.Http.ContentType = "application/octet-stream";
                Connection.Http.Request(query);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Check attachment is exists in AGM
        /// </summary>
        /// <param name="attachmentName">Attachment file name</param>
        /// <returns></returns>
        public bool IsAttachmentExists(string attachmentName)
        {
            var query = string.Format(@"{0}/{1}/attachments/{2}", RestUrl, Id, attachmentName);
            var result = false;

            try
            {
                Connection.Http.Method = "GET";
                Connection.Http.ContentType = "application/octet-stream";
                var status = Connection.Http.Request(query);

                if (status >= 200 && status <= 204)
                    result = true;
            }
            catch (Exception ex)
            {
                if (ex.Message.IndexOf("No entity of type attachment with id") >= 0)
                    result = false;
                else
                    throw new Exception(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// List attachments in AGM
        /// </summary>
        /// <returns></returns>
        public List<string> ListAttachments()
        {
            var query = string.Format(@"{0}/{1}/attachments", RestUrl, Id);
            List<string> attachments = new List<string>();

            try
            {
                Connection.Http.Method = "GET";
                Connection.Http.ContentType = "application/octet-stream";
                var status = Connection.Http.Request(query);

                if (status >= 200 && status <= 204)
                {
                    if (!string.IsNullOrEmpty(Connection.Http.Response))
                    {
                        var doc = new XmlDocument();
                        doc.LoadXml(Connection.Http.Response);

                        XmlNodeList entityNodes = doc.SelectNodes("Entities/Entity");
                        if (entityNodes != null)
                        {
                            foreach (XmlNode entityNode in entityNodes)
                            {
                                attachments.Add(entityNode.SelectSingleNode("Fields/Field[@Name='name']").InnerText);
                            }                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return attachments;
        }
        #endregion

        #region Common Methods
        /// <summary>
        /// Delete Entity
        /// </summary>
        public void Delete()
        {
            Delete(false);
        }

        /// <summary>
        /// Delete Entity with children
        /// </summary>
        /// <param name="isDeleteChildren">true = delete all chilren</param>
        public void Delete(bool isDeleteChildren)
        {
            if (Id == null)
                throw new Exception("Entity Id cannot be empty.");

            try
            {
                var query = string.Format(@"{0}/{1}", RestUrl, Id);

                if (isDeleteChildren)
                    query = string.Format("{0}?{1}", query, "force-delete-children=y");

                Connection.Http.Method = "DELETE";
                Connection.Http.Request(query);           
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Set fields which need to be updated
        /// </summary>
        /// <param name="Name">field name</param>
        /// <param name="value">field value</param>
        public void SetUpdateField(string Name, string value)
        {
            if (Fields.Where(o => o.Name.Equals(Name)).Count() == 1)
            {
                Fields.Where(o => o.Name.Equals(Name)).First().Value = value;
                Fields.Where(o => o.Name.Equals(Name)).First().IsToUpdate = true;
            }
            else if (Fields.Where(o => o.Label.Trim().Equals(Name, StringComparison.InvariantCultureIgnoreCase)).Count() == 1)
            {
                Fields.Where(o => o.Label.Trim().Equals(Name, StringComparison.InvariantCultureIgnoreCase)).First().Value = value;
                Fields.Where(o => o.Label.Trim().Equals(Name, StringComparison.InvariantCultureIgnoreCase)).First().IsToUpdate = true;
            }                
            else
                throw new Exception(string.Format("Cannot find the field '{0}'.", Name));
        }

        /// <summary>
        /// Get field by field name
        /// </summary>
        /// <param name="Name">field name</param>
        /// <returns></returns>
        public AGMField GetField(string Name)
        {
            if (Fields.Where(o => o.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase)).Count() == 1)
                return Fields.Where(o => o.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase)).First();
            if (Fields.Where(o => o.Label.Trim().Equals(Name, StringComparison.InvariantCultureIgnoreCase)).Count() == 1)
                return Fields.Where(o => o.Label.Trim().Equals(Name, StringComparison.InvariantCultureIgnoreCase)).First();
            throw new Exception(string.Format("Cannot find the field '{0}'.", Name));
        }
        #endregion        

        /// <summary>
        /// Load entity information
        /// </summary>
        public void Load()
        {
            if (string.IsNullOrEmpty(EntityXml)) throw new Exception("Entity Xml is empty.");
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(EntityXml);

                var entityNode = doc.SelectSingleNode("Entity");
                Id = Convert.ToInt32(Common.GetEntityId(entityNode.SelectSingleNode("Fields")));
                var entityType = doc.SelectSingleNode("Entity").Attributes["Type"].Value;

                var fields = entityNode.SelectNodes("Fields/Field");
                foreach (XmlNode field in fields)
                {
                    var Name = field.Attributes["Name"].InnerText;
                    string Value = null;
                    if (field.HasChildNodes) Value = field.SelectSingleNode("Value").InnerText;

                    Fields.Where(o => o.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase)).Single().Value = Value;
                    //Fields.Add(new AGMField { Name = Name, Value = Value, Entity = entityType });
                }

                var relatedEntitiesNode = entityNode.SelectSingleNode("RelatedEntities");
                if (relatedEntitiesNode != null)
                {
                    if (relatedEntitiesNode.HasChildNodes)
                    {
                        List<AGMField> relatedEntitiesFields = new List<AGMField>();
                        var relatedEntityType = relatedEntitiesNode.SelectSingleNode("Relation/Entity").Attributes["Type"].Value;
                        foreach (XmlNode reField in relatedEntitiesNode.SelectNodes("Relation/Entity/Fields/Field"))
                        {
                            var Name = reField.Attributes["Name"].InnerText;
                            string Value = null;
                            if (reField.HasChildNodes) Value = reField.SelectSingleNode("Value").InnerText;
                            relatedEntitiesFields.Add(new AGMField { Name = Name, Value = Value, Entity = relatedEntityType });
                        }

                        RelatedEntities.Add(relatedEntityType, relatedEntitiesFields);
                    }                    
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Build dictionary from fields in entity
        /// </summary>
        /// <returns></returns>
        public List<AGMField> GetFieldsDictionary()
        {
            var fields = new List<AGMField>();
            foreach (var AGMCf in Fields.Where(o => o.IsToUpdate))
            {
                fields.Add(new AGMField { Name = AGMCf.Name, Value = AGMCf.Value });
                AGMCf.IsToUpdate = false;
            }

            return fields;
        }
        /// <summary>
        /// Get History of the entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<AGMAudit> GetEntityAudits(string entity)
        {
            try
            {
                AGMAudits audits = new AGMAudits(Connection);
                return audits.GetAudits(Id.Value, string.Format("{0}s", entity));
            }
            catch
            {

            }

            return null;
            
        }
        #endregion

        #region Private methods
        
        #endregion
    }
}
