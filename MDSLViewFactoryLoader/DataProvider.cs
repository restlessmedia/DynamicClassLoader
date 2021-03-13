using MDSL.DataLayer.Repositories;

namespace MDSL.DataLayer
{
  public class DataProvider<TModel> : IDataProvider<TModel>
  {
    public static DataProvider<TModel> Create(IDatabase database, ProviderViewModel view)
    {
      return new DataProvider<TModel>();
    }
  }
}