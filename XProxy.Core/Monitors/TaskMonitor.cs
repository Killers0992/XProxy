using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace XProxy.Core.Monitors
{

    public class TaskMonitor
    {
        private static ConcurrentDictionary<int, Task> _activeTasks = new ConcurrentDictionary<int, Task>();

        public static void RegisterTask(Task task)
        {
            _activeTasks.TryAdd(task.Id, task);
            task.ContinueWith(t => _activeTasks.TryRemove(t.Id, out _));
        }

        public static int GetRunningTaskCount()
        {
            return _activeTasks.Count;
        }
    }
}
