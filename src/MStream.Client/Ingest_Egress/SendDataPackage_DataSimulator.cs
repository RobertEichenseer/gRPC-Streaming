using System;
using Grpc.Core;
using Grpc.Net.Client;
using MStream.ServiceDefinition;

namespace MStream.Client
{
    public class SendGetDataPackage_DataSimulator : ISendGetData
    {
        ILogger<SendGetDataPackage_DataSimulator> _logger; 
        Guid _appGuid;

        string _serverAddress = "https://localhost:5001"; 
        GrpcChannel _grpcChannel;
        Ingest.IngestClient _ingestClient;  
        Egress.EgressClient _egressClient; 
        IConfiguration _configuration; 

        public SendGetDataPackage_DataSimulator(ILogger<SendGetDataPackage_DataSimulator> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration; 

            _serverAddress = string.IsNullOrEmpty(_configuration["MSTREAM_SERVERURL"]) ? "https://localhost:5001" : _configuration["MSTREAM_SERVERURL"]; 
            _appGuid = new Guid("bc7f1bec-dbbc-4be8-a439-10133a9bae18"); 
            _grpcChannel = GrpcChannel.ForAddress(_serverAddress);
            _ingestClient = new Ingest.IngestClient(_grpcChannel);
            _egressClient = new Egress.EgressClient(_grpcChannel);

        }

        public async Task<bool> GetDataPackagesStream()
        {
            GetDataPackage_Request request = new GetDataPackage_Request(){
                TagName = "SomeTagName",
                TrackingGuid = Guid.NewGuid().ToString(),
            };

            CancellationToken canellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
            
            using var dataPackageStream = _egressClient.GetDataPackagesStream(request);
            try
            {
                await foreach (var dataPackage in dataPackageStream.ResponseStream.ReadAllAsync(canellationToken))
                {
                    string msg; 
                    if (dataPackage.IsSuccess) {
                        msg = $"Data Package arrived: {dataPackage.TrackingGuid}"; 
                    } else {
                        msg = $"Error in data stream: {dataPackage.LastError}";
                        return false; 
                    }
                    _logger.LogTrace(msg);
                    Console.WriteLine(msg); 
                }
            }
            catch (RpcException exc)
            {
                string msg = $"Error during datapackage streaming: {exc.Message}";
                _logger.LogTrace(msg);
                Console.WriteLine(msg);
                return false; 
            }

            return true; 
        }

        public async Task<bool> SendDataPackage()
        {
            SendDataPackage_Request request = new SendDataPackage_Request() {
                TrackingGuid = Guid.NewGuid().ToString(),
                TelemetryGuid = _appGuid.ToString(),
                TagName = "DateTimeTicks",
                TagValue = DateTime.UtcNow.Ticks.ToString()
            };
 
            try 
            {
                SendDataPackage_Response response = await _ingestClient.SendDataPackageAsync(request);
                string msg = ""; 
                if (response.IsSuccess) {
                    msg = $"Data package send: {response.TrackingGuid}";
                    _logger.LogTrace(msg);
                } else {
                    msg = $"Error data package send: {response.LastError}";
                    _logger.LogError(msg);
                }
                return response.IsSuccess; 
            }
            catch (RpcException rpcException)
            {
                string msg = rpcException.Message; 
                _logger.LogCritical(msg);
                Console.WriteLine(msg);

                return false;   
            }
             
        }

        public async Task<bool> SendDataPackagesStream()
        {
            using var sendDataPackagesStream = _ingestClient.SendDataPackagesStream(); 

            string msg; 
            SendDataPackage_Request request; 
            using var sendDataStream = _ingestClient.SendDataPackagesStream(); 
            for (int i=0; i<10; i++)
            {
                request = new SendDataPackage_Request(){
                    TrackingGuid = Guid.NewGuid().ToString(),
                    TelemetryGuid = _appGuid.ToString(),
                    TagName = "DateTimeTicks",
                    TagValue = DateTime.UtcNow.Ticks.ToString()
                };
                msg = $"DataPacke to be send: {request.TrackingGuid}";
                _logger.LogTrace(msg);
                Console.WriteLine(msg);

                try {
                    await sendDataStream.RequestStream.WriteAsync(request);
                } catch (RpcException) {
                    return false; 
                }
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            await sendDataPackagesStream.RequestStream.CompleteAsync();
            SendDataPackage_Response response = await sendDataPackagesStream.ResponseAsync;

            msg = $"Send data packages count: {response.DataPackageCount}";
            _logger.LogTrace(msg);
            Console.WriteLine(msg);

            return response.IsSuccess; 
        }
    }
}