using System;

namespace XIVATM.Structs;

public class GlobalEvent
{
    public Action Register { get; }
    public Action Unregister { get; }

    public GlobalEvent(Action register, Action unregister)
    {
        Register = register;
        Unregister = unregister;
    }
}
