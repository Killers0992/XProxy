using XProxy.API.Networking.Common;

namespace XProxy.API.Core;

/// <summary>
/// Represents a game world, managing objects, waypoints, and clients.
/// Provides thread-safe access and runs periodic logic in a background thread.
/// </summary>
public class World : IDisposable
{
    private static readonly ReaderWriterLockSlim WorldsLock = new();
    /// <summary>
    /// Thread-safe dictionary of all worlds by their unique ID.
    /// </summary>
    public static Dictionary<int, World> WorldById { get; } = new();

    /// <summary>
    /// Finds a free world ID.
    /// </summary>
    public static int GetFreeWorldId()
    {
        WorldsLock.EnterReadLock();
        try
        {
            for (int x = 0; x < int.MaxValue; x++)
            {
                if (WorldById.ContainsKey(x))
                    continue;
                return x;
            }
        }
        finally
        {
            WorldsLock.ExitReadLock();
        }
        return 0;
    }

    /// <summary>
    /// Unique world ID.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// World name.
    /// </summary>
    public string Name { get; }

    public bool DestroyOnEmpty { get; set; }

    private readonly ReaderWriterLockSlim _lock = new();

    /// <summary>
    /// Thread-safe dictionary of objects in the world.
    /// </summary>
    public Dictionary<uint, NetworkObject> Objects { get; } = new();

    /// <summary>
    /// Thread-safe dictionary of waypoints in the world.
    /// </summary>
    public Dictionary<byte, WaypointToyObject> Waypoints { get; } = new();

    private readonly Thread _updateThread;
    private readonly CancellationTokenSource _cts = new();

    private readonly List<Client> _clients = new();
    private int _clientsVersion = 0;

    private readonly ThreadLocal<(int version, IReadOnlyList<Client> snapshot)> _clientsSnapshotCache
        = new(() => (-1, null));

    /// <summary>
    /// Initializes a new world and starts its update thread.
    /// </summary>
    /// <param name="name">World name.</param>
    public World(string name)
    {
        Id = GetFreeWorldId();
        WorldsLock.EnterWriteLock();
        try
        {
            WorldById.Add(Id, this);
        }
        finally
        {
            WorldsLock.ExitWriteLock();
        }
        Name = name;

        _updateThread = new Thread(UpdateLoop)
        {
            IsBackground = true,
            Name = $"WorldUpdateThread-{Id}"
        };
        _updateThread.Start();
    }

    /// <summary>
    /// Periodic update logic, runs every 10ms in a background thread.
    /// Override to implement world logic.
    /// </summary>
    public virtual void Update()
    {
    }

    private void UpdateLoop()
    {
        var token = _cts.Token;
        try
        {
            while (!token.IsCancellationRequested)
            {
                Update();
                Thread.Sleep(10);
            }
        }
        catch (ThreadAbortException) { }
        catch (Exception ex)
        {
            ProxyLogger.Error($"Exception in World update thread: {ex}", "World");
        }
    }

