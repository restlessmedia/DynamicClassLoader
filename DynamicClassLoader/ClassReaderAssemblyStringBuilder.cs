using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace DynamicClassLoader
{
  internal class ClassReaderAssemblyStringBuilder : AssemblyStringBuilder, IAssemblyBuilder
  {
    public ClassReaderAssemblyStringBuilder(string name, IClassReader classReader)
      : base(name)
    {
      _classReader = classReader;
    }

    public override Assembly Build()
    {
      IEnumerable<ClassDefinition> classDefinitions = _classReader.GetDefinitions();

      foreach (ClassDefinition classDefinition in classDefinitions)
      {
        Add(GetClassCode(classDefinition));
      }
      
      AddReference<ReadOnlyAttribute>();

      foreach (Type type in classDefinitions
        .SelectMany(classDefinition => classDefinition.Properties)
        .Select(propertyDefinition => propertyDefinition.GetPropertyType())) 
      {
        AddReference(type);
      }

      return base.Build();
    }

    private string GetClassCode(ClassDefinition classDefinition)
    {
      return $"{GetUsingsCode(classDefinition)} namespace {Name} {{ public class {classDefinition.Name} {{ {GetPropertyCode(classDefinition)} }} }}";
    }

    private string GetUsingsCode(ClassDefinition classDefinition)
    {
      List<string> namespaces = new List<string>(classDefinition.Properties.Select(column => column.GetPropertyType().Namespace));

      // for attributes
      namespaces.Add(typeof(ReadOnlyAttribute).Namespace);

      return string.Join(Environment.NewLine, namespaces.Distinct().Select(ns => $"using {ns};"));
    }

    private string GetPropertyCode(ClassDefinition classDefinition)
    {
      return string.Join(Environment.NewLine, classDefinition.Properties.Select(column => $"{GetPropertyAttributesCode(column)} public {column.GetPropertyType().Name} {column.Name} {{ get; set; }}"));
    }

    private string GetPropertyAttributesCode(PropertyDefinition propertyDefinition)
    {
      if (propertyDefinition.IsReadOnly)
      {
        return $"[ReadOnly(true)]";
      }

      return null;
    }

    private readonly IClassReader _classReader;
  }
}