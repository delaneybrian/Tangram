
namespace Tangram
{
    public interface ISerializer
    {
        string Serialize<T>(T obj);

        T Deserialize<T>(string data) where T : class;
    }
}
