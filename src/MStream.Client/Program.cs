using System.CommandLine;
using System.CommandLine.Invocation;
using MStream.Client;

IHost consoleHost = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddTransient<Main>(); 
        services.AddSingleton<ISendGetData, SendGetDataPackage_DataSimulator>();
    })
    .Build();


Main main = consoleHost.Services.GetRequiredService<Main>(); 
main.ExecuteAsync(args); 

class Main
{
    private readonly ILogger<Main> _logger;
    private readonly ISendGetData _sendGetData;

    public Main(ILogger<Main> logger, ISendGetData sendGetData)
    {
        _logger = logger;
        _sendGetData = sendGetData; 
    }
    public void ExecuteAsync(string[] args, CancellationToken stoppingToken = default)
    {
        RootCommand rootCommand = new RootCommand("MStream Command Line Interface");
        DefineCommandLine(rootCommand, args);
        rootCommand.Invoke(args);
    }

    private void DefineCommandLine(RootCommand rootCommand, string[] args)
    {
        //Send Single Data Package
        Command sendDataCommand = new Command("senddata", "Send single data package"){};
        sendDataCommand.Handler = CommandHandler.Create(sendDataPackageCommand);
        rootCommand.Add(sendDataCommand);
        
        //Send Data Package Stream
        Command sendStreamDataCommand = new Command("sendstream", "Send data packages as stream,"){};
        sendStreamDataCommand.Handler = CommandHandler.Create(sendStreamDataPackagesCommand);
        rootCommand.Add(sendStreamDataCommand);

        //Get Data Package Stream
        Command getStreamDataCommand = new Command("getstream", "Get data packages as stream"){};
        getStreamDataCommand.Handler = CommandHandler.Create(getStreamDataPackagesCommandAsync);
        rootCommand.Add(getStreamDataCommand);
    }

    private async Task getStreamDataPackagesCommandAsync()
    {
        bool result = await _sendGetData.GetDataPackagesStream();
    }

    private async Task sendStreamDataPackagesCommand()
    {
        bool result = await _sendGetData.SendDataPackagesStream();
    }

    private async Task sendDataPackageCommand()
    {
        bool result = await _sendGetData.SendDataPackage();
    }
}
