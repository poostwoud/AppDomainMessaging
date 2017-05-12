using System;
using OperationMessaging;

namespace AppDomainMessaging
{
    /// <summary>
    /// https://msdn.microsoft.com/en-us/library/system.net.http.httpresponsemessage(v=vs.118).aspx
    /// </summary>
    [Serializable]
    public sealed class AppDomainResponseMessage
    {
        public AppDomainContent Content { get; set; }

        public bool IsSuccess => StatusCode == AppDomainStatusCodes.OK
                                 || StatusCode == AppDomainStatusCodes.Created
                                 || StatusCode == AppDomainStatusCodes.NoContent;

        public AppDomainStatusCodes StatusCode { get; set; }

        public static AppDomainResponseMessage FromOperationResult(OperationResponse response)
        {
            return new AppDomainResponseMessage
            {
                Content = new AppDomainContent {Content = response.Result.ToString()},
                StatusCode = response.Succes ? AppDomainStatusCodes.OK : AppDomainStatusCodes.InternalServerError
            };
        }
    }
}