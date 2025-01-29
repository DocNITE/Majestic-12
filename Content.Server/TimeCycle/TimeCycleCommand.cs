

using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.TimeCycle;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.Syntax;

namespace Content.Server.Toolshed;

//TODO: Add some events for time change

[ToolshedCommand, AdminCommand(AdminFlags.Admin)]
public sealed class TimeCycleCommand : ToolshedCommand
{
    [CommandImplementation("settime")]
    public void SetTime([PipedArgument] EntityUid map, [CommandArgument] int hour)
    {
        if (TryComp<TimeCycleTrackerComponent>(map, out var trackerComp))
        {
            var days = trackerComp.CurrentTime.Days;
            trackerComp.CurrentTime = TimeSpan.FromHours(hour);
            trackerComp.CurrentTime.Add(TimeSpan.FromDays(days));
            Logger.Info($"Set world time of {trackerComp.TrackerId} to {hour}:00");
        }
    }

    [CommandImplementation("togglepause")]
    public void TogglePause([PipedArgument] EntityUid map)
    {
        if (TryComp<TimeCycleTrackerComponent>(map, out var trackerComp))
        {
            trackerComp.Paused = !trackerComp.Paused;
            Logger.Info($"Set world pause: {trackerComp.Paused}");
        }
    }

    [CommandImplementation("addhours")]
    public void AddSixHours([PipedArgument] EntityUid map, [CommandArgument] int hours)
    {
        if (TryComp<TimeCycleTrackerComponent>(map, out var trackerComp))
        {
            trackerComp.CurrentTime.Add(TimeSpan.FromHours(hours));
            Logger.Info($"Added world hours: {hours}");
        }
    }
}
