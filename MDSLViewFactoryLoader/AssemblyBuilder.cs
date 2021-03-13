using DynamicClassLoader;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace MDSLViewFactoryLoader
{
  public class AssemblyBuilder : AssemblyEmitBuilder
  {
    public AssemblyBuilder(IClassReader classReader)
      : base(AppDomain.CurrentDomain, "MDSL.DataLayer", classReader) { }

    public override Assembly Build()
    {
      return base.Build();
    }

    protected override TypeBuilder DefineType(string name)
    {
      return base.DefineType("ViewFactory");
    }
  }
}