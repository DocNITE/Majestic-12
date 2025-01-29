using System.Numerics;
using Content.Client.Parallax.Data;
using Content.Client.Parallax.Managers;
using Content.Shared.Parallax;
using Robust.Client.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client._Finster.ZLayer;

public sealed class ZLayerSystem : SharedParallaxSystem
{
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly IOverlayManager _overlay = default!;

    public const int ZIndex = 0;

    public override void Initialize()
    {
        base.Initialize();
        _overlay.AddOverlay(new ZLayerOverlay());
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<ZLayerOverlay>();
    }
}
