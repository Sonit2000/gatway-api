using gatway_clone.Utils;

namespace gatway_clone;

public class GlobalTasks
{
    public static readonly List<Task> Tasks = new();

    static GlobalTasks()
    {
        TimerUtil.RegisterTimer(() => Tasks.RemoveAll(x => x.IsCompleted), TimeSpan.FromMinutes(1));
    }
}
