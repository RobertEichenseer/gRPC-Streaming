using Grpc.Core;
using MStream.Srv;
using MStream.ServiceDefinition;

namespace MStream.Srv.IngestDataService; 

public class IngestDataService : Ingest.IngestBase
{
    private readonly ILogger<IngestDataService> _logger;
 
    public IngestDataService(ILogger<IngestDataService> logger)
    {
        _logger = logger;
    }

    public override async Task<SendDataPackage_Response> SendDataPackage (SendDataPackage_Request request, ServerCallContext context)
    {
        string msg = $"Data Package arrived: {request.TrackingGuid}";
        await Task.Run( () => _logger.LogTrace(msg)); 

        return new SendDataPackage_Response(){
            IsSuccess = true
        }; 
    }

    //External Counter Storage needed - static class variable not appropriate - just for demo purpose
    private static int _packageCounter = 0;  
    public override async Task<SendDataPackage_Response> SendDataPackagesStream (IAsyncStreamReader<SendDataPackage_Request> request, ServerCallContext context) {
      
        while (await request.MoveNext())
        {
            SendDataPackage_Request dataPackage = request.Current;
            string msg = $"Data Package: {dataPackage.TelemetryGuid} arrived!";
            _logger.LogTrace(msg);
            _packageCounter ++; 
        }
 
        return new SendDataPackage_Response(){
            IsSuccess = true,
            LastError = "",
            DataPackageCount = _packageCounter,
        };         
    }
}
