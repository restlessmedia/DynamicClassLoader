using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DynamicClassLoader
{
  public class Class1
  {
    public void ssdfsd()
    {
      WsdlClassParserFactory factory = new WsdlClassParserFactory();
      object parser = factory.CreateWsdlClassParser();
      factory.Unload();
    }

    public void dassd()
    {
      AppDomain appDomain = null;
      try
      {
        using (MemoryStream memoryStream = new MemoryStream())
        {
          // write IL code into memory


          //string path = @"C:myAssembly.dll";
          byte[] buffer; // = File.ReadAllBytes(path);

          // define source code, then parse it (to the type used for compilation)
          SyntaxTree[] syntaxTrees = new SyntaxTree[] { CSharpSyntaxTree.ParseText("public class fooBar {}") };

          // analyse and generate IL code from syntax tree
          MetadataReference[] references = new MetadataReference[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) };
          CSharpCompilation compilation = CSharpCompilation.Create("Test2", syntaxTrees, references, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

          EmitResult result = compilation.Emit(memoryStream);

          memoryStream.Seek(0, SeekOrigin.Begin);
          buffer = memoryStream.ToArray();

          appDomain = AppDomain.CreateDomain("Test");
          Assembly assm = appDomain.Load(buffer);

          Type[] types = assm.GetTypes();
          foreach (Type type in types)
          {
            Console.WriteLine(type.FullName);
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
      finally
      {
        if (appDomain != null)
          AppDomain.Unload(appDomain);
      }
    }
  }

  [ClassInterface(ClassInterfaceType.AutoDual)]
  public class WsdlClassParserFactory : MarshalByRefObject
  {
    public AppDomain LocalAppDomain = null;
    public string ErrorMessage = string.Empty;

    /// <summary>
    /// Creates a new instance of the WsdlParser in a new AppDomain
    /// </summary>
    /// <returns></returns>        
    public object CreateWsdlClassParser()
    {
      CreateAppDomain(null);

      string AssemblyPath = Assembly.GetExecutingAssembly().Location;
      object parser = null;
      try
      {
        parser = (object)LocalAppDomain.CreateInstanceFrom(AssemblyPath,
                                          typeof(object).FullName).Unwrap();
      }
      catch (Exception ex)
      {
        ErrorMessage = ex.Message;
      }
      return parser;
    }

    public bool CreateAppDomain(string appDomain)
    {
      if (string.IsNullOrEmpty(appDomain))
        appDomain = "wsdlparser" + Guid.NewGuid().ToString().GetHashCode().ToString("x");

      AppDomainSetup domainSetup = new AppDomainSetup();
      domainSetup.ApplicationName = appDomain;

      // *** Point at current directory
      domainSetup.ApplicationBase = Environment.CurrentDirectory;   // AppDomain.CurrentDomain.BaseDirectory;                 

      LocalAppDomain = AppDomain.CreateDomain(appDomain, null, domainSetup);

      // *** Need a custom resolver so we can load assembly from non current path
      AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

      return true;
    }

    Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
      try
      {
        Assembly assembly = Assembly.Load(args.Name);
        if (assembly != null)
          return assembly;
      }
      catch
      { // ignore load error
      }

      // *** Try to load by filename - split out the filename of the full assembly name
      // *** and append the base path of the original assembly (ie. look in the same dir)
      // *** NOTE: this doesn't account for special search paths but then that never
      //           worked before either.
      string[] Parts = args.Name.Split(',');
      string File = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + Parts[0].Trim() + ".dll";

      return Assembly.LoadFrom(File);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Unload()
    {
      if (LocalAppDomain != null)
      {
        AppDomain.Unload(LocalAppDomain);
        LocalAppDomain = null;
      }
    }
  }
}
