﻿using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class UpdateNodeTests
    {
        [Test]
        public void ShouldUpdateNode()
        {
            var nodeToUpdate = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };

            var httpFactory = MockHttpFactory.Generate("http://foo/db/data", new Dictionary<RestRequest, HttpResponse>
            {
                {
                    new RestRequest { Resource = "/", Method = Method.GET },
                    new HttpResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content = @"{
                          'node' : 'http://foo/db/data/node',
                          'node_index' : 'http://foo/db/data/index/node',
                          'relationship_index' : 'http://foo/db/data/index/relationship',
                          'reference_node' : 'http://foo/db/data/node/0',
                          'extensions_info' : 'http://foo/db/data/ext',
                          'extensions'' : {
                          }
                        }".Replace('\'', '"')
                    }
                },
                 {
                    new RestRequest { Resource = "/node/456", Method = Method.GET },
                    new HttpResponse {
                        StatusCode = HttpStatusCode.OK,
                        ContentType = "application/json",
                        Content = @"{ 'self': 'http://foo/db/data/node/456',
                          'data': { 'Foo': 'foo',
                                    'Bar': 'bar',
                                    'Baz': 'baz'
                          },
                          'create_relationship': 'http://foo/db/data/node/456/relationships',
                          'all_relationships': 'http://foo/db/data/node/456/relationships/all',
                          'all_typed relationships': 'http://foo/db/data/node/456/relationships/all/{-list|&|types}',
                          'incoming_relationships': 'http://foo/db/data/node/456/relationships/in',
                          'incoming_typed relationships': 'http://foo/db/data/node/456/relationships/in/{-list|&|types}',
                          'outgoing_relationships': 'http://foo/db/data/node/456/relationships/out',
                          'outgoing_typed relationships': 'http://foo/db/data/node/456/relationships/out/{-list|&|types}',
                          'properties': 'http://foo/db/data/node/456/properties',
                          'property': 'http://foo/db/data/node/456/property/{key}',
                          'traverse': 'http://foo/db/data/node/456/traverse/{returnType}'
                        }".Replace('\'', '"')
                    }
                },
                {
                    new RestRequest {
                        Resource = "/node/456/properties",
                        Method = Method.PUT,
                        RequestFormat = DataFormat.Json
                    }.AddBody(nodeToUpdate),
                    new HttpResponse {
                        StatusCode = HttpStatusCode.OK
                    }
                }
            });

            var graphClient = new GraphClient(new Uri("http://foo/db/data"), httpFactory);
            graphClient.Connect();

            var pocoReference = new NodeReference<TestNode>(456);
            graphClient.Update(
                pocoReference, nodeFromDb =>
                    {
                        nodeFromDb.Foo = "fooUpdated";
                        nodeFromDb.Baz = "bazUpdated";
                        nodeToUpdate = nodeFromDb;
                    }
                );

            Assert.AreEqual("fooUpdated", nodeToUpdate.Foo);
            Assert.AreEqual("bazUpdated", nodeToUpdate.Baz);
            Assert.AreEqual("bar", nodeToUpdate.Bar);
        }

        public class TestNode
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            public string Baz { get; set; }
        }
    }
}