using Content.Server.Ghost.Components;
using Content.Server.Popups;
using Content.Server.Warps;
using Content.Shared.Ghost;
using Content.Shared.Interaction;
using Content.Shared.Verbs;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using System.Numerics;

namespace Content.Server._N14.Warps;

public class WarperSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly WarpPointSystem _warpPointSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WarperComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<WarperComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<WarperComponent, GetVerbsEvent<Verb>>(AddVerbs);
    }

    private void OnShutdown(Entity<WarperComponent> entity, ref ComponentShutdown args)
    {
        if (entity.Comp.Location is null)
            return;

        var warpUid = _warpPointSystem.FindWarpPoint(entity.Comp.Location);

        if (!TryComp<WarpPointComponent>(warpUid, out var warpComp))
            return;

        if (warpUid is not null && warpComp.SyncedByWarper)
            Del(warpUid);
    }

    private void AddVerbs(EntityUid uid, WarperComponent component, GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var verb = new Verb()
        {
            Text = Loc.GetString("warper-verb-use"),
            Act = () => TryInteract(uid, component, args.User, args.Target),
        };

        args.Verbs.Add(verb);
    }

    private void OnInteractHand(EntityUid uid, WarperComponent component, InteractHandEvent args)
    {
        TryInteract(uid, component, args.User, args.Target);
    }

    public void TryInteract(EntityUid uid, WarperComponent component, EntityUid user, EntityUid target)
    {
        if (component.Location is null)
        {
            Logger.DebugS("warper", "Warper has no destination");
            _popupSystem.PopupEntity(Loc.GetString("warper-goes-nowhere", ("warper", target)), user, Filter.Entities(user), true);
            return;
        }

        var dest = _warpPointSystem.FindWarpPoint(component.Location);
        if (dest is null)
        {
            Logger.DebugS("warper", String.Format("Warp destination '{0}' not found", component.Location));
            _popupSystem.PopupEntity(Loc.GetString("warper-goes-nowhere", ("warper", target)), user, Filter.Entities(user), true);
            return;
        }

        var entMan = IoCManager.Resolve<IEntityManager>();
        TransformComponent? destXform;
        entMan.TryGetComponent<TransformComponent>(dest.Value, out destXform);
        if (destXform is null)
        {
            Logger.DebugS("warper", String.Format("Warp destination '{0}' has no transform", component.Location));
            _popupSystem.PopupEntity(Loc.GetString("warper-goes-nowhere", ("warper", target)), user, Filter.Entities(user), true);
            return;
        }

        // Check that the destination map is initialized and return unless in aghost mode.
        var mapMgr = IoCManager.Resolve<IMapManager>();
        var destMap = destXform.MapID;
        if (!mapMgr.IsMapInitialized(destMap) || mapMgr.IsMapPaused(destMap))
        {
            if (!entMan.HasComponent<GhostComponent>(user))
            {
                // Normal ghosts cannot interact, so if we're here this is already an admin ghost.
                Logger.DebugS("warper", String.Format("Player tried to warp to '{0}', which is not on a running map", component.Location));
                _popupSystem.PopupEntity(Loc.GetString("warper-goes-nowhere", ("warper", target)), user, Filter.Entities(user), true);
                return;
            }
        }

        var xform = entMan.GetComponent<TransformComponent>(user);
        xform.Coordinates = destXform.Coordinates;
        xform.AttachToGridOrMap();
        if (entMan.TryGetComponent(uid, out PhysicsComponent? phys))
        {
            _physics.SetLinearVelocity(uid, Vector2.Zero);
        }
    }
}
