using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Redb.OBAC.ApiHost.Grpc
{
    public class SslCertificate
    {
        [JsonProperty("name")]
        public string CertName { get; set; }
    }
    public class SslOptions
    {
        [JsonProperty("basePath", Required = Required.Always)]
        public string BasePath { get; set; }

        [JsonProperty("caCertificate")] public string CaCertificatePath { get; set; } = "ca";

        [JsonProperty("forceClientAuth")]
        public bool ForceClientAuth  {get; set;} = false;

        [JsonProperty("certificates")]
        public SslCertificate[] Certificates { get; set; }

        public SslServerCredentials CreateSslCredentials()
        {
            if (string.IsNullOrWhiteSpace(BasePath))
                throw new Exception("Required param 'CertificateBasePath' is empty!");

            var pairs = Certificates
                .Select(CreateKeyCertificatePair);

            return new SslServerCredentials(pairs, GetCaCert(), ForceClientAuth);
        }

        private KeyCertificatePair CreateKeyCertificatePair(SslCertificate c)
        {
            try
            {
                var path = Path.Combine(BasePath, $"server-{c.CertName}");
                var cert = File.ReadAllText($"{path}.crt");
                var key = File.ReadAllText($"{path}.key");

                return new KeyCertificatePair(cert, key);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to read cert pair '{c.CertName}'!", ex);
            }
        }

        private string GetCaCert()
        {
            if (string.IsNullOrEmpty(CaCertificatePath))
                return DefaultSslCertificates.cCACert;

            try
            {
                return File.ReadAllText(Path.Combine(BasePath, $"{CaCertificatePath}.crt"));
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to read CA certificate from file '{CaCertificatePath}'!", ex);
            }
        }

    }
    public class Endpoint 
    {
        [JsonProperty("address", Required = Required.Always)]
        public string Address { get; set; }

        [JsonProperty("sslOptions")]//, NullValueHandling = NullValueHandling.Ignore)]
        public SslOptions SslOptions { get; set; } = null;

        public IPEndPoint GetEndpoint()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Address))
                    throw new Exception("Endpoint is empty!");

                var ss = Address.Split(':');
                return new IPEndPoint(IPAddress.Parse(ss[0]), Int32.Parse(ss[1]));
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Unable to get IP Endpoint from config! Correct format: '0.0.0.0:0000'. Actial value: '{Address}'. Message: {ex.Message}.");
            }
        }

        public ServerPort GetServerPort()
        {
            var endpoint = GetEndpoint();
            var cred = SslOptions?.Certificates?.Length > 0 
                ? SslOptions.CreateSslCredentials() 
                : ServerCredentials.Insecure;

            return new ServerPort(endpoint.Address.ToString(), endpoint.Port, cred);
        }

      
    }
    public class GrpcServerConfiguration
    {
        [JsonProperty("localhostPort")]
        public int LocalhostPort { get; set; } = 0;

        [JsonProperty("endpoints")]
        public List<Endpoint> Endpoints { get; set; } = new List<Endpoint>();

        [JsonProperty("shutdownTimeoutSec")]
        public int ShutdownTimeoutSec { get; set; } = 5;

        public IEnumerable<ServerPort> GetServerPorts()
        {
            var res = Endpoints
                .Select(o => o.GetServerPort())
                .ToList();

            if (LocalhostPort > 0)
            {
                var keyPair = new KeyCertificatePair(DefaultSslCertificates.cLocalhostCert,
                    DefaultSslCertificates.cLocalhostCertKey);

                res.Add(new ServerPort("127.0.0.1", LocalhostPort,
                    new SslServerCredentials(new[] { keyPair }, DefaultSslCertificates.cCACert, false)));
            }

            return res;
        }
    }
    public class GrpcServerExecutable
    {
        private readonly IEnumerable<ServerServiceDefinition> _serviceDefinitions;
        private Server _grpcServer;
        CancellationTokenRegistration? _tokenRegistration;
        private readonly ILogger _logger;
        private readonly GrpcServerConfiguration _configuration;
        private int _shutdownTimeoutSec = 30;

        public GrpcServerExecutable(ILogger logger, 
            GrpcServerConfiguration configuration, 
            IEnumerable<ServerServiceDefinition> serviceDefinitions)
        {
            _logger = logger;
            _configuration = configuration;
            
            _serviceDefinitions = serviceDefinitions;
            if (!_serviceDefinitions.Any())
                throw new ArgumentException("Service list is empty!", nameof(serviceDefinitions));
        }
        
        public Task Worker
        {
            get
            {
                if (_grpcServer == null)
                    throw new InvalidOperationException("Not started!");

                return _grpcServer.ShutdownTask;
            }
        }
        
        public Task StartAsync(CancellationToken token)
        {
            if (_grpcServer != null)
                throw new InvalidOperationException("Already started");

            _grpcServer = new Server();
            
            // add ports
            foreach (var port in _configuration.GetServerPorts())
            {
                var isSsl = port.Credentials is SslServerCredentials ? "SSL:" : "";
                _logger.Log(LogLevel.Information,$"Listening {isSsl}{port.Host}:{port.Port}.");
                _grpcServer.Ports.Add(port);
            }
            
            if (!_grpcServer.Ports.Any())
                throw new InvalidOperationException("At least one listening port must be configured!");

            foreach (var cmd in _serviceDefinitions)
            {
                try
                {
                    _grpcServer.Services.Add(cmd);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Warning,
                        $"Unable to instantiate gRPC service definition!", ex);
                }
            }

            _grpcServer.Start();
            _logger.Log(LogLevel.Debug,"GRPC server started!");

            
            _tokenRegistration = token.Register(() =>
            {
                _logger.Log(LogLevel.Information,"GRPC server shutdown started...");
                try
                {
                    var isOk = _grpcServer
                        .ShutdownAsync()
                        .Wait(_shutdownTimeoutSec * 1000);

                    if (isOk)
                        _logger.Log(LogLevel.Information,"GRPC server shutdown completed!");
                    else
                        _logger.Log(LogLevel.Information,"GRPC server shutdown with timeout!");
                    
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Warning,"GRPC server shutdown failed!", ex);
                }
            });

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Worker?.Dispose();
            _tokenRegistration?.Dispose();
        }
    }
}