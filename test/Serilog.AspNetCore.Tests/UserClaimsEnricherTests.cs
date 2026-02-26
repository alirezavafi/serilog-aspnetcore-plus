using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.AspNetCore.Tests.Support;
using Serilog.Core;
using Serilog.Enrichers;
using Serilog.Events;
using Serilog.Parsing;
using Xunit;

namespace Serilog.AspNetCore.Tests
{
    public class UserClaimsEnricherTests
    {
        [Fact]
        public void Enrich_ShouldAddAllClaims_WhenNoWhiteListOrBlackList()
        {
            // Arrange
            UserClaimsEnricher.BlacklistedClaims.Clear();
            UserClaimsEnricher.WhitelistedClaims.Clear();
            var enricher = new UserClaimsEnricher();
            var logEvent = CreateLogEvent();
            var propertyFactory = new MockPropertyFactory();

            var claims = new List<Claim>
            {
                new Claim("test-claim-1", "value-1"),
                new Claim("test-claim-2", "value-2")
            };
            var httpContext = CreateHttpContext(claims);
            
            var accessor = new HttpContextAccessor();
            accessor.HttpContext = httpContext;

            // Act
            enricher.Enrich(logEvent, propertyFactory);

            // Assert
            Assert.True(logEvent.Properties.ContainsKey("User"));
            var userProp = logEvent.Properties["User"] as StructureValue;
            Assert.NotNull(userProp);
            Assert.Equal(2, userProp.Properties.Count);
        }

        [Fact]
        public void Enrich_ShouldRespectBlacklist()
        {
            // Arrange
            UserClaimsEnricher.BlacklistedClaims.Clear();
            UserClaimsEnricher.WhitelistedClaims.Clear();
            UserClaimsEnricher.BlacklistedClaims.Add("test-claim-1");

            var enricher = new UserClaimsEnricher();
            var logEvent = CreateLogEvent();
            var propertyFactory = new MockPropertyFactory();

            var claims = new List<Claim>
            {
                new Claim("test-claim-1", "value-1"),
                new Claim("test-claim-2", "value-2")
            };
            var httpContext = CreateHttpContext(claims);
            
            var accessor = new HttpContextAccessor();
            accessor.HttpContext = httpContext;

            // Act
            enricher.Enrich(logEvent, propertyFactory);

            // Assert
            Assert.True(logEvent.Properties.ContainsKey("User"));
            var userProp = logEvent.Properties["User"] as StructureValue;
            Assert.NotNull(userProp);
            Assert.Single(userProp.Properties);
            Assert.Equal("test-claim-2", userProp.Properties[0].Name);
        }

        [Fact]
        public void Enrich_ShouldRespectWhitelist()
        {
            // Arrange
            UserClaimsEnricher.BlacklistedClaims.Clear();
            UserClaimsEnricher.WhitelistedClaims.Clear();
            UserClaimsEnricher.WhitelistedClaims.Add("test-claim-1");

            var enricher = new UserClaimsEnricher();
            var logEvent = CreateLogEvent();
            var propertyFactory = new MockPropertyFactory();

            var claims = new List<Claim>
            {
                new Claim("test-claim-1", "value-1"),
                new Claim("test-claim-2", "value-2")
            };
            var httpContext = CreateHttpContext(claims);
            
            var accessor = new HttpContextAccessor();
            accessor.HttpContext = httpContext;

            // Act
            enricher.Enrich(logEvent, propertyFactory);

            // Assert
            Assert.True(logEvent.Properties.ContainsKey("User"));
            var userProp = logEvent.Properties["User"] as StructureValue;
            Assert.NotNull(userProp);
            Assert.Single(userProp.Properties);
            Assert.Equal("test-claim-1", userProp.Properties[0].Name);
        }

        [Fact]
        public void Enrich_ShouldRespectIgnoredClaimsAlias()
        {
            // Arrange
            UserClaimsEnricher.BlacklistedClaims.Clear();
            UserClaimsEnricher.WhitelistedClaims.Clear();
            UserClaimsEnricher.BlacklistedClaims.Add("test-claim-1");

            var enricher = new UserClaimsEnricher();
            var logEvent = CreateLogEvent();
            var propertyFactory = new MockPropertyFactory();

            var claims = new List<Claim>
            {
                new Claim("test-claim-1", "value-1"),
                new Claim("test-claim-2", "value-2")
            };
            var httpContext = CreateHttpContext(claims);
            
            var accessor = new HttpContextAccessor();
            accessor.HttpContext = httpContext;

            // Act
            enricher.Enrich(logEvent, propertyFactory);

            // Assert
            Assert.True(logEvent.Properties.ContainsKey("User"));
            var userProp = logEvent.Properties["User"] as StructureValue;
            Assert.NotNull(userProp);
            Assert.Single(userProp.Properties);
            Assert.Equal("test-claim-2", userProp.Properties[0].Name);
        }

        [Fact]
        public void Enrich_ShouldRespectBothWhitelistAndBlacklist()
        {
            // Arrange
            UserClaimsEnricher.BlacklistedClaims.Clear();
            UserClaimsEnricher.WhitelistedClaims.Clear();
            UserClaimsEnricher.WhitelistedClaims.Add("test-claim-1");
            UserClaimsEnricher.WhitelistedClaims.Add("test-claim-2");
            UserClaimsEnricher.BlacklistedClaims.Add("test-claim-1");

            var enricher = new UserClaimsEnricher();
            var logEvent = CreateLogEvent();
            var propertyFactory = new MockPropertyFactory();

            var claims = new List<Claim>
            {
                new Claim("test-claim-1", "value-1"),
                new Claim("test-claim-2", "value-2"),
                new Claim("test-claim-3", "value-3")
            };
            var httpContext = CreateHttpContext(claims);
            
            var accessor = new HttpContextAccessor();
            accessor.HttpContext = httpContext;

            // Act
            enricher.Enrich(logEvent, propertyFactory);

            // Assert
            Assert.True(logEvent.Properties.ContainsKey("User"));
            var userProp = logEvent.Properties["User"] as StructureValue;
            Assert.NotNull(userProp);
            Assert.Single(userProp.Properties);
            Assert.Equal("test-claim-2", userProp.Properties[0].Name);
        }

        private LogEvent CreateLogEvent()
        {
            return new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplate(new List<MessageTemplateToken>()), new List<LogEventProperty>());
        }

        private HttpContext CreateHttpContext(IEnumerable<Claim> claims)
        {
            var context = new DefaultHttpContext();
            var identity = new ClaimsIdentity(claims, "TestAuth");
            context.User = new ClaimsPrincipal(identity);
            return context;
        }

        private class MockPropertyFactory : ILogEventPropertyFactory
        {
            public LogEventProperty CreateProperty(string name, object? value, bool destructureObjects = false)
            {
                return new LogEventProperty(name, MapValue(value));
            }

            private LogEventPropertyValue MapValue(object? value)
            {
                if (value is Dictionary<string, object> dict)
                {
                    var props = new List<LogEventProperty>();
                    foreach (var kvp in dict)
                    {
                        props.Add(new LogEventProperty(kvp.Key, MapValue(kvp.Value)));
                    }
                    return new StructureValue(props);
                }
                if (value is Array array)
                {
                    var elements = new List<LogEventPropertyValue>();
                    foreach (var item in array)
                    {
                        elements.Add(MapValue(item));
                    }
                    return new SequenceValue(elements);
                }
                return new ScalarValue(value);
            }
        }
    }
}
