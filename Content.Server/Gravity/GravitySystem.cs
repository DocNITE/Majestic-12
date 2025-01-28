using Content.KayMisaZlevels.Shared.Systems;
using Content.Shared.Gravity;
using Content.Shared.Maps;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;

namespace Content.Server.Gravity
{
    [UsedImplicitly]
    public sealed class GravitySystem : SharedGravitySystem
    {
        [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
        [Dependency] private readonly SharedMapSystem _mapSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<GravityComponent, ComponentInit>(OnGravityInit);
            SubscribeLocalEvent<IsGravityAffectedEvent>(OnGravityAffected);
            SubscribeLocalEvent<IsGravitySource>(OnGravitySource);
        }

        private void OnGravityAffected(ref IsGravityAffectedEvent args)
        {
            // TODO: Maybe impelement better gravity affection for Z levels

            if (!TryComp<TransformComponent>(args.Target, out var xform))
                return;

            if (!TryComp<PhysicsComponent>(args.Target, out var physicsComponent))
                return;

            ContentTileDefinition? tileDef = null;

            // Don't bother getting the tiledef here if we're weightless or in-air
            // since no tile-based modifiers should be applying in that situation
            if (TryComp(xform.GridUid, out MapGridComponent? gridComp)
                && _mapSystem.TryGetTileRef(xform.GridUid.Value, gridComp, xform.Coordinates, out var tile))
            {
                tileDef = (ContentTileDefinition) _tileDefinitionManager[tile.Tile.TypeId];
            }

            if ((tileDef is null || tileDef.ID == "Space") && physicsComponent.BodyStatus != BodyStatus.InAir)
                args.Affected = true;
        }

        private void OnGravitySource(ref IsGravitySource args)
        {
            if (TryComp<GravityComponent>(args.Target, out var comp))
                args.Handled = comp.Enabled;
        }

        /// <summary>
        /// Iterates gravity components and checks if this entity can have gravity applied.
        /// </summary>
        public void RefreshGravity(EntityUid uid, GravityComponent? gravity = null)
        {
            if (!Resolve(uid, ref gravity))
                return;

            if (gravity.Inherent)
                return;

            var enabled = false;

            foreach (var (comp, xform) in EntityQuery<GravityGeneratorComponent, TransformComponent>(true))
            {
                if (!comp.GravityActive || xform.ParentUid != uid)
                    continue;

                enabled = true;
                break;
            }

            if (enabled != gravity.Enabled)
            {
                gravity.Enabled = enabled;
                var ev = new GravityChangedEvent(uid, enabled);
                RaiseLocalEvent(uid, ref ev, true);
                Dirty(uid, gravity);

                if (HasComp<MapGridComponent>(uid))
                {
                    StartGridShake(uid);
                }
            }
        }

        private void OnGravityInit(EntityUid uid, GravityComponent component, ComponentInit args)
        {
            RefreshGravity(uid);
        }

        /// <summary>
        /// Enables gravity. Note that this is a fast-path for GravityGeneratorSystem.
        /// This means it does nothing if Inherent is set and it might be wiped away with a refresh
        ///  if you're not supposed to be doing whatever you're doing.
        /// </summary>
        public void EnableGravity(EntityUid uid, GravityComponent? gravity = null)
        {
            if (!Resolve(uid, ref gravity))
                return;

            if (gravity.Enabled || gravity.Inherent)
                return;

            gravity.Enabled = true;
            var ev = new GravityChangedEvent(uid, true);
            RaiseLocalEvent(uid, ref ev, true);
            Dirty(uid, gravity);

            if (HasComp<MapGridComponent>(uid))
            {
                StartGridShake(uid);
            }
        }
    }
}
