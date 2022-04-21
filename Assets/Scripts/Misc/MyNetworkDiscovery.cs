using System;
using System.Net;
using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.Events;

namespace MyDiscovery
{
    [Serializable]
    public class MyServerFoundUnityEvent : UnityEvent<MyDiscoveryResponse> { };
    /*
        Documentation: https://mirror-networking.gitbook.io/docs/components/network-discovery
        API Reference: https://mirror-networking.com/docs/api/Mirror.Discovery.NetworkDiscovery.html
    */

    public class DiscoveryRequest : NetworkMessage
    {
        // Add properties for whatever information you want sent by clients
        // in their broadcast messages that servers will consume.
    }

    public class MyDiscoveryResponse : NetworkMessage
    {
        //用于发现者的discoveredDictionary
        public long serverId;
        public Uri uri;
        public string hostname;
        // Add properties for whatever information you want the server to return to
        // clients for them to display or consume for establishing a connection.
    }

    public class MyNetworkDiscovery : NetworkDiscoveryBase<DiscoveryRequest, MyDiscoveryResponse>
    {
        [Tooltip("Transport to be advertised during discovery")]
        public Transport transport;
        [Tooltip("Invoked when a server is found")]
        public MyServerFoundUnityEvent OnServerFound;
        long ServerId;
        public string HostName { get; private set; }
        public override void Start()
        {
            ServerId = RandomLong();
            HostName = $"Hat {UnityEngine.Random.Range(0,256)}";
        }
        #region Server

        /// <summary>
        /// Reply to the client to inform it of this server
        /// </summary>
        /// <remarks>
        /// Override if you wish to ignore server requests based on
        /// custom criteria such as language, full server game mode or difficulty
        /// </remarks>
        /// <param name="request">Request coming from client</param>
        /// <param name="endpoint">Address of the client that sent the request</param>
        protected override void ProcessClientRequest(DiscoveryRequest request, IPEndPoint endpoint)
        {
            base.ProcessClientRequest(request, endpoint);
        }
        /// <summary>
        /// Process the request from a client
        /// </summary>
        /// <remarks>
        /// Override if you wish to provide more information to the clients
        /// such as the name of the host player
        /// </remarks>
        /// <param name="request">Request coming from client</param>
        /// <param name="endpoint">Address of the client that sent the request</param>
        /// <returns>A message containing information about this server</returns>
        protected override MyDiscoveryResponse ProcessRequest(DiscoveryRequest request, IPEndPoint endpoint)
        {
            try
            {
                return new MyDiscoveryResponse()
                {
                    serverId = ServerId,
                    hostname = HostName,
                    uri = transport.ServerUri()
                };
            }
            catch (NotImplementedException)
            {
                Debug.LogError($"Transport {transport} does not support network discovery");
                throw;
            }
        }

        public void SetHostName(string name)
        {
            HostName = name;
        }
        #endregion

        #region Client

        /// <summary>
        /// Create a message that will be broadcasted on the network to discover servers
        /// </summary>
        /// <remarks>
        /// Override if you wish to include additional data in the discovery message
        /// such as desired game mode, language, difficulty, etc... </remarks>
        /// <returns>An instance of ServerRequest with data to be broadcasted</returns>
        protected override DiscoveryRequest GetRequest()
        {
            return new DiscoveryRequest() { };
        }

        /// <summary>
        /// Process the answer from a server
        /// </summary>
        /// <remarks>
        /// A client receives a reply from a server, this method processes the
        /// reply and raises an event
        /// </remarks>
        /// <param name="response">Response that came from the server</param>
        /// <param name="endpoint">Address of the server that replied</param>
        protected override void ProcessResponse(MyDiscoveryResponse response, IPEndPoint endpoint)
        {

            // although we got a supposedly valid url, we may not be able to resolve
            // the provided host
            // However we know the real ip address of the server because we just
            // received a packet from it,  so use that as host.
            UriBuilder realUri = new UriBuilder(response.uri)
            {
                Host = endpoint.Address.ToString()
            };
            response.uri = realUri.Uri;
            OnServerFound.Invoke(response);
        }

        #endregion
    }
}