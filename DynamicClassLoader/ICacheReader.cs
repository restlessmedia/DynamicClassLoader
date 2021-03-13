using System;

namespace DynamicClassLoader
{
  public interface ICacheReader<T>
  {
    bool TryGet(string name, out T value);
  }
}