using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Service.Helpers;
using Service.Models;

namespace Service.Storage
{
    public class TableStorage : IStorage
    {
        private CloudTableClient _tableClient;
        private readonly string _tentant;

        private const string TABLE_PREFIX = "xxx";

        public static string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

        public TableStorage(string tentant, IConfiguration config)
        {
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.DefaultConnectionLimit = 100;
            ServicePointManager.Expect100Continue = false;


            // Retrieve storage account information from connection string.
            var customSetting = config["STORAGE_ACCOUNT"];
            CloudStorageAccount.TryParse(customSetting, out var storageAccount);

            // Create a table client for interacting with the table service
            _tableClient = storageAccount.CreateCloudTableClient();
            _tentant = tentant;
        }

        public TableStorage()
        {
        }

        public CloudTableClient GetTableClient()
        {
            return _tableClient;
        }

        public async Task DeleteCredential(byte[] id)
        {
            // improve this code so that only id is required
            var table = GetTenantTable();

            var existingCredential = await GetCredential(id);

            if (existingCredential == null) { throw new ApiException("credentialId does not exist or was already deleted.", 400); }

            // delete pointers
            var pk = "UH-" + existingCredential.UserHandle.Base64();
            var rk = Encoder.ToBase64StringStorageSafe(id);
            var op = TableOperation.Delete(new DynamicTableEntity(pk, rk) { ETag = "*" });

            // delete credentials
            var pk2 = "C-" + rk;
            var rk2 = "";
            var op2 = TableOperation.Delete(new DynamicTableEntity(pk2, rk2) { ETag = "*" });

            await Task.WhenAll(
            table.ExecuteAsync(op),
            table.ExecuteAsync(op2)
            );
        }

        public async Task<bool> TenantExists()
        {
            var table = GetTenantTable();
            return await table.ExistsAsync();
        }

        public async Task<string> GetUserIdByAliasAsync(string alias)
        {
            var table = GetTenantTable();

            // Create the filter

            var pk = "A>U-" + alias.Base64();
            var op = TableOperation.Retrieve<DynamicTableEntity>(pk, "");

            //Execute the query
            var result = await table.ExecuteAsync(op);

            var userid = (result.Result as DynamicTableEntity)?["userid"]?.StringValue;

            return userid;
        }

        public async Task<List<PublicKeyCredentialDescriptor>> GetCredentialsByAliasAsync(string alias)
        {
            var userid = await GetUserIdByAliasAsync(alias);

            var realresult = await GetCredentialsByUserIdAsync(userid, encodeBase64: false);
            return realresult;
        }

        public async Task<HashSet<string>> GetAliasesByUserId(byte[] userid)
        {
            var table = GetTenantTable();

            // Create the filter

            var pk = "U>A-" + userid.Base64();
            var op = TableOperation.Retrieve<AliasesRecord>(pk, "");

            //Execute the query
            var result = await table.ExecuteAsync(op);

            if (result.Result == null) { return null; }
            var data = (result.Result as AliasesRecord);

            return data?.Aliases;
        }

        public class AliasesRecord : TableEntity
        {
            [IgnoreProperty]
            public HashSet<string> Aliases { get; set; }

            public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
            {
                if (properties.TryGetValue(nameof(Aliases), out var aliases))
                {
                    Aliases = JsonConvert.DeserializeObject<HashSet<string>>(aliases.StringValue);
                }
            }

            public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
            {
                var properties = new Dictionary<string, EntityProperty>();
                properties.Add(nameof(Aliases), new EntityProperty(JsonConvert.SerializeObject(Aliases)));
                return properties;
            }
        }

        public async Task<List<PublicKeyCredentialDescriptor>> GetCredentialsByUserIdAsync(string userId, bool encodeBase64 = true)
        {

            var table = GetTenantTable();

            // Create the filter

            var pk = "UH-" + (encodeBase64 ? userId.Base64() : userId);
            string filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, pk);

            var tableQuery =
                   new TableQuery<TableEntityAdapter<PublicKeyCredentialDescriptor>>().Where(filter);

            //Execute the query
            var result = await table.ExecuteQueryAsync(tableQuery);

            var realResult = result.ToList().Select(e => e.OriginalEntity).ToList();

            return realResult;
        }


        public async Task DeleteAccount()
        {
            var table = GetTenantTable();
            await table.DeleteIfExistsAsync();
        }


