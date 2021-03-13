using DynamicClassLoader;
using FakeItEasy;
using MDSLViewFactoryLoader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnitTestProject1.MDSLViewFactoryLoader
{
  [TestClass]
  public class AssemblyBuilderTests
  {
    public AssemblyBuilderTests()
    {
      _classReader = A.Fake<IClassReader>();
      _assemblyBuilder = new AssemblyBuilder(_classReader);
    }

    [TestMethod]
    public void dynamic_test()
    {
      // set-up
      IEnumerable<ClassDefinition> classDefinitions = new List<ClassDefinition>
      {
        new ClassDefinition
        {
          Name = "AccessCctInterface",
          Properties = new List<PropertyDefinition>
          {
            new PropertyDefinition
            {
              Name = "acccctint",
              TypeName = typeof(int).FullName,
              IsReadOnly = true,
            },
            new PropertyDefinition
            {
              Name = "acccctintname",
              TypeName = typeof(string).FullName,
              IsReadOnly = false,
            }
          }
        }
      };

      A.CallTo(() => _classReader.GetDefinitions()).Returns(classDefinitions);

      Assembly assembly = _assemblyBuilder.Build();

      Type type = assembly.GetTypes().FirstOrDefault(x => x.Name == "AccessCctInterface");
    }

    private readonly IClassReader _classReader;

    private readonly AssemblyBuilder _assemblyBuilder;
  }
}
