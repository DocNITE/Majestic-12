using Robust.Client.UserInterface;

namespace Content.Client._Finster.UserActions.Tabs;

[Virtual]
public class BaseTabControl : Control
{
    public virtual bool UpdateState() { return true; }
}
