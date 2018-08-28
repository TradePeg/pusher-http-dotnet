﻿using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PusherServer.RestfulClient
{
    /// <summary>
    /// A client for the Pusher REST requests
    /// </summary>
    public class PusherRestClient : IPusherRestClient
    {
        private readonly string _libraryName;
        private readonly string _version;
        private readonly HttpClient _httpClient;
        private readonly WebClient _webClient;

        /// <summary>
        /// Constructs a new instance of the PusherRestClient
        /// </summary>
        /// <param name="baseAddress">The base address of the Pusher API as a URI formatted string</param>
        /// <param name="libraryName"></param>
        /// <param name="version"></param>
        public PusherRestClient(string baseAddress, string libraryName, Version version) : this(new Uri(baseAddress), libraryName, version)
        { }

        /// <summary>
        /// Constructs a new instance of the PusherRestClient
        /// </summary>
        /// <param name="baseAddress">The base address of the Pusher API</param>
        /// <param name="libraryName">The name of the Pusher Library</param>
        /// <param name="version">The version of the Pusher library</param>
        public PusherRestClient(Uri baseAddress, string libraryName, Version version)
        {
            _httpClient = new HttpClient { BaseAddress = baseAddress };
            _libraryName = libraryName;
            _version = version.ToString(3);

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("Pusher-Library-Name", _libraryName);
            _httpClient.DefaultRequestHeaders.Add("Pusher-Library-Version", _version);

            _webClient = new WebClient() { BaseAddress = baseAddress.ToString() };
            _webClient.Headers.Clear();
            _webClient.Headers["Accept"] = "application/json";
            _webClient.Headers["Pusher-Library-Name"] = _libraryName;
            _webClient.Headers["Pusher-Library-Version"] = _version;
        }

        ///<inheritDoc/>
        public Uri BaseUrl
        {
            get { return _httpClient.BaseAddress; }
        }

        ///<inheritDoc/>
        public async Task<GetResult<T>> ExecuteGetAsync<T>(IPusherRestRequest pusherRestRequest)
        {
            if (pusherRestRequest.Method == PusherMethod.GET)
            {
                var response = await _httpClient.GetAsync(pusherRestRequest.ResourceUri);
                var responseContent = await response.Content.ReadAsStringAsync();

                return new GetResult<T>(response, responseContent);
            }

            return null;
        }

        ///<inheritDoc/>
        public GetResult<T> ExecuteGet<T>(IPusherRestRequest pusherRestRequest)
        {
            if (pusherRestRequest.Method == PusherMethod.GET)
            {
                _webClient.Headers.Remove("Content-Type");
                var responseContent = _webClient.DownloadString(pusherRestRequest.ResourceUri);

                return new GetResult<T>(new HttpResponseMessage() { Content = new StringContent(responseContent), StatusCode = HttpStatusCode.OK }, responseContent);
            }

            return null;
        }

        ///<inheritDoc/>
        public async Task<TriggerResult> ExecutePostAsync(IPusherRestRequest pusherRestRequest)
        {
            if (pusherRestRequest.Method == PusherMethod.POST)
            {
                var content = new StringContent(pusherRestRequest.GetContentAsJsonString(), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(pusherRestRequest.ResourceUri, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                return new TriggerResult(response, responseContent);
            }

            return null;
        }

        ///<inheritDoc/>
        public TriggerResult ExecutePost(IPusherRestRequest pusherRestRequest)
        {
            if (pusherRestRequest.Method == PusherMethod.POST)
            {
                var content = new StringContent(pusherRestRequest.GetContentAsJsonString(), Encoding.UTF8, "application/json");

                _webClient.Headers["Content-Type"] = "application/json";
                var responseContent = _webClient.UploadString(pusherRestRequest.ResourceUri, "POST", pusherRestRequest.GetContentAsJsonString());

                return new TriggerResult(new HttpResponseMessage() { Content = new StringContent(responseContent), StatusCode = HttpStatusCode.OK }, responseContent);

            }

            return null;
        }
    }
}
