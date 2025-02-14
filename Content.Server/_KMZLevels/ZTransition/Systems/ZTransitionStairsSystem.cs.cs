
using System.Linq;
using Content.KayMisaZlevels.Shared.Components;
using Content.KayMisaZlevels.Shared.Miscellaneous;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Teleportation.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;

namespace Content.Server._KMZLevels.ZTransition;

public abstract class ZTransitionStairsSystem : EntitySystem
{
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private const string UpFixture = "upFixture";
    private const string DownFixture = "downFixture";

    public override void Initialize()
    {
        SubscribeLocalEvent<ZStairsComponent, StartCollideEvent>(OnTeleportStartCollide);
    }

    private void OnTeleportStartCollide(Entity<ZStairsComponent> ent, ref StartCollideEvent args)
    {
        if (!ShouldCollide(args.OurFixtureId, out var dir))
            return;
        if (!TryComp<LinkedEntityComponent>(ent, out var linkComp) || linkComp.LinkedEntities.Count <= 0)
            return;

        var target = linkComp.LinkedEntities.First();
        var other = args.OtherEntity;

        if (!TryComp<TransformComponent>(target, out var xformComp) || xformComp.MapUid is null)
            return;

        var otherCoords = _transform.GetMapCoordinates(other);
        var teleporter = _transform.GetMapCoordinates(ent);

        var diff = otherCoords.Position - teleporter.Position;
        if (diff.Length() > 10)
            return;

        teleporter = teleporter.Offset(diff);
        teleporter = teleporter.Offset(ent.Comp.Adjust);

        // Shit, i don't like RMC-14 code. Why are we teleports by HandlePulling? What the fuck?
        HandlePulling(other, (EntityUid) xformComp.MapUid, teleporter);
    }
/*
    private bool TryGetTargetMapUid()
    {

    }
*/
    private bool ShouldCollide(string ourId, out ZDirection? dir)
    {
        dir = null;

        var targetFixture = "";

        if (ourId == UpFixture)
        {
            targetFixture = UpFixture;
            dir = ZDirection.Up;
        }
        else if (ourId == DownFixture)
        {
            targetFixture = DownFixture;
            dir = ZDirection.Down;
        }

        return ourId == targetFixture;
    }

    public void HandlePulling(EntityUid user, EntityUid mapTarget, MapCoordinates teleport)
    {
        if (TryComp(user, out PullableComponent? otherPullable) &&
            otherPullable.Puller != null)
        {
            _pulling.TryStopPull(user, otherPullable, otherPullable.Puller.Value);
        }

        if (TryComp(user, out PullerComponent? puller) &&
            TryComp(puller.Pulling, out PullableComponent? pullable))
        {
            if (TryComp(puller.Pulling, out PullerComponent? otherPullingPuller) &&
                TryComp(otherPullingPuller.Pulling, out PullableComponent? otherPullingPullable))
            {
                _pulling.TryStopPull(otherPullingPuller.Pulling.Value, otherPullingPullable, puller.Pulling);
            }

            var pulling = puller.Pulling.Value;
            _pulling.TryStopPull(pulling, pullable, user);
            _transform.SetCoordinates(user, new EntityCoordinates(mapTarget, teleport.Position));
            _transform.SetCoordinates(pulling, new EntityCoordinates(mapTarget, teleport.Position));
            _pulling.TryStartPull(user, pulling);
        }
        else
        {
            _transform.SetCoordinates(user, new EntityCoordinates(mapTarget, teleport.Position));
        }
    }
}
