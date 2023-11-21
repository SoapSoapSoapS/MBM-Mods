using System;
using System.Collections.Generic;
using HarmonyLib;
using MBMScripts;

namespace Tools;

public struct CustomAction : IEquatable<CustomAction>
{
    public Guid id;
    public Action act;

    public CustomAction(Action act)
    {
        id = Guid.NewGuid();
        this.act = act;
    }

    public bool Equals(CustomAction obj)
    {
        return id == obj.id;
    }
}

public struct CustomAction<T> : IEquatable<CustomAction<T>>
{
    public Guid id;
    public Action<T> act;

    public CustomAction(Action<T> act)
    {
        id = Guid.NewGuid();
        this.act = act;
    }

    public bool Equals(CustomAction<T> obj)
    {
        return id == obj.id;
    }
}

public class PeriodicActionGroup
{
    public Guid id;
    public float timeSinceRun;
    public float period;
    public IList<CustomAction> actions = new List<CustomAction>();

    public PeriodicActionGroup(float period, CustomAction act)
    {
        timeSinceRun = 0;
        this.period = period;
        actions.Add(act);
    }

    public void Act()
    {
        foreach (var action in actions)
        {
            action.act();
        }
    }
}
