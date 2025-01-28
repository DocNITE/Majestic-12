using System.Linq;
using Content.Shared.Examine;
using Content.Shared.Ghost;

namespace Content.Server.Warps;

public sealed class WarpPointSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WarpPointComponent, ExaminedEvent>(OnWarpPointExamine);
    }

    // FINSTER EDIT - Need for ladders
    public EntityUid? FindWarpPoint(string id)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        var found = entMan.EntityQuery<WarpPointComponent>(true).Where(p => p.Location == id).FirstOrDefault();
        if (found is not null)
            return found.Owner;
        else
            return null;
    }
    // FINSTER EDIT END

    private void OnWarpPointExamine(EntityUid uid, WarpPointComponent component, ExaminedEvent args)
    {
        if (!HasComp<GhostComponent>(args.Examiner))
            return;

        var loc = component.Location == null ? "<null>" : $"'{component.Location}'";
        args.PushText(Loc.GetString("warp-point-component-on-examine-success", ("location", loc)));
    }
}