        public async Task LockAllApiKeys(bool isLocked)
        {
            var table = GetTenantTable();
            var filter = "PartitionKey ge 'KEY-' and PartitionKey le 'KEYX'";
            var tableQuery =
                  new TableQuery<TableEntityAdapter<ApiKeyDesc>>().Where(filter).Select(new List<string>() { });

            var r = await table.ExecuteQueryAsync(tableQuery);

            var tbo = new List<Task>();
            foreach (var key in r)
            {
                key.OriginalEntity.IsLocked = isLocked;
                tbo.Add(table.ExecuteAsync(TableOperation.Replace(key)));
            }

            await Task.WhenAll(tbo);
        }

        public async Task SaveAccountInformation(AccountMetaInformation info)
        {
            var table = GetTenantTable();

            var pk = "M-ACCOUNTINFO";

            var tea = new TableEntityAdapter<AccountMetaInformation>(info, pk, "");

            var op = TableOperation.InsertOrReplace(tea);

            await table.ExecuteAsync(op);
        }

        public async Task<AccountMetaInformation> GetAccountInformation()
        {
            var table = GetTenantTable();

            var pk = "M-ACCOUNTINFO";

            var op = TableOperation.Retrieve<TableEntityAdapter<AccountMetaInformation>>(pk, "");

            var result = await table.ExecuteAsync(op);

            return (result.Result as TableEntityAdapter<AccountMetaInformation>).OriginalEntity;
        }

        public async Task<List<TokenKey>> GetTokenKeys()
        {
            var table = GetTenantTable();
            var pk = "M-TOKENKEY";
            string filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, pk);

            var tableQuery =
                   new TableQuery<TableEntityAdapter<TokenKey>>().Where(filter);

            //Execute the query
            var result = await table.ExecuteQueryAsync(tableQuery);

            var realResult = result.ToList().Select(e => e.OriginalEntity).ToList();

