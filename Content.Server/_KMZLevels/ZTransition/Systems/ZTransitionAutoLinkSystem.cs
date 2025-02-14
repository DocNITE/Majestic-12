using Content.KayMisaZlevels.Server.Systems;
using Content.KayMisaZlevels.Shared.Components;
using Content.KayMisaZlevels.Shared.Miscellaneous;
using Content.Shared.Teleportation.Components;
using Content.Shared.Teleportation.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using System.Linq;

namespace Content.Server._KMZLevels.ZTransition;

public class ZTransitionAutoLinkSystem : EntitySystem
{
    [Dependency] private readonly LinkedEntitySystem _linkedEntitySystem = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly ZStackSystem _zStack = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ZTransitionAutoLinkComponent, ComponentInit>(OnAutoLinkInit);
        SubscribeLocalEvent<ZTransitionAutoLinkComponent, ComponentShutdown>(OnAutoLinkShutdown);
        SubscribeLocalEvent<ZTransitionAutoLinkComponent, MapInitEvent>(HandleMapInitialization);
    }

    private void OnAutoLinkInit(Entity<ZTransitionAutoLinkComponent> entity, ref ComponentInit args)
    {
    }

    private void OnAutoLinkShutdown(Entity<ZTransitionAutoLinkComponent> entity, ref ComponentShutdown args)
    {
        if (!TryComp<LinkedEntityComponent>(entity, out var linkComp) || linkComp.LinkedEntities.Count <= 0)
            return;

        var linkedEntity = linkComp.LinkedEntities.First();

        if (_linkedEntitySystem.TryUnlink(entity, linkedEntity))
            Del(linkedEntity);
    }

    private void HandleMapInitialization(Entity<ZTransitionAutoLinkComponent> entity, ref MapInitEvent eventArgs)
    {
        var result = PerformAutoLink(entity, out var linkedEntity);
        if (!result)
            Del(entity);
    }

    public bool PerformAutoLink(Entity<ZTransitionAutoLinkComponent> entity, out EntityUid? linkedEntityId)
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
            RemComp<ZTransitionAutoLinkComponent>((EntityUid) linkedEntityId);
            RemComp<ZTransitionAutoLinkComponent>(entity);
            return true;
        }
        return false;
    }
}
