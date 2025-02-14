using System.Numerics;

namespace Content.Server._KMZLevels.ZTransition;

/// <summary>
/// Marks if entity is stairs, for moving between Z levels.
/// </summary>
[RegisterComponent]
public sealed partial class ZStairsComponent : Component
{
    [DataField]
    public Vector2 Adjust = Vector2.Zero;
}
