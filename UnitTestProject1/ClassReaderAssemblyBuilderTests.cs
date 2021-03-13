using DynamicClassLoader;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.Reflection;

namespace UnitTestProject1
{
  [TestClass]
  public class ClassReaderAssemblyBuilderTests
  {
    public ClassReaderAssemblyBuilderTests()
    {
      _fakeSchemaReader = A.Fake<IClassReader>();
      _instance = new ClassReaderAssemblyStringBuilder("test", _fakeSchemaReader);
    }

    [TestMethod]
    public void Adds_multiple_classes()
    {
      // set-up
      ClassDefinition[] classDefinitions = new ClassDefinition[]
      {
        new ClassDefinition
        {
          Name = "TestClassA",
          Properties = new PropertyDefinition[]
          {
            new PropertyDefinition
            {
              Name= "Name",
              TypeName = typeof(string).FullName
            }
          },
        },
        new ClassDefinition
        {
          Name = "TestClassB",
          Properties = new PropertyDefinition[]
          {
            new PropertyDefinition
            {
              Name= "CreatedDate",
              TypeName = typeof(DateTime).FullName
            }
          },
        },
      };
      A.CallTo(() => _fakeSchemaReader.GetDefinitions()).Returns(classDefinitions);

      // call
      Assembly assembly = _instance.Build();

      // assert
      Assert.AreEqual(assembly.GetType("test.TestClassA").GetProperty("Name").PropertyType, typeof(string));
      Assert.AreEqual(assembly.GetType("test.TestClassB").GetProperty("CreatedDate").PropertyType, typeof(DateTime));
    }

    [TestMethod]
    public void Adds_attributes()
    {
      ClassDefinition[] classDefinitions = new ClassDefinition[]
      {
        new ClassDefinition
        {
          Name = "TestClassA",
          Properties = new PropertyDefinition[]
          {
            new PropertyDefinition
            {
              Name= "Name",
              TypeName = typeof(string).FullName,
              IsReadOnly = true,
            }
          },
        },
      };
      A.CallTo(() => _fakeSchemaReader.GetDefinitions()).Returns(classDefinitions);

      // call
      Assembly assembly = _instance.Build();

      // assert
      Assert.IsTrue(assembly.GetType("test.TestClassA").GetProperty("Name").GetCustomAttribute<ReadOnlyAttribute>().IsReadOnly);
    }

    private readonly ClassReaderAssemblyStringBuilder _instance;

    private readonly IClassReader _fakeSchemaReader;
  }
}
