using Godot;
using System.Diagnostics.CodeAnalysis;

namespace BLIT.scripts.Common;
public static class Check {
    public static bool IsGodotSafe([NotNullWhen(true)] GodotObject? other) {
        return other != null && GodotObject.IsInstanceValid(other);
    }
}
