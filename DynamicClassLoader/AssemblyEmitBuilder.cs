using System;
using System.Reflection;
using System.Reflection.Emit;

namespace DynamicClassLoader
{
  [Serializable]
  public class AssemblyEmitBuilder : IAssemblyBuilder
  {
    public AssemblyEmitBuilder(AppDomain appDomain, string assemblyName, IClassReader classReader)
    {
      AssemblyBuilder assemblyBuilder = appDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
      ModuleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
      _classReader = classReader;
    }

    public virtual Assembly Build()
    {
      foreach (ClassDefinition classDefinition in _classReader.GetDefinitions())
      {
        TypeBuilder typeBuilder = DefineType(classDefinition.Name);

        foreach (PropertyDefinition property in classDefinition.Properties)
        {
          CreateProperty(typeBuilder, property.Name, property.GetPropertyType());
        }

        typeBuilder.CreateType();
      }

      return ModuleBuilder.Assembly;
    }

    protected virtual TypeBuilder DefineType(string name)
    {
      return ModuleBuilder.DefineType(name,
              TypeAttributes.Public |
              TypeAttributes.Class |
              TypeAttributes.AutoClass |
              TypeAttributes.AnsiClass |
              TypeAttributes.BeforeFieldInit |
              TypeAttributes.AutoLayout,
              null);
    }

    private void CreateProperty(TypeBuilder typeBuilder, string name, Type type)
    {
      FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + name, type, FieldAttributes.Private);
      PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(name, PropertyAttributes.HasDefault, type, null);
      MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod("get_" + name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, type, Type.EmptyTypes);
      ILGenerator getIl = getPropMthdBldr.GetILGenerator();

      getIl.Emit(OpCodes.Ldarg_0);
      getIl.Emit(OpCodes.Ldfld, fieldBuilder);
      getIl.Emit(OpCodes.Ret);

      MethodBuilder setPropMthdBldr =
          typeBuilder.DefineMethod("set_" + name,
            MethodAttributes.Public |
            MethodAttributes.SpecialName |
            MethodAttributes.HideBySig,
            null, new[] { type });

      ILGenerator setIl = setPropMthdBldr.GetILGenerator();
      Label modifyProperty = setIl.DefineLabel();
      Label exitSet = setIl.DefineLabel();

      setIl.MarkLabel(modifyProperty);
      setIl.Emit(OpCodes.Ldarg_0);
      setIl.Emit(OpCodes.Ldarg_1);
      setIl.Emit(OpCodes.Stfld, fieldBuilder);

      setIl.Emit(OpCodes.Nop);
      setIl.MarkLabel(exitSet);
      setIl.Emit(OpCodes.Ret);

      propertyBuilder.SetGetMethod(getPropMthdBldr);
      propertyBuilder.SetSetMethod(setPropMthdBldr);
    }

    protected readonly ModuleBuilder ModuleBuilder;

    private readonly IClassReader _classReader;
  }
}