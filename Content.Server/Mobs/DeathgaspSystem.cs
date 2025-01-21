using Content.Server.Chat.Systems;
using Content.Server.Speech.Muting;
using Content.Shared.Mobs;
using Content.Shared.Speech.Muting;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server.Mobs;

/// <see cref="DeathgaspComponent"/>
public sealed class DeathgaspSystem: EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private static readonly SoundSpecifier DeathSounds = new SoundCollectionSpecifier("DeathSounds");
    //private static readonly SoundSpecifier HeartSounds = new SoundCollectionSpecifier("HeartSounds");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DeathgaspComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(EntityUid uid, DeathgaspComponent component, MobStateChangedEvent args)
    {
        /*//^.^
        switch (args.NewMobState)
        {
            case MobState.Invalid:
                StopPlayingStream(uid);
                break;
            case MobState.Alive:
                StopPlayingStream(uid);
                break;
            case MobState.Critical:
                PlayPlayingStream(uid);
                break;
            case MobState.Dead:
                StopPlayingStream(uid);
                var deathGaspMessage = SelectRandomDeathGaspMessage();
                var localizedMessage = LocalizeDeathGaspMessage(deathGaspMessage);
                SendDeathGaspMessage(uid, localizedMessage);
                PlayDeathSound(uid);
                break;
        }*/

        // don't deathgasp if they arent going straight from crit to dead
        if (component.NeedsCritical && args.OldMobState != MobState.Critical
            || args.NewMobState != MobState.Dead)
            return;

        Deathgasp(uid, component);
    }

    /// <summary>
    ///     Causes an entity to perform their deathgasp emote, if they have one.
    /// </summary>
    public bool Deathgasp(EntityUid uid, DeathgaspComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return false;

        if (HasComp<MutedComponent>(uid))
            return false;

        _chat.TryEmoteWithChat(uid, component.Prototype, ignoreActionBlocker: true);

        return true;
    }
    /*
    private void PlayPlayingStream(EntityUid uid)
    {
        //_audio.PlayEntity(HeartSounds, uid, uid, AudioParams.Default.WithLoop(true));
    }

    private void StopPlayingStream(EntityUid uid)
    {
        if (_playingStreams.TryGetValue(uid, out var currentStream))
        {
            currentStream.Stop();
            _playingStreams.Remove(uid);
        }
    }
    */
}
