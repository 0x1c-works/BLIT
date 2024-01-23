using System.Reflection;
using System.Threading.Tasks;

namespace BLIT.scripts.Common;
public static class ReflectionExtensions {
    internal static async Task<object?> InvokeAsync(this MethodInfo @this, object obj, params object?[] parameters) {
        dynamic? awaitable = @this.Invoke(obj, parameters);
        if (awaitable == null) return null;
        await awaitable;
        return awaitable.GetAwaiter().GetResult();
    }
}
