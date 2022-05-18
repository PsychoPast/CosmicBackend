using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CosmicBackend.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CosmicBackend.Core
{
    internal class MongoManager
    {
        private const string MongoCred = "mongodb+srv://CosmicAdmin:nQ2wBiMjaLGkM5wz@cosmicusers.jeual.mongodb.net/CosmicUsers?retryWrites=true&w=majority";
        private readonly IMongoCollection<BsonDocument> mongoCollection;

        internal MongoManager()
        {
            MongoClient mongoClient = new(MongoCred);
            mongoCollection = mongoClient.GetDatabase("CosmicUsers").GetCollection<BsonDocument>("Accounts");
        }

        internal async Task<bool> IsUsernameTakenAsync(string username)
        {
            IAsyncCursor<BsonDocument> documents = await GetDocumentsFilteredAsync("username", username).ConfigureAwait(false);
            while (await documents.MoveNextAsync(CancellationToken.None).ConfigureAwait(false))
            {
                if (documents.Current.Any(document => string.CompareOrdinal(document["username"].AsString, username) == 0))
                {
                    return true;
                }
            }

            return false;
        }

        internal async Task<bool> TryAddAccountAsync(User userAccount)
        {
            bool exists = await GetAccount(userAccount.Credentials.Email).ConfigureAwait(false) != null; ;
            if (!exists)
            {
                BsonDocument document = new()
                                            {
                                                { "username", userAccount.Username },
                                                { "email", userAccount.Credentials.Email },
                                                { "password", userAccount.Credentials.Password }
                                            };

                await mongoCollection.InsertOneAsync(document).ConfigureAwait(false);
            }

            return !exists;
        }

        internal async Task<User> GetAccount(string username)
        {
            BsonDocument document = await (await GetDocumentsFilteredAsync("username", username).ConfigureAwait(false))
                                         .FirstOrDefaultAsync<BsonDocument>().ConfigureAwait(false);

            return document != null
                       ? new()
                             {
                                 Username = document["username"].AsString,
                                 UserId = document["_id"].AsObjectId.ToString(),
                                 Credentials = new()
                                                   {
                                                       Email = document["email"].AsString,
                                                       Password = document["password"].AsString
                                                   }
                             }
                       : null;
        }

        internal async Task DeleteAccount(string userId)
        {
            BsonDocument document = await(await GetDocumentsFilteredAsync("_id", userId).ConfigureAwait(false))
                                         .FirstOrDefaultAsync().ConfigureAwait(false);
            if (document != null)
            {
                await mongoCollection.DeleteOneAsync(document).ConfigureAwait(false);
            }
        }

        private Task<IAsyncCursor<BsonDocument>> GetDocumentsFilteredAsync(string key, string value) =>
            mongoCollection.FindAsync(Builders<BsonDocument>.Filter.Eq(key, value));
    }
}