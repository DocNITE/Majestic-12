using Content.Shared.Actions;
using Content.Shared.Hands.Components;
using Content.Shared.MouseRotator;
using Content.Shared.Movement.Components;
using Content.Shared.Standing;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._White.Intent;

public abstract class SharedIntentSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<IntentComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<IntentComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<IntentComponent, ToggleIntentEvent>(OnToggleIntent);

        //SubscribeLocalEvent<StandingStateComponent, DownedEvent>(OnDown);
        //SubscribeLocalEvent<StandingStateComponent, StoodEvent>(OnStood);
    }

    public virtual void OnStartup(EntityUid uid, IntentComponent component, ComponentStartup args)
    {
        if (!TryComp(uid, out ActionsComponent? comp))
            return;

        //_action.AddAction(uid, ref component.HelpActionEntity, component.HelpAction, component: comp);
        //_action.AddAction(uid, ref component.DisarmActionEntity, component.DisarmAction, component: comp);
        //_action.AddAction(uid, ref component.GrabActionEntity, component.GrabAction, component: comp);
        //_action.AddAction(uid, ref component.HarmActionEntity, component.HarmAction, component: comp);

        //UpdateActions(uid, component);
    }

    public virtual void OnShutdown(EntityUid uid, IntentComponent component, ComponentShutdown args)
    {
        if (!TryComp(uid, out ActionsComponent? comp))
            return;

        //_action.RemoveAction(uid, component.HelpActionEntity, comp);
        //_action.RemoveAction(uid, component.DisarmActionEntity, comp);
        //_action.RemoveAction(uid, component.GrabActionEntity, comp);
        //_action.RemoveAction(uid, component.HarmActionEntity, comp);
    }

    private void OnDown(EntityUid uid, StandingStateComponent comp, DownedEvent args)
    {
        //UpdateCollisionCollide(uid);
    }

    public void OnStood(EntityUid uid, StandingStateComponent comp, StoodEvent args)
    {
        //UpdateCollisionCollide(uid);
    }

    private void OnToggleIntent(EntityUid uid, IntentComponent component, ToggleIntentEvent args)
    {
        if (component.Intent == args.Type)
            return;

        args.Handled = true;

        component.Intent = args.Type;
        Dirty(uid, component);

        // Change bodyType if we switch from HELP
        //UpdateCollisionCollide(uid);

        //UpdateActions(uid, component);
    }

    private void UpdateCollisionCollide(EntityUid uid)
    {
        if (!TryComp(uid, out IntentComponent? component))
            return;
        if (!TryComp(uid, out StandingStateComponent? standingComp))
            return;

        if (component.Intent is not Intent.Help && standingComp.CurrentState > StandingState.GettingUp)
            _physics.TrySetBodyType(uid, Robust.Shared.Physics.BodyType.Dynamic);
        else
            _physics.TrySetBodyType(uid, Robust.Shared.Physics.BodyType.KinematicController);
    }
/*
    private void UpdateActions(EntityUid uid, IntentComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        _action.SetToggled(component.HelpActionEntity, component.Intent == Intent.Help);
        _action.SetToggled(component.DisarmActionEntity, component.Intent == Intent.Disarm);
        _action.SetToggled(component.GrabActionEntity, component.Intent == Intent.Grab);
        _action.SetToggled(component.HarmActionEntity, component.Intent == Intent.Harm);
    }
*/
    public bool CanAttack(EntityUid? uid)
    {
        if (uid == null)
            return false;
        if (!TryComp(uid, out IntentComponent? comp))
            return false;

        // We shouldn't attack or fire if we wanna throw item
        //if (TryComp(uid, out HandsComponent? handsComp))
        //{
        //    if (handsComp.InThrowMode)
        //        return false;
        //}

        // What the fuck are you doing spatison?
        // Why are we can attack the entity from grab and disarm mode?
        return comp.Intent is not Intent.Help;
        //return comp.Intent == Intent.Harm;
    }

    public virtual void SetIntent(EntityUid uid, Intent intent = Intent.Help, IntentComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.Intent = intent;
        Dirty(uid, component);

        //UpdateActions(uid, component);
    }

    public void SetCanDisarm(EntityUid uid, bool canDisarm, IntentComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.CanDisarm = canDisarm;
        Dirty(uid, component);
    }

    public Intent? GetIntent(EntityUid? uid, IntentComponent? component = null)
    {
        if (uid == null || !Resolve(uid.Value, ref component))
            return null;

        return component.Intent;
    }

    protected abstract bool IsNpc(EntityUid uid);
}

public sealed partial class ToggleIntentEvent : InstantActionEvent
{
    [DataField]
    public Intent Type = Intent.Harm;
}

public sealed class DisarmedEvent : HandledEntityEventArgs
{
    /// <summary>
    ///     The entity being disarmed.
    /// </summary>
    public EntityUid Target { get; init; }

    /// <summary>
    ///     The entity performing to disarm.
    /// </summary>
    public EntityUid Source { get; init; }

    /// <summary>
    ///     Probability for push/knockdown.
    /// </summary>
    public float PushProbability { get; init; }

    /// <summary>
    ///     Potential stamina damage if this disarm results in a shove.
    /// </summary>
    public float StaminaDamage { get; init; }
}
