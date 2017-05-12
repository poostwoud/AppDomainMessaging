using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AppDomainMessaging
{
    [Serializable]
    public sealed class AppDomainNameServer
    {
        private readonly Dictionary<string, AppDomainNameRecord> _records;

        private readonly Dictionary<string, AppDomainCacheRecord> _cache;

        private readonly string _workspacePath;

        public AppDomainNameServer()
        {
            _records = new Dictionary<string, AppDomainNameRecord>(StringComparer.InvariantCultureIgnoreCase);
            _cache = new Dictionary<string, AppDomainCacheRecord>(StringComparer.InvariantCultureIgnoreCase);
            //***** TODO:Configure;
            _workspacePath = AppDomain.CurrentDomain.BaseDirectory;
        }

        public IReadOnlyList<AppDomainNameRecord> Records => _records.Values.ToList().AsReadOnly();

        public void Add(AppDomainNameRecord nameRecord)
        {
            _records.Add(nameRecord.Name, nameRecord);
        }

        public void Add(string name, string location)
        {
            Add(new AppDomainNameRecord(name, location));
        }

        internal AppDomainProxy Resolve(string name, out Exception exception)
        {
            //*****
            exception = null;

            //***** Return if exists and not expired. Else flush and reload;
            if (_cache.ContainsKey(name))
            {
                if (!_cache[name].Expired) return _cache[name].Proxy;
                Flush(name);
            }

            //***** TODO:Custom exception;
            if (!_records.ContainsKey(name))
            {
                exception = new Exception($"Unknown host: {name}");
                return null;
            }
            var nameRecord = _records[name];

            //***** TODO:Set workspace as application base;
            //***** TODO:Security;
            var appDomainSetup = new AppDomainSetup();
            var appDomain = AppDomain.CreateDomain(nameRecord.Name, null, appDomainSetup);
            var appDomainProxyType = typeof(AppDomainProxy);
            var appDomainProxy = (AppDomainProxy) appDomain.CreateInstanceAndUnwrap(appDomainProxyType.Assembly.FullName, appDomainProxyType.FullName);

            //***** Move to workspace;
            if (!File.Exists(nameRecord.Location))
                throw new FileNotFoundException();

            //***** TODO:Use last modified time;
            var workspaceLocation = $@"{_workspacePath}{Path.GetFileName(nameRecord.Location)}.{DateTime.Now.Ticks}";
            File.Copy(nameRecord.Location, workspaceLocation);

            //*****
            appDomainProxy.LoadFrom(workspaceLocation);
            _cache.Add(nameRecord.Name, new AppDomainCacheRecord(appDomain, appDomainProxy));

            //*****
            return _cache[name].Proxy;
        }

        internal void Flush(string name = null)
        {
            //*****
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (!_cache.ContainsKey(name)) throw new Exception();
                var item = _cache[name];
                AppDomain.Unload(item.Domain);
                _cache.Remove(name);
                return;
            }

            //*****
            foreach (var item in _cache)
            {
                /*System.Diagnostics.Debug.WriteLine(item.Value.Domain.Id);
                System.Diagnostics.Debug.WriteLine(item.Value.Domain.FriendlyName);
                System.Diagnostics.Debug.WriteLine(item.Value.Domain.IsDefaultAppDomain());
                System.Diagnostics.Debug.WriteLine(item.Value.Domain.IsFinalizingForUnload());*/
                AppDomain.Unload(item.Value.Domain);
            }

            //*****
            _cache.Clear();
            _records.Clear();
        }
    }
}