namespace Content.Server.Warps
{
    /// <summary>
    /// Allows ghosts etc to warp to this entity by name.
    /// </summary>
    [RegisterComponent]
    public sealed partial class WarpPointComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite), DataField]
        public string? Location;

        /// <summary>
        ///     If true, ghosts warping to this entity will begin following it.
        /// </summary>
        [DataField]
        public bool Follow;

        /// <summary>
        /// If true, entity with component would be deleted, when entity with <seealso cref="WarperComponent"/> will be deleted
        /// Like ladders and another teleporters. Because it can be builded by the players.
        /// </summary>
        [DataField]
        public bool SyncedByWarper = false;
    }
}
