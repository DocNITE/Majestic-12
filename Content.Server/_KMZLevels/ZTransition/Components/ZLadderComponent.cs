namespace Content.Server._KMZLevels.ZTransition;

[RegisterComponent]
public sealed partial class ZLadderComponent : Component
{
    /// Warp destination unique identifier.
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("location")]
    public string? Location { get; set; }
}
