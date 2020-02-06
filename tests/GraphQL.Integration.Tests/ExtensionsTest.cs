using System;
using System.Collections.Generic;
using System.Text.Json;
using Dahomey.Json;
using FluentAssertions;
using GraphQL.Client;
using GraphQL.Integration.Tests.Extensions;
using GraphQL.Integration.Tests.Helpers;
using IntegrationTestServer;
using IntegrationTestServer.ChatSchema;
using Xunit;

namespace GraphQL.Integration.Tests {
	public class ExtensionsTest {
		private static TestServerSetup SetupTest(bool requestsViaWebsocket = false) =>
			WebHostHelpers.SetupTest<StartupChat>(requestsViaWebsocket);

		[Fact]
		public async void CanDeserializeExtensions() {

			using var setup = SetupTest();
			var response = await setup.Client.SendQueryAsync(new GraphQLRequest("query { extensionsTest }"),
					() => new {extensionsTest = ""})
				.ConfigureAwait(false);

			response.Errors.Should().NotBeNull();
			response.Errors.Should().ContainSingle();
			response.Errors[0].Extensions.Should().NotBeNull();

			JsonElement data = new JsonElement();
			response.Errors[0].Extensions.Value.Invoking(element => data = element.GetProperty("data")).Should()
				.NotThrow();

			foreach (var item in ChatQuery.TestExtensions) {
				JsonElement value = new JsonElement();
				data.Invoking(element => value = element.GetProperty(item.Key)).Should().NotThrow();

				switch (item.Value) {
					case int i:
						value.GetInt32().Should().Be(i);
						break;
					default:
						value.GetString().Should().BeEquivalentTo(item.Value.ToString());
						break;
				}
			}
		}

		[Fact]
		public async void DontNeedToUseCamelCaseNamingStrategy() {

			using var setup = SetupTest();
			setup.Client.Options.JsonSerializerOptions = new JsonSerializerOptions().SetupExtensions();

			const string message = "some random testing message";
			var graphQLRequest = new GraphQLRequest(
				@"mutation($input: MessageInputType){
				  addMessage(message: $input){
				    content
				  }
				}",
				new {
					input = new {
						fromId = "2",
						content = message,
						sentAt = DateTime.Now
					}
				});
			var response = await setup.Client.SendMutationAsync(graphQLRequest, () => new { addMessage = new { content = "" } });

			Assert.Equal(message, response.Data.addMessage.content);
		}
	}
}