    /// <summary>
    /// Gets a free waypoint ID.
    /// </summary>
    public byte GetFreeWaypointId()
    {
        _lock.EnterReadLock();
        try
        {
            for (byte x = 1; x < byte.MaxValue; x++)
            {
                if (Waypoints.ContainsKey(x))
                    continue;

                return x;
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }
        return 0;
    }

    /// <summary>
    /// Adds a waypoint at the specified position.
    /// </summary>
    public void AddWaypoint(Vector3 position)
    {
        var waypoint = new WaypointToyObject(this)
        {
            Position = position
        };

        waypoint.WaypointToy.BoundsSize = new Vector3(100f, 100f, 100f);
        waypoint.WaypointToy.WaypointId = GetFreeWaypointId();

        ProxyLogger.Info("Add waypoint " + waypoint.WaypointToy.WaypointId + " at " + position, "World");

        _lock.EnterWriteLock();
        try
        {
            Waypoints[waypoint.WaypointToy.WaypointId] = waypoint;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Gets a free object ID.
    /// </summary>
    public uint GetFreeId()
    {
        for (uint x = 0; x < uint.MaxValue; x++)
        {
            if (Objects.ContainsKey(x))
                continue;
            return x;
        }

        return 0;
    }

    /// <summary>
    /// Loads a client into the world.
    /// </summary>
    public bool Load(Client client)
    {
        bool result;
        _lock.EnterWriteLock();
        try
        {
            if (_clients.Contains(client))
                return false;

            OnLoad(client);

            _clients.Add(client);
            _clientsVersion++;

            ProxyLogger.Info($"{client.Tag} Loaded world (f=green){this}(f=white)", "Client");

            result = true;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
        
        SpawnObjectsForClient(client);

        return result;
    }

    /// <summary>
    /// Unloads a client from the world.
    /// </summary>
    public bool Unload(Client client)
    {
        _lock.EnterWriteLock();
        try
        {
            if (!_clients.Contains(client))
                return false;

            OnUnload(client);
            _clients.Remove(client);
            _clientsVersion--;

            ProxyLogger.Info($"{client.Tag} Unloaded world (f=green){this}(f=white)", "Client");
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        if (GetClientsSnapshot().Count == 0 && DestroyOnEmpty)
            Dispose();

        return true;
    }

    /// <summary>
    /// Spawns all objects for a client.
    /// </summary>
    public void SpawnObjectsForClient(Client client)
    {
        _lock.EnterReadLock();
        try
        {
            foreach (var obj in Objects)
            {
                if (obj.Value.WithPayload)
                {
                    // Spawn objects for client.
                    ProxyLogger.Info($"Spawn {obj.Value.GetType().Name} for {client.PreAuth.UserId}");
                    obj.Value.SpawnWithPayload(client);
                }
                else
                {
                    ProxyLogger.Info($"Spawn normal {obj.Value.GetType().Name} for {client.PreAuth.UserId}");
                    obj.Value.Spawn(client);
                }
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }
        // Call outside the lock to avoid recursion
        OnObjectsSpawned(client);
    }

    /// <summary>
    /// Returns a thread-local snapshot of the current clients. The snapshot is reused for the same thread if the list hasn't changed.
    /// </summary>
    public IReadOnlyList<Client> GetClientsSnapshot()
    {
        var cache = _clientsSnapshotCache.Value;
        int currentVersion;
        List<Client> snapshot = null;

        _lock.EnterReadLock();
        try
        {
            currentVersion = _clientsVersion;
            if (cache.version == currentVersion && cache.snapshot != null)
                return cache.snapshot;

            snapshot = [.. _clients];
        }
        finally
        {
            _lock.ExitReadLock();
        }

        _clientsSnapshotCache.Value = (currentVersion, snapshot);
        return snapshot;
    }

    /// <summary>
    /// Called when a client loads the world.
    /// </summary>
    public virtual void OnLoad(Client client) { }

    /// <summary>
    /// Called after all objects are spawned for a client.
    /// </summary>
    public virtual void OnObjectsSpawned(Client client) { }

    /// <summary>
    /// Called when a client unloads the world.
    /// </summary>
    public virtual void OnUnload(Client client) { }

    public virtual void OnDestroy() { }

    /// <summary>
    /// Disposes the world, stops the update thread, and removes it from the global list.
    /// </summary>
    public void Dispose()
    {
        OnDestroy();

        _cts.Cancel();
        _updateThread.Join();

        WorldsLock.EnterWriteLock();

        try
        {
            WorldById.Remove(Id);
        }
        finally
        {
            WorldsLock.ExitWriteLock();
        }

        _lock.Dispose();
        _cts.Dispose();

        _clientsSnapshotCache.Dispose();
    }

    /// <summary>
    /// Returns a string representation of the world.
    /// </summary>
    public override string ToString()
    {
        return $"[{Id}] {Name}";
    }
}