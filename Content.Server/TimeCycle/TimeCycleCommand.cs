

using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.Syntax;

namespace Content.Server.Toolshed;

[ToolshedCommand, AdminCommand(AdminFlags.Admin)]
public sealed class TimeCycleCommand : ToolshedCommand
{
    [CommandImplementation("settime")]
    public void SetTime([PipedArgument] EntityUid map, [CommandArgument] int hour, [CommandArgument] int minute)
    {
    }
}
