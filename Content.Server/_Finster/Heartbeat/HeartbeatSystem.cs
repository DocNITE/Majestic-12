using Content.Shared.Damage;
using Content.Shared.Mobs;
using Robust.Server.Audio;
using Robust.Shared.Audio;

namespace Content.Server._Finster.Heartbeat;

public sealed class HeartbeatSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeartbeatComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<HeartbeatComponent, DamageChangedEvent>(OnDamage);
    }

    private void OnMobStateChanged(Entity<HeartbeatComponent> ent, ref MobStateChangedEvent args)
    {
        if (!ent.Comp.Enabled)
            return;

        ent.Comp.AudioStream = args.NewMobState == MobState.Critical
            ? _audio.PlayEntity(ent.Comp.HeartbeatSound, ent, ent)?.Entity
            : _audio.Stop(ent.Comp.AudioStream);
    }

    private void OnDamage(Entity<HeartbeatComponent> ent, ref DamageChangedEvent args)
    {
        if (!ent.Comp.Enabled)
            return;

        if (!Exists(ent.Comp.AudioStream))
            return;

        var pitch = Math.Min(1, 100 / args.Damageable.TotalDamage.Float());

        _audio.Stop(ent.Comp.AudioStream);
        ent.Comp.AudioStream = _audio.PlayEntity(ent.Comp.HeartbeatSound, ent, ent, AudioParams.Default.WithPitchScale(pitch).WithLoop(true))?.Entity;
    }
}
