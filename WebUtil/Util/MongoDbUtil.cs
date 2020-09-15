using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebUtil.Util
{
    public class MongoDbUtil<T> where T : class
    {
        public IMongoCollection<T> Collection { get; private set; }

        public MongoDbUtil(IMongoDatabase database)
        {
            Collection = database.GetCollection<T>(typeof(T).Name);
        }

        public MongoDbUtil(IMongoDatabase database, string name)
        {
            Collection = database.GetCollection<T>(name);
        }

        public async Task<List<T>> All() =>
            (await Collection.FindAsync(t => true)).ToList();

        public T FindOne(FilterDefinition<T> filter) =>
            Collection.Find(filter).FirstOrDefault();

        public async Task<T> FindOneAsync(FilterDefinition<T> filter) =>
            await Collection.Find(filter).FirstOrDefaultAsync();

        public async Task<T> FindOneAsyncById(string id) =>
            await Collection.Find(Builders<T>.Filter.Eq("_id", ObjectId.Parse(id))).FirstOrDefaultAsync();

        public async Task<List<T>> FindAsync() =>
            (await Collection.FindAsync(t => true)).ToList();

        public async Task<List<T>> FindAsync(FilterDefinition<T> filter) =>
            (await Collection.FindAsync(filter)).ToList();

        public List<T> List(FilterDefinition<T> filter) => Collection.Find(filter).ToList();

        public T Create(T t)
        {
            Collection.InsertOne(t);
            return t;
        }

        public async Task<T> CreateAsync(T t)
        {
            await Collection.InsertOneAsync(t);
            return t;
        }

        public List<T> Create(List<T> t)
        {
            try
            {
                Collection.InsertMany(t);
            }
            catch (MongoBulkWriteException ex)
            {
                Console.Write(ex.Message);
            }
            return t;
        }

        public void Clear()
        {
            Collection.DeleteMany(x => true);
        }

        public void Update(string id, T t) =>
            Collection.ReplaceOne(Builders<T>.Filter.Eq("_id", ObjectId.Parse(id)), t);

        public void Update(FilterDefinition<T> filter, T t) =>
            Collection.ReplaceOne(filter, t);


        public async Task<T> UpdateAsync(FilterDefinition<T> filter, T t)
        {
            var result = await Collection.ReplaceOneAsync(filter, t);
            return result.ModifiedCount > 0 ? t : null;
        }

        public async Task<T> UpdateAsync(string id, T t)
        {
            var result = await Collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", ObjectId.Parse(id)), t);
            return result.ModifiedCount > 0 ? t : null;
        }

        public void Remove(FilterDefinition<T> filter) => Collection.DeleteOne(filter);

        public async Task<T> RemoveAsync(string id) => await Collection.FindOneAndDeleteAsync(Builders<T>.Filter.Eq("_id", ObjectId.Parse(id)));

        public async Task<T> RemoveAsync(FilterDefinition<T> filter) => await Collection.FindOneAndDeleteAsync(filter);

    }
}
