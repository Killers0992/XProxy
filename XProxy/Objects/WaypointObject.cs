using XProxy.Objects.Components;

namespace XProxy.Objects;

public class WaypointObject : SpawnableObject
{
    public byte WaypointId
    {
        get => WaypointComponent.WaypointId;
        set => WaypointComponent.WaypointId = value;
    }

    public WaypointComponent WaypointComponent { get; private set; }

    public WaypointObject(World world, byte waypointId) : base(world, null, 3938583646)
    {
        WithPayload = true;

        WaypointComponent = new WaypointComponent(this);
        Behaviours = new[]
        {
            WaypointComponent
        };

        WaypointId = waypointId;

        world.Waypoints.Add(WaypointId, this);
    }
}
