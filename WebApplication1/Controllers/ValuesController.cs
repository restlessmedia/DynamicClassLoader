using DynamicClassLoader;
using System;
using System.Linq;
using System.Web.Http;

namespace WebApplication1.Controllers
{
  public class ValuesController : ApiController
  {
    public ValuesController()
    {
      _classLoaderFacade = new FileSystemDynamicLoaderFacade("dynamic_class_loader_example");
    }

    // GET api/values
    public object[] Get(string name, bool clearCache = true)
    {
      if (clearCache)
      {
        _classLoaderFacade.Clear();
      }

      Type type = _classLoaderFacade.GetByName(name);

      if (type != null)
      {
        return new object[] { new { type.FullName, properties = type.GetProperties().Select(prop => new { prop.Name, prop.PropertyType.FullName }) } };
      }

      return null;
    }

    private readonly FileSystemDynamicLoaderFacade _classLoaderFacade;
  }
}