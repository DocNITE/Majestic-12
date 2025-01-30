using System.Threading;
using Robust.Shared.GameStates;

namespace Content.Shared._ERRORGATE.Hearing;

/// <summary>
/// Changes all incoming chat messages to DeafChatMessage.
/// Also block any sound sources.
/// Added by the DeafnessSystem on the HearingChangedEvent
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DeafComponent : Component
{
    [DataField]
    public string DeafChatMessage = "You can almost hear something...";

    [DataField]
    public bool Permanent = false;

    [DataField]
    public bool BlockSounds = true;

    [DataField]
    public float Duration = 0f; // In seconds

    public CancellationTokenSource? TokenSource;
}
