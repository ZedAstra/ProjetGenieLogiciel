using Microsoft.AspNetCore.Builder;

namespace Backend.Modules
{
    public interface IModule
    {
        void Setup(WebApplication app);
    }
}
