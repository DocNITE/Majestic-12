using Content.KayMisaZlevels.Server.Systems;
using Content.KayMisaZlevels.Shared.Components;
using Content.KayMisaZlevels.Shared.Miscellaneous;
using Content.Server.Ghost.Components;
using Content.Server.Popups;
using Content.Server.Warps;
using Content.Shared.Ghost;
using Content.Shared.Interaction;
using Content.Shared.Teleportation.Components;
using Content.Shared.Teleportation.Systems;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using System.Linq;
using System.Numerics;

namespace Content.Server._KMZLevels.ZTransition;

public class ZTransitionLaddersSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly WarpPointSystem _warpPointSystem = default!;
    [Dependency] private readonly LinkedEntitySystem _linkedEntitySystem = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly ZStackSystem _zStack = default!;

    private uint _nextKeyId = 0;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ZLadderComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<ZLadderComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<ZLadderComponent, GetVerbsEvent<Verb>>(AddVerbs);

        SubscribeLocalEvent<ZLadderAutoLinkComponent, ComponentInit>(OnAutoLinkInit);
        SubscribeLocalEvent<ZLadderAutoLinkComponent, MapInitEvent>(HandleMapInitialization);
    }

    private void OnShutdown(Entity<ZLadderComponent> entity, ref ComponentShutdown args)
    {
        if (!TryComp<LinkedEntityComponent>(entity, out var linkComp) || linkComp.LinkedEntities.Count <= 0)
            return;

        var linkedEntity = linkComp.LinkedEntities.First();

        if (_linkedEntitySystem.TryUnlink(entity, linkedEntity))
            Del(linkedEntity);
    }

    private void OnAutoLinkInit(Entity<ZLadderAutoLinkComponent> entity, ref ComponentInit args)
    {
    }

    private void HandleMapInitialization(Entity<ZLadderAutoLinkComponent> entity, ref MapInitEvent eventArgs)
    {
        var result = PerformAutoLink(entity, out var linkedEntity);
        if (!result)
            Del(entity);
    }

    public bool PerformAutoLink(Entity<ZLadderAutoLinkComponent> entity, out EntityUid? linkedEntityId)
    {
        linkedEntityId = null;

        if (!TryComp<TransformComponent>(entity, out var fXformComp) || fXformComp.MapUid is null)
            return false;

        if (!TryComp<ZTransitionMarkerComponent>(entity, out var zTransMarkerComp))
            return false;

        var firstMapUid = fXformComp.MapUid.Value;

        if (!_zStack.TryGetZStack(firstMapUid, out var zStack))
            return false; // Not in a Z level containing space.

        var coords = _xform.GetWorldPosition(entity);
        var maps = zStack.Value.Comp.Maps;
        var mapIdx = maps.IndexOf(firstMapUid);
        int moveDir;

        if (zTransMarkerComp.Dir == ZDirection.Up)
        {
            if (mapIdx >= maps.Count - 1)
                return false;
            moveDir = 1;
        }
        else
        {
            if (mapIdx <= 0)
                return false;
            moveDir = -1;
        }

        linkedEntityId = Spawn(entity.Comp.OtherSideProto, new EntityCoordinates(maps[mapIdx + moveDir], coords));
        if (linkedEntityId is null)
            return false;
        _xform.SetWorldRotation((EntityUid) linkedEntityId, _xform.GetWorldRotation(entity));

        if (_linkedEntitySystem.TryLink(entity, (EntityUid) linkedEntityId, true))
        {
            RemComp<ZLadderAutoLinkComponent>((EntityUid) linkedEntityId);
            RemComp<ZLadderAutoLinkComponent>(entity);
            return true;
        }
        return false;
    }

    private void AddVerbs(EntityUid uid, ZLadderComponent component, GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var verb = new Verb()
        {
            Text = Loc.GetString("ladder-verb-use"),
            Act = () => TryInteract(uid, component, args.User, args.Target),
        };

        args.Verbs.Add(verb);
    }

    private void OnInteractHand(EntityUid uid, ZLadderComponent component, InteractHandEvent args)
    {
        TryInteract(uid, component, args.User, args.Target);
    }

    public void TryInteract(EntityUid uid, ZLadderComponent component, EntityUid user, EntityUid target)
    {
        if (!TryComp<LinkedEntityComponent>(uid, out var linkComp) || linkComp.LinkedEntities.Count <= 0)
        {
            SendPopupEntity(user, target);
            return;
        }

        var dest = linkComp.LinkedEntities.First();

        var entMan = IoCManager.Resolve<IEntityManager>();
        TransformComponent? destXform;
        entMan.TryGetComponent<TransformComponent>(dest, out destXform);
        if (destXform is null)
        {
            SendPopupEntity(user, target);
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
                SendPopupEntity(user, target);
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

    public void SendPopupEntity(EntityUid user, EntityUid target)
    {
        _popupSystem.PopupEntity(Loc.GetString("ladder-goes-nowhere", ("ladder", target)), user, Filter.Entities(user), true);
    }
}
