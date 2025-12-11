using System.Threading.Tasks;

namespace Bulky.DataAccess.Initializer;

public interface IDbInitializer
{
    Task InitializeAsync();
}
