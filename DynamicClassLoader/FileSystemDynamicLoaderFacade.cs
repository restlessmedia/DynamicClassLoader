using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DynamicClassLoader
{
  public class FileSystemDynamicLoaderFacade
  {
    public FileSystemDynamicLoaderFacade(string name)
    {
      _typeCache = new ReflectionTypeCacheReader();
      _name = name;
    }

    public Type GetByName(string name)
    {
      name = string.Concat(_name, ".", name);

      if (!_typeCache.TryGet(name, out Type value))
      {
        string path = Assembly.GetEntryAssembly().Location;
        Stream stream = File.OpenRead(path);
        IClassReader classReader = new StreamClassReader(stream);
        IAssemblyBuilder assemblyBuilder = new ClassReaderAssemblyStringBuilder(_name, classReader);
        assemblyBuilder.Build();
        _typeCache.TryGet(name, out value);
      }

      return value;
    }

    public void Clear()
    {

    }

    private readonly string _name;

    private readonly ICacheReader<Type> _typeCache;
  }
}