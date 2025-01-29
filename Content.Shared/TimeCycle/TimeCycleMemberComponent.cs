namespace Content.Shared.TimeCycle;

[RegisterComponent]
public sealed partial class TimeCycleMemberComponent : Component
{
    /// <summary>
    /// Link that component with <seealso cref="TimeCycleTrackerComponent"/>
    /// </summary>
    [DataField]
    public string? TrackerId;
}
