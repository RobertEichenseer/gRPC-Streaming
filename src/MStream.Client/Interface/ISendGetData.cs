using System;
namespace MStream.Client
{
    interface ISendGetData
    {
        Task<bool> SendDataPackage();
        Task<bool> SendDataPackagesStream();
        Task<bool> GetDataPackagesStream();
    }
}