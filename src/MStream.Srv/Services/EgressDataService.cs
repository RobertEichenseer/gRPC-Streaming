using Grpc.Core;
using MStream.ServiceDefinition;
using MStream.Srv;

namespace MStream.Srv.IngestDataService; 

public class EgressDataService : Egress.EgressBase 
{
    private readonly ILogger<EgressDataService> _logger;

    public EgressDataService(ILogger<EgressDataService> logger)
    {
        _logger = logger; 
    }

    public override async Task GetDataPackagesStream(GetDataPackage_Request request, IServerStreamWriter<GetDataPackage_Response> responseStream, ServerCallContext context )
    {
        while (!context.CancellationToken.IsCancellationRequested)
        {
            GetDataPackage_Response response = new GetDataPackage_Response(){
                IsSuccess = true,
                LastError = "",
                TrackingGuid = request.TrackingGuid, 
                TagName = request.TagName, 
                TagValue = DateTime.UtcNow.Ticks.ToString()
            };
            await responseStream.WriteAsync(response);
            await Task.Delay(TimeSpan.FromSeconds(1));
        }  
    }

}
