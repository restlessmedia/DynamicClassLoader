using System.Collections.Generic;

namespace DynamicClassLoader
{
  public interface IClassReader
  {
    IEnumerable<ClassDefinition> GetDefinitions();
  }
}