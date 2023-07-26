using System.Reflection;
using System.Threading.Tasks;

namespace BLIT.Helpers;
static class InvokeHelper
{
    internal static async Task<object?> InvokeAsync(this MethodInfo @this, object obj, params object?[] parameters)
    {
        dynamic? awaitable = @this.Invoke(obj, parameters);
        if (awaitable == null) return null;
        await awaitable;
        return awaitable.GetAwaiter().GetResult();
    }
}