            return realResult;
        }

        public Task AddTokenKey(TokenKey tokenKey)
        {
            var table = GetTenantTable();
            var pk = "M-TOKENKEY";
            var rk = tokenKey.KeyId;
            var tea = new TableEntityAdapter<TokenKey>(tokenKey, pk, rk.ToString());
            var op = TableOperation.Insert(tea);
            return table.ExecuteAsync(op);
        }

        public Task RemoveTokenKey(int keyId)
        {
            var table = GetTenantTable();
            var pk = "M-TOKENKEY";
            var rk = keyId;
            var op = TableOperation.Delete(new DynamicTableEntity(pk, rk.ToString()) { ETag = "*" });
            return table.ExecuteAsync(op);
        }


        public async Task<ApiKeyDesc> GetApiKeyAsync(string apiKey)
        {
            var table = GetTenantTable();

            var pk = apiKey.Substring(apiKey.Length - 4);

            var op = TableOperation.Retrieve<TableEntityAdapter<ApiKeyDesc>>("KEY-" + pk, "");
            var result = await table.ExecuteAsync(op);

            return (result.Result as TableEntityAdapter<ApiKeyDesc>).OriginalEntity;
        }

        public async Task StoreApiKey(string pkpart, string apikey, string[] scopes)
        {
            // PK: PK-{apikey}
            // RK: 
            // fields: scopes
            var table = GetTenantTable();

            await table.CreateIfNotExistsAsync();

            var pk = "KEY-" + pkpart;

            var key = new ApiKeyDesc() { AccountName = _tentant, ApiKey = apikey, Scopes = scopes };
            var tea = new TableEntityAdapter<ApiKeyDesc>(key, pk, "");

            var op = TableOperation.Insert(tea);

            await table.ExecuteAsync(op);
        }

        public async Task<StoredCredential> GetCredential(byte[] credentialId)
        {
            var table = GetTenantTable();

            var pk = "C-" + credentialId.Base64();

            var op = TableOperation.Retrieve<TableEntityAdapter<StoredCredential>>(pk, "");

            var result = await table.ExecuteAsync(op);

            return (result.Result as TableEntityAdapter<StoredCredential>)?.OriginalEntity;
        }

        private CloudTable GetTenantTable()
        {
            return _tableClient.GetTableReference(TABLE_PREFIX + _tentant);
        }

        public async Task<bool> ExistsAsync(byte[] credentialId)
        {
            // todo: possible micro optimization is not to return all fields
            return await GetCredential(credentialId) != null;
        }

        public async Task AddCredentialToUser(Fido2User user, StoredCredential cred)
        {
            var table = GetTenantTable();

            // store credential
            var pk = "C-" + cred.Descriptor.Id.Base64();
            var rk = "";
            var ad = new TableEntityAdapter<StoredCredential>(cred, pk, rk);
            var insertOrMergeOperation = TableOperation.InsertOrReplace(ad);

            await table.ExecuteAsync(insertOrMergeOperation);

            // store user pointer
            var pk2 = "UH-" + user.Id.Base64();
            var rk2 = cred.Descriptor.Id.Base64();
            var ad2 = new TableEntityAdapter<PublicKeyCredentialDescriptor>(cred.Descriptor, pk2, rk2);
            var op2 = TableOperation.InsertOrReplace(ad2);
            await table.ExecuteAsync(op2);
        }

        public async Task UpdateCredential(byte[] credentialId, uint counter, string country, string device)
        {
            var table = GetTenantTable();
            var pk = "C-" + credentialId.Base64();
            var rk = "";

            var ad = new DynamicTableEntity(pk, rk);

            // todo: Better way to store uint int Table Storage?
            var x = new EntityProperty((int)counter);
            ad.Properties.Add("SignatureCounter", x);
            ad.Properties.Add("LastUsedAt", new EntityProperty(DateTime.UtcNow));
            ad.Properties.Add("Country", EntityProperty.GeneratePropertyForString(country));
            ad.Properties.Add("Device", EntityProperty.GeneratePropertyForString(device));
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(ad);

            await table.ExecuteAsync(insertOrMergeOperation);
        }

        public async Task StoreAlias(byte[] userid, HashSet<string> newAliases)
        {
            var table = GetTenantTable();

            List<TableOperation> operations = new List<TableOperation>();
            if (newAliases == null)
            {
                // do nothing
                return;
            }

            // check if aliases already exist for other user
            var getops = new List<TableOperation>();
            var useridb64 = userid.Base64();
            foreach (var newalias in newAliases)
            {
                var pk = "A>U-" + newalias.Base64();
                var op = TableOperation.Retrieve<DynamicTableEntity>(pk, "");
                getops.Add(op);
            }

            var existing = await Task.WhenAll(getops.Select(table.ExecuteAsync));
            foreach (var result in existing)
            {
                if (result.Result != null)
                {
                    var existing_userid = (result.Result as DynamicTableEntity)?["userid"]?.StringValue;
                    if (existing_userid != useridb64) throw new ApiException($"Alias is already in use. It's registered to user ({existing_userid.Base64Decode()})", 409);
                }
            }

            // Update for current user
            var storedOnUser = await GetAliasesByUserId(userid);
            if (storedOnUser != null)
            {
                // delete all a>u stored
                foreach (var existingAlias in storedOnUser)
                {
                    if (newAliases.Contains(existingAlias)) continue;

                    var pk = "A>U-" + existingAlias.Base64();
                    operations.Add(TableOperation.Delete(new DynamicTableEntity(pk, "", "*", new Dictionary<string, EntityProperty>())));
                }
            }

            // insert all a>u
            foreach (var alias in newAliases)
            {
                operations.Add(WriteAliasToUserId(userid, alias));
            }

            // insert u>a json
            operations.Add(WriteUserIdToAliases(userid, newAliases));

            await Task.WhenAll(operations.Select(table.ExecuteAsync));
        }

        private TableOperation WriteUserIdToAliases(byte[] userid, HashSet<string> aliases)
        {
            var pk = "U>A-" + userid.Base64();
            var rk = "";


            //var ad = new TableEntityAdapter<AliasesRecord>(new AliasesRecord() { Aliases = aliases }, pk, rk);
            var a = new AliasesRecord() { Aliases = aliases };
            a.PartitionKey = pk;
            a.RowKey = rk;
            TableOperation insertOrReplace = TableOperation.InsertOrReplace(a);

            return insertOrReplace;
        }

        private TableOperation WriteAliasToUserId(byte[] userid, string alias)
        {
            var pk = "A>U-" + alias.Base64();
            var rk = "";


            var ad = new DynamicTableEntity(pk, rk);
            ad.Properties.Add("userid", EntityProperty.GeneratePropertyForString(userid.Base64()));
            TableOperation insertOrReplace = TableOperation.InsertOrReplace(ad);

            return insertOrReplace;
        }
    }
}
