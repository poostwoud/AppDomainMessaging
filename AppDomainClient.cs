using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using OperationMessaging;

namespace AppDomainMessaging
{
    public sealed class AppDomainClient : IDisposable
    {
        public AppDomainClient()
        {
            NameServer = new AppDomainNameServer();
            IsDisposed = false;
        }

        ~AppDomainClient()
        {
            OnDispose(false);
            GC.SuppressFinalize(this);
        }

        public bool IsDisposed { get; private set; }

        public AppDomainNameServer NameServer { get; }

        public AppDomainResponseMessage Get(string requestUri)
        {
            //***** TODO:Build request message;
            var uri = new Uri(requestUri);

            //*****
            var protocol = uri.Scheme;
            if (!protocol.Equals("adp")) throw new Exception($"Protocol not supported: {protocol}");

            //***** 
            var host = uri.Host;
            var path = uri.AbsolutePath;

            //***** Local;
            if (host.ToLower() == "local")
            {
                if (path.Equals("/dns"))
                {
                    return new AppDomainResponseMessage
                    {
                        StatusCode = AppDomainStatusCodes.OK,
                        Content = new AppDomainContent
                        {
                            Content = JsonConvert.SerializeObject(NameServer.Records, Formatting.Indented)
                        }
                    };
                }
            }

            //***** Resolve domain;
            Exception exception;
            var proxy = NameServer.Resolve(host, out exception);
            if (proxy == null || exception != null)
                return new AppDomainResponseMessage
                {
                    StatusCode = AppDomainStatusCodes.NotFound,
                    Content = new AppDomainContent
                    {
                        Content = exception.Message
                    }
                };

            //***** Create operation request;
            var pathElements = path.Split(new[] {@"/"}, StringSplitOptions.RemoveEmptyEntries);

            var request = new OperationRequest
            {
                ClassName = pathElements[0],
                MethodName = pathElements[1]
            };

            var parameters = new List<object>();
            for (var idx = 2; idx < pathElements.Length; idx++)
                parameters.Add(pathElements[idx]);
            request.Parameters = parameters.ToArray();

            //*****
            try
            {
                return AppDomainResponseMessage.FromOperationResult(proxy.Execute(request));
            }
            catch (Exception ex)
            {
                return new AppDomainResponseMessage
                {
                    StatusCode = AppDomainStatusCodes.OK,
                    Content = new AppDomainContent
                    {
                        Content = ex.Message
                    }
                };
            }
        }

        public AppDomainResponseMessage Post(string requestUri, AppDomainContent content)
        {
            //***** TODO:Build request message;
            var uri = new Uri(requestUri);

            //*****
            var protocol = uri.Scheme;
            if (!protocol.Equals("adp")) throw new Exception($"Protocol not supported: {protocol}");

            //*****
            var host = uri.Host;
            var path = uri.AbsolutePath;

            //***** Local;
            if (host.ToLower() == "local")
            {
                if (path.Equals("/dns"))
                {
                    var addDnsCommand = JsonConvert.DeserializeObject<AddDnsCommand>(content.Content);
                    NameServer.Add(new AppDomainNameRecord(addDnsCommand.Name, addDnsCommand.Location));
                    return new AppDomainResponseMessage
                    {
                        StatusCode = AppDomainStatusCodes.Created
                    };
                }
            }

            //***** Resolve domain;
            Exception exception;
            var proxy = NameServer.Resolve(host, out exception);
            if (proxy == null || exception != null)
                return new AppDomainResponseMessage
                {
                    StatusCode = AppDomainStatusCodes.NotFound,
                    Content = new AppDomainContent
                    {
                        Content = exception.Message
                    }
                };

            //***** Create operation request;
            var pathElements = path.Split(new[] { @"/" }, StringSplitOptions.RemoveEmptyEntries);

            var request = new OperationRequest
            {
                ClassName = pathElements[0],
                MethodName = pathElements[1]
            };

            var parameters = new List<object>();
            for (var idx = 2; idx < pathElements.Length; idx++)
                parameters.Add(pathElements[idx]);
            request.Parameters = parameters.ToArray();

            //*****
            return AppDomainResponseMessage.FromOperationResult(proxy.Execute(request));
        }

        public AppDomainResponseMessage Delete(string requestUri)
        {
            //***** TODO:Build request message;
            var uri = new Uri(requestUri);

            //*****
            var protocol = uri.Scheme;
            if (!protocol.Equals("adp")) throw new Exception($"Protocol not supported: {protocol}");

            //*****
            var host = uri.Host;
            var path = uri.AbsolutePath;

            //***** Local;
            if (host.ToLower() == "local")
            {
                //***** TODO:Flush specific;
                if (path.Equals("/dns"))
                {
                    NameServer.Flush();
                    return new AppDomainResponseMessage
                    {
                        StatusCode = AppDomainStatusCodes.OK
                    };
                }
            }
            
            //***** Resolve domain;
            Exception exception;
            var proxy = NameServer.Resolve(host, out exception);
            if (proxy == null || exception != null)
                return new AppDomainResponseMessage
                {
                    StatusCode = AppDomainStatusCodes.NotFound,
                    Content = new AppDomainContent
                    {
                        Content = exception.Message
                    }
                };

            //***** Create operation request;
            var pathElements = path.Split(new[] { @"/" }, StringSplitOptions.RemoveEmptyEntries);

            var request = new OperationRequest
            {
                ClassName = pathElements[0],
                MethodName = pathElements[1]
            };

            var parameters = new List<object>();
            for (var idx = 2; idx < pathElements.Length; idx++)
                parameters.Add(pathElements[idx]);
            request.Parameters = parameters.ToArray();

            //*****
            return AppDomainResponseMessage.FromOperationResult(proxy.Execute(request));
        }

        public void Dispose()
        {
            OnDispose(true);
        }

        /// <summary>
        /// Should be called when the object is being disposed.
        /// </summary>
        /// <param name="disposing">
        /// Was Dispose() called or did we get here from the finalizer?
        /// </param>
        private void OnDispose(bool disposing)
        {
            //*****
            if (disposing)
            {
                if (!IsDisposed)
                {
                    NameServer.Flush();
                }
            }

            //*****
            IsDisposed = true;
        }
    }
}
