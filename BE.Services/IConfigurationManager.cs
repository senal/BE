using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.WindowsRuntime;

namespace BE.Services
{
    public interface IConfigurationManager
    {
        T Read<T>(string key);
        //bool Read(string key);
    }
}
