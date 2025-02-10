/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 */


using System;
using Content.KayMisaZlevels.Shared.Miscellaneous;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;

namespace Content.KayMisaZlevels.Shared.Components;

/// <summary>
///     Marker that transitions between Z-Levels
/// </summary>
[RegisterComponent]
public sealed partial class ZTransitionMarkerComponent : Component
{
    /// <summary>
    /// Direction this marker transitions an entity (Up or Down)
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("direction")]
    public ZDirection Dir;

    /// <summary>
    /// AHTUNG!: Used only for prototypes like ladders and stairs for define ZDirection Dir;
    /// </summary>
    [DataField("directionStr")]
    public string DirStr = "down";

    /// <summary>
    /// Map position of this marker
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [NonSerialized]
    public MapCoordinates Position;
}
