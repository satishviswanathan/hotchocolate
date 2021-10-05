using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using HotChocolate.AspNetCore.Utilities;
using Snapshooter.Xunit;
using Xunit;

namespace HotChocolate.AspNetCore
{
    public class HttpGetSchemaMiddlewareTests : ServerTestBase
    {
        public HttpGetSchemaMiddlewareTests(TestServerFactory serverFactory)
            : base(serverFactory)
        {
        }

        [Fact]
        public async Task Download_GraphQL_SDL()
        {
            // arrange
            TestServer server = CreateStarWarsServer();
            var url = TestServerExtensions.CreateUrl("/graphql?sdl");
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // act
            HttpResponseMessage response = await server.CreateClient().SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            result.MatchSnapshot();
        }

        [Fact]
        public async Task Download_GraphQL_SDL_Explicit_Route()
        {
            // arrange
            TestServer server = CreateServer(b => b.MapGraphQLSchema());
            var url = TestServerExtensions.CreateUrl("/graphql/sdl");
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // act
            HttpResponseMessage response = await server.CreateClient().SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            result.MatchSnapshot();
        }

        [Fact]
        public async Task Download_GraphQL_SDL_Explicit_Route_Explicit_Pattern()
        {
            // arrange
            TestServer server = CreateServer(b => b.MapGraphQLSchema("/foo/bar"));
            var url = TestServerExtensions.CreateUrl("/foo/bar");
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // act
            HttpResponseMessage response = await server.CreateClient().SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            result.MatchSnapshot();
        }

        [Fact]
        public async Task Download_GraphQL_SDL_Disabled()
        {
            // arrange
            TestServer server = CreateStarWarsServer(
                configureConventions: e => e.WithOptions(
                    new GraphQLServerOptions
                    {
                        EnableSchemaRequests = false
                    }));
            var url = TestServerExtensions.CreateUrl("/graphql?sdl");
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // act
            HttpResponseMessage response = await server.CreateClient().SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            result.MatchSnapshot();
        }
    }
}
