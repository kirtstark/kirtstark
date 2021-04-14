using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDBDemo2
{
    /// <summary>
    /// This class is just for experimenting with Mongodb,
    /// creating the standard crud methods
    /// and testing ideas and thoughts
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            MongoCRUD db = new MongoCRUD("Addresses");

            
            Person person = new Person
            {
                FirstName = "Bob Arnold",
                LastName = "Johnson",
                DateOfBirth = new DateTime(1982, 10, 31, 0, 0, 0, DateTimeKind.Utc),
                PrimaryAddress = new AddressModel
                {
                    StreetAddress = "111 2nd street",
                    City = "New York",
                    State = "NJ",
                    Zip = "111111"

                },
                SecondaryAddress = new AddressModel
                {
                    StreetAddress = "1112 2nd street",
                    City = "New York",
                    State = "AA",
                    Zip = "line"
                }
            };

            db.InsertRecord("Users", person);

            person = new Person
            {
                FirstName = "Natalie",
                LastName = "Jones",
                DateOfBirth = new DateTime(1983, 10, 31, 0, 0, 0, DateTimeKind.Utc),
                PrimaryAddress = new AddressModel
                {
                    StreetAddress = "2222 B street",
                    City = "New York",
                    State = "NJ",
                    Zip = "111111"

                },
            };

            db.InsertRecord("Users", person);


            person = new Person
            {
                FirstName = "Seth",
                LastName = "Jones",
                DateOfBirth = new DateTime(1987, 10, 31, 0, 0, 0, DateTimeKind.Utc),
                PrimaryAddress = new AddressModel
                {
                    StreetAddress = "3333 L street",
                    City = "Town",
                    State = "NJ",
                    Zip = "111111"

                },
            };

            db.InsertRecord("Users", person);

            Console.WriteLine();
            Console.WriteLine("Records one **");
            Console.WriteLine();

            var records = db.LoadRecords<Person>("Users");

            foreach(var rec in records)
            {
                Console.WriteLine($"{rec.Id}: {rec.FirstName} {rec.LastName}");

                if (rec.PrimaryAddress != null)
                {
                    Console.WriteLine(rec.PrimaryAddress.StreetAddress);
                    Console.WriteLine(rec.PrimaryAddress.State);
                    Console.WriteLine(rec.PrimaryAddress.Zip);

                }
                Console.WriteLine();

                if (rec.SecondaryAddress != null)
                {
                    Console.WriteLine(rec.SecondaryAddress.StreetAddress);
                    Console.WriteLine(rec.SecondaryAddress.State);
                    Console.WriteLine(rec.SecondaryAddress.Zip);

                }
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("Records two **");
            Console.WriteLine();

            var records2 = db.LoadRecordByField<Person>("Users", "LastName", "Jones");

            foreach (var rec in records2)
            {
                Console.WriteLine($"{rec.Id}: {rec.FirstName} {rec.LastName}");

                if (rec.PrimaryAddress != null)
                {
                    Console.WriteLine(rec.PrimaryAddress.StreetAddress);
                    Console.WriteLine(rec.PrimaryAddress.State);
                    Console.WriteLine(rec.PrimaryAddress.Zip);

                }
                Console.WriteLine();

                if (rec.SecondaryAddress != null)
                {
                    Console.WriteLine(rec.SecondaryAddress.StreetAddress);
                    Console.WriteLine(rec.SecondaryAddress.State);
                    Console.WriteLine(rec.SecondaryAddress.Zip);

                }
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("Records by last name **");
            Console.WriteLine();

            var record = db.LoadRecordByLastName<Person>("Users", "Jones");
            Console.WriteLine($"{record.Id}: {record.FirstName} {record.LastName}");
            Console.WriteLine();

            record.FirstName = "Timothy";
            db.UpsertRecord<Person>("Users", record.Id, record);

            Console.WriteLine();
            Console.WriteLine("Records by last name after change **");
            Console.WriteLine();

            var record2b = db.LoadRecordByLastName<Person>("Users", "Jones");
            Console.WriteLine($"{record2b.Id}: {record2b.FirstName} {record2b.LastName}");
            Console.WriteLine();


            Console.WriteLine();
            Console.WriteLine("Records from address **");
            Console.WriteLine();


            // It doesn't seem to work to try pulling data from a child model:


            var records3 = db.LoadRecordByField<AddressModel>("Users", "Zip", "111111");

            foreach (var rec in records3)
            {
                Console.WriteLine("Address: " + rec.StreetAddress);
            }

            Console.WriteLine();
            Console.WriteLine("Records deletion **");
            Console.WriteLine();

            var records4 = db.LoadRecords<Person>("Users");

            foreach (var rec in records4)
            {
                db.DeleteRecord<Person>("Users", rec.Id);
            }

                Console.ReadLine();
        }

        public class Person
        {
            [BsonId]
            public Guid Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public  AddressModel PrimaryAddress { get; set; }
            public AddressModel SecondaryAddress { get; set; }
            [BsonElement("dob")]
            public DateTime DateOfBirth { get; set; }
        }

        public class AddressModel
        {
            public string StreetAddress { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Zip { get; set; }
        }

        public class MongoCRUD
        {
            private IMongoDatabase db;

            public MongoCRUD(string database)
            {
                MongoClient client = new MongoClient();
                db = client.GetDatabase(database);
            }

            public void InsertRecord<T>(string table, T record)
            {
                IMongoCollection<T> collection = db.GetCollection<T>(table);
                collection.InsertOne(record);
       
            }

            public List<T> LoadRecords<T>(string table)
            {
                var collection = db.GetCollection<T>(table);

                return collection.Find(new BsonDocument()).ToList();
            }

            public T LoadRecordById<T>(string table, Guid id)
            {
                var collection = db.GetCollection<T>(table);
                var filter = Builders<T>.Filter.Eq("Id", id);

                return collection.Find(filter).First();
            }

            public T LoadRecordByLastName<T>(string table, string name)
            {
                var collection = db.GetCollection<T>(table);
                var filter = Builders<T>.Filter.Eq("LastName", name);

                return collection.Find(filter).First();
            }

            public List<T> LoadRecordByField<T>(string table, string field, string value)
            {
                var collection = db.GetCollection<T>(table);
                var filter = Builders<T>.Filter.Eq(field, value);

                return collection.Find(filter).ToList();
            }

            public void UpsertRecord<T>(string table, Guid id, T record)
            {
                var collection = db.GetCollection<T>(table);
                var result = collection.ReplaceOne(
                    new BsonDocument("_id", id),
                    record,
                    new UpdateOptions { IsUpsert = true});
            }

            public void DeleteRecord<T>(string table, Guid id)
            {
                var collection = db.GetCollection<T>(table);
                var filter = Builders<T>.Filter.Eq("Id", id);
                collection.DeleteOne(filter);
            }
        }
    }
}
