using System;

namespace AppDomainMessaging
{
    /// <summary>
    /// https://msdn.microsoft.com/en-us/library/system.net.http.httpcontent(v=vs.118).aspx
    /// </summary>
    [Serializable]
    public sealed class AppDomainContent
    {
        public string Content { get; set; }
    }
}