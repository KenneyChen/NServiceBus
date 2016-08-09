﻿namespace NServiceBus.Core.Tests.Serializers
{
    using System;
    using System.Collections.Generic;
    using MessageInterfaces.MessageMapper.Reflection;
    using Serialization;
    using NUnit.Framework;

    [TestFixture]
    public class MessageDeserializerResolverTests
    {
        MessageDeserializerResolver resolver;

        [SetUp]
        public void Setup()
        {
            var mapper = new MessageMapper();
            var xml = new XmlMessageSerializer(mapper, new Conventions());
            var json = new JsonMessageSerializer(mapper);
            resolver = new MessageDeserializerResolver(xml, new IMessageSerializer[]
            {
                xml,
                json
            });
        }

        [TestCase(ContentTypes.Xml, typeof(XmlMessageSerializer))]
        [TestCase(ContentTypes.Json, typeof(JsonMessageSerializer))]
        public void RetrievesSerializerByContentType(string contentType, Type expected)
        {
            var headers = new Dictionary<string, string>
            {
                {Headers.ContentType, contentType}
            };
            var serializer = resolver.Resolve(headers);
            Assert.IsInstanceOf(expected, serializer);
        }

        [Test]
        public void UnknownContentTypeFallsBackToDefaultSerialization()
        {
            var headers = new Dictionary<string, string>
            {
                {Headers.ContentType, "unknown/unsupported"}
            };
            var serializer = resolver.Resolve(headers);
            Assert.IsInstanceOf<XmlMessageSerializer>(serializer);
        }

        [Test]
        public void NoContentTypeFallsBackToDefaultSerialization()
        {
            var headers = new Dictionary<string, string>();
            var serializer = resolver.Resolve(headers);
            Assert.IsInstanceOf<XmlMessageSerializer>(serializer);
        }

        [Test]
        public void MultipleDeserializersWithSameContentTypeShouldThrowException()
        {
            var mapper = new MessageMapper();
            var xml = new XmlMessageSerializer(mapper, new Conventions());
            var json1 = new JsonMessageSerializer(mapper);
            var json2 = new JsonMessageSerializer(mapper);

            Assert.That(() => new MessageDeserializerResolver(xml, new IMessageSerializer[]
            {
                json1,
                json2
            }), Throws.Exception.TypeOf<Exception>().And.Message.Contains("Multiple additional deserializers are registered for content-type 'application/json'. Remove ambiguous deserializers for this content-type."));
        }
    }
}