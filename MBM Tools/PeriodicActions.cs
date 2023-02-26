using System;
using System.Collections.Generic; 

namespace Tools;

public class PeriodicAction
{
    public Guid id;
    public Action act;

    public PeriodicAction(Action act)
    {
        id = Guid.NewGuid();
        this.act = act;
    }
}

public class PeriodicActionGroup
{
    public Guid id;
    public float timeSinceRun;
    public float period;
    public IList<PeriodicAction> actions = new List<PeriodicAction>();

    public PeriodicActionGroup(float period, PeriodicAction act)
    {
        timeSinceRun = 0;
        this.period = period;
        actions.Add(act);
    }

    public void Act()
    {
        foreach(var action in actions)
        {
            action.act();
        }
    }
}