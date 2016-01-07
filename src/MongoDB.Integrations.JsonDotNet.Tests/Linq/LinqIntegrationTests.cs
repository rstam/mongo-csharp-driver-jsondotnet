/* Copyright 2015 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System.Linq;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;

namespace MongoDB.Integrations.JsonDotNet.Tests.Linq
{
    [TestFixture]
    public class LinqIntegrationTests
    {
        #region static
        // private static fields
        private static IMongoClient __client;
        private static IMongoCollection<C> __collection;
        private static IMongoDatabase __database;

        // public static methods
        [TestFixtureSetUp]
        public static void TestFixtureSetup()
        {
            JsonDotNetSerializationProviderRegisterer.EnsureProviderIsRegistered();
            __client = new MongoClient("mongodb://localhost");
            __database = __client.GetDatabase("JsonDotNetIntegrationTests");
            __collection = __database.GetCollection<C>("c");
            InitializeCollection();
        }

        // private static methods
        private static void InitializeCollection()
        {
            __database.DropCollection(__collection.CollectionNamespace.CollectionName);
            __collection.InsertMany(new C[]
            {
                new C { Id = 1, X = 1, A = new[] { 1, 2, 3 } },
                new C { Id = 2, X = 2, A = new[] { 4, 5, 6 } }
            });
        }
        #endregion

        // public methods
        [Test]
        public void Linq_query_against_array_property_should_return_expected_result()
        {
            var results = __collection.AsQueryable()
                .Where(x => x.A.Contains(1))
                .ToList();

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(1);
        }

        [Test]
        public void Linq_query_against_scalar_property_should_return_expected_result()
        {
            var results = __collection.AsQueryable()
                .Where(x => x.X == 1)
                .ToList();

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(1);
        }

        [Test]
        public void ToJson_should_use_registered_serialization_provider()
        {
            var c = new C { Id = 1, X = 2, A = new[] { 3, 4, 5 } };

            var json = c.ToJson();

            json.Should().Be("{ \"_id\" : 1, \"x\" : 2, \"a\" : [3, 4, 5] }");
        }


        // nested types
        [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.OptIn)]
        private class C
        {
            [Newtonsoft.Json.JsonProperty("_id")]
            public int Id { get; set; }
            [Newtonsoft.Json.JsonProperty("x")]
            public int X { get; set; }
            [Newtonsoft.Json.JsonProperty("a")]
            public int[] A { get; set; }
        }
    }
}
