using Robust.Shared.Audio;

namespace Content.Server._Finster.Heartbeat;

[RegisterComponent]
public sealed partial class HeartbeatComponent : Component
{
    [DataField]
    public SoundSpecifier HeartbeatSound = new SoundPathSpecifier("/Audio/_Finster/Effects/Heart/heart.ogg");

    [DataField]
    public bool Enabled = true;

    public EntityUid? AudioStream;
}
