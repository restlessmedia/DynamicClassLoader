using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DynamicClassLoader
{
  internal class AssemblyStringBuilder : IAssemblyBuilder
  {
    public AssemblyStringBuilder(string name)
      : this(AppDomain.CurrentDomain, name) { }

    public AssemblyStringBuilder(AppDomain appDomain, string name)
    {
      _appDomain = appDomain;
      Name = name;
      _classCode = new List<string>();
      _references = new List<string>();
    }

    public void Add(string code)
    {
      _classCode.Add(code);
    }

    public void AddReference<T>()
    {
      AddReference(typeof(T));
    }

    public void AddReference(Type type)
    {
      AddReferences(type.Assembly.Location);
    }

    public void AddReferences(params string[] referenceLocations)
    {
      _references.AddRange(referenceLocations.Except(_references));
    }

    public virtual Assembly Build()
    {
      using (MemoryStream memoryStream = new MemoryStream())
      {
        // write IL code into memory
        EmitResult result = CompileToStream(memoryStream);

        if (!result.Success)
        {
          throw CreateFailureException(result);
        }
        else
        {
          // load the assembly
          memoryStream.Seek(0, SeekOrigin.Begin);
          return _appDomain.Load(memoryStream.ToArray());
        }
      }
    }

    public Assembly Build(string code, params string[] referenceLocations)
    {
      Add(code);
      AddReferences(referenceLocations);
      return Build();
    }

    private EmitResult CompileToStream(Stream stream)
    {
      // define source code, then parse it (to the type used for compilation)
      SyntaxTree[] syntaxTrees = _classCode.Select(code => CSharpSyntaxTree.ParseText(code)).ToArray();

      // analyse and generate IL code from syntax tree
      MetadataReference[] references = _references.Select(name => MetadataReference.CreateFromFile(name)).ToArray();
      CSharpCompilation compilation = CSharpCompilation.Create(Name, syntaxTrees, references, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

      return compilation.Emit(stream);
    }

    private AggregateException CreateFailureException(EmitResult result)
    {
      IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
      Exception[] exceptions = failures.Select(diagnostic => new Exception($"{diagnostic.Id}: {diagnostic.GetMessage()}")).ToArray();
      return new AggregateException(exceptions);
    }

    protected readonly string Name;

    private readonly AppDomain _appDomain;

    private readonly List<string> _classCode;

    private readonly List<string> _references;
  }
}