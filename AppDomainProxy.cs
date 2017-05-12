using System;
using System.Reflection;
using OperationMessaging;

namespace AppDomainMessaging
{
    //***** http://stackoverflow.com/questions/17225276/create-custom-appdomain-and-add-assemblies-to-it
    public class AppDomainProxy : MarshalByRefObject
    {
        private string _path;
        private Assembly _serviceAssembly;
        private Type _serviceType;

        public void Load(string path)
        {
            ValidatePath(path);

            _path = path;

            _serviceAssembly = Assembly.Load(_path);
        }

        public void LoadFrom(string path)
        {
            ValidatePath(path);

            _path = path;

            _serviceAssembly = Assembly.LoadFrom(_path);
        }

        private void ValidatePath(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (!System.IO.File.Exists(path))
                throw new ArgumentException($"path \"{path}\" does not exist");
        }

        private static bool DerivesFromClass(Type currentType, string baseTypeName)
        {
            //*****
            if (currentType == null) throw new ArgumentNullException(nameof(currentType));
            if (string.IsNullOrWhiteSpace(baseTypeName)) throw new ArgumentNullException(nameof(baseTypeName));

            //*****
            var type = currentType;
            while (type != null && type.BaseType != typeof(object))
            {
                if (type.BaseType != null && type.BaseType.FullName == baseTypeName )
                    return true;
                type = type.BaseType;
            }

            //*****
            return false;
        }

        public OperationResponse Execute(OperationRequest request)
        {
            var types = _serviceAssembly.GetTypes();
            foreach (var type in types)
                if (DerivesFromClass(type, "OperationMessaging.OperationService"))
                {
                    _serviceType = type;
                    break;
                }

            //*****
            if (_serviceType == null)
                return new OperationResponse {Succes = false, NonSuccessMessage = "No type", Result = "No type" };

            //*****
            var service = (IOperationService) _serviceAssembly.CreateInstance(_serviceType.FullName);
            return service == null ? new OperationResponse { Succes = false, NonSuccessMessage = "No type", Result = "No type" } : service.Execute(request);
        }
    }
}
