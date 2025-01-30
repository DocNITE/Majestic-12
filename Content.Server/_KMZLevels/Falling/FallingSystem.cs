using Content.KayMisaZlevels.Shared.Systems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Administration.Logs;
using Content.Shared.Damage;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using JetBrains.Annotations;

namespace Content.Server._KMZLevels.Falling;

[UsedImplicitly]
public sealed class FallingSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly DamageableSystem _damSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FallingComponent, ZLevelDroppedEvent>(OnDropped);
    }

    private void OnDropped(Entity<FallingComponent> ent, ref ZLevelDroppedEvent args)
    {
        _stun.TryParalyze(ent, TimeSpan.FromSeconds(ent.Comp.ParalyzeTime), true);
        if (!ent.Comp.IgnoreDamage)
        {
            _damSystem.TryChangeDamage(ent, ent.Comp.BaseDamage * args.Distance, ignoreResistances: true, targetPart: TargetBodyPart.LeftLeg);
            _damSystem.TryChangeDamage(ent, ent.Comp.BaseDamage * args.Distance, ignoreResistances: true, targetPart: TargetBodyPart.RightLeg);
        }
    }
}
