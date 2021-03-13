using System.Reflection;

namespace DynamicClassLoader
{
  public interface IAssemblyBuilder
  {
    Assembly Build();
  }
}