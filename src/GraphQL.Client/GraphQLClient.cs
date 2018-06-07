using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Internal.Http;
using GraphQL.Common.Request;
using GraphQL.Common.Response;

namespace GraphQL.Client {

	/// <summary>
	/// A Client to access GraphQL EndPoints
	/// </summary>
	public partial class GraphQLClient : IGraphQLClient {

		#region Properties

		/// <summary>
		/// Gets the headers which should be sent with each request.
		/// </summary>
		public HttpRequestHeaders DefaultRequestHeaders => this.graphQLHttpHandler.HttpClient.DefaultRequestHeaders;

		/// <summary>
		/// The GraphQL EndPoint to be used
		/// </summary>
		public Uri EndPoint {
			get => this.Options.EndPoint;
			set => this.Options.EndPoint = value;
		}

		/// <summary>
		/// The Options	to be used
		/// </summary>
		public GraphQLClientOptions Options {
			get => this.graphQLHttpHandler.Options;
			set => this.graphQLHttpHandler.Options = value;
		}

		#endregion

		private readonly GraphQLHttpHandler graphQLHttpHandler;

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="endPoint">The EndPoint to be used</param>
		public GraphQLClient(string endPoint) : this(new Uri(endPoint)) { }

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="endPoint">The EndPoint to be used</param>
		public GraphQLClient(Uri endPoint) : this(new GraphQLClientOptions { EndPoint = endPoint }) { }

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="endPoint">The EndPoint to be used</param>
		/// <param name="options">The Options to be used</param>
		public GraphQLClient(string endPoint, GraphQLClientOptions options) : this(new Uri(endPoint), options) { }

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="endPoint">The EndPoint to be used</param>
		/// <param name="options">The Options to be used</param>
		public GraphQLClient(Uri endPoint, GraphQLClientOptions options) {
			if (options == null) { throw new ArgumentNullException(nameof(options)); }
			if (options.EndPoint == null) { throw new ArgumentNullException(nameof(options.EndPoint)); }
			if (options.JsonSerializerSettings == null) { throw new ArgumentNullException(nameof(options.JsonSerializerSettings)); }
			if (options.HttpMessageHandler == null) { throw new ArgumentNullException(nameof(options.HttpMessageHandler)); }
			if (options.MediaType == null) { throw new ArgumentNullException(nameof(options.MediaType)); }

			options.EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
			this.graphQLHttpHandler = new GraphQLHttpHandler(options);
		}

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="options">The Options to be used</param>
		public GraphQLClient(GraphQLClientOptions options) {
			if (options == null) { throw new ArgumentNullException(nameof(options)); }
			if (options.EndPoint == null) { throw new ArgumentNullException(nameof(options.EndPoint)); }
			if (options.JsonSerializerSettings == null) { throw new ArgumentNullException(nameof(options.JsonSerializerSettings)); }
			if (options.HttpMessageHandler == null) { throw new ArgumentNullException(nameof(options.HttpMessageHandler)); }
			if (options.MediaType == null) { throw new ArgumentNullException(nameof(options.MediaType)); }

			this.graphQLHttpHandler = new GraphQLHttpHandler(options);
		}

		public async Task<GraphQLResponse> SendQueryAsync(string query, CancellationToken cancellationToken = default) =>
			await this.SendQueryAsync(new GraphQLRequest { Query = query }, cancellationToken).ConfigureAwait(false);

		public async Task<GraphQLResponse> SendQueryAsync(GraphQLRequest request, CancellationToken cancellationToken = default) =>
			await this.graphQLHttpHandler.PostAsync(request, cancellationToken).ConfigureAwait(false);

		public async Task<GraphQLResponse> SendMutationAsync(string query, CancellationToken cancellationToken = default) =>
			await this.SendMutationAsync(new GraphQLRequest { Query = query }, cancellationToken).ConfigureAwait(false);

		public async Task<GraphQLResponse> SendMutationAsync(GraphQLRequest request, CancellationToken cancellationToken = default) =>
			await this.graphQLHttpHandler.PostAsync(request, cancellationToken).ConfigureAwait(false);

		[Obsolete("EXPERIMENTAL API")]
		public async Task<GraphQLSubscriptionResult> SendSubscribeAsync(string query, CancellationToken cancellationToken = default) =>
			await this.SendSubscribeAsync(new GraphQLRequest { Query = query }, cancellationToken).ConfigureAwait(false);

		[Obsolete("EXPERIMENTAL API")]
		public async Task<GraphQLSubscriptionResult> SendSubscribeAsync(GraphQLRequest request, CancellationToken cancellationToken = default) {
			if (request == null) { throw new ArgumentNullException(nameof(request)); }
			if (request.Query == null) { throw new ArgumentNullException(nameof(request.Query)); }

			var webSocketUri = new Uri($"ws://{this.EndPoint.Host}:{this.EndPoint.Port}{this.EndPoint.AbsolutePath}");
			var graphQLSubscriptionResult = new GraphQLSubscriptionResult(webSocketUri, request);
			graphQLSubscriptionResult.StartAsync(cancellationToken);
			return await Task.FromResult(graphQLSubscriptionResult).ConfigureAwait(false);
		}

		/// <summary>
		/// Releases unmanaged resources
		/// </summary>
		public void Dispose() =>
			this.graphQLHttpHandler.Dispose();

	}

}
