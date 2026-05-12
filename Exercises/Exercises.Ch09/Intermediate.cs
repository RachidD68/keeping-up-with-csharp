// Chapter 9 — Asynchronous Programming Evolution — INTERMEDIATE
// ----------------------------------------------------------------
// Exercise: Build a Task.WhenEach-based progress indicator. Given
//   a Task<T>[] array, report "[progress] k/n completed" each time
//   a task finishes, using Task.WhenEach. Verify the output order
//   matches completion time, not array order. Test with an array
//   of Task.Delay tasks with randomized delays.
//
// Hint: Count in the await foreach loop; no extra bookkeeping.
// ----------------------------------------------------------------

namespace KeepUpCs.Exercises.Ch09;

public static class ProgressIndicator
{
    /// <summary>
    /// Awaits every task and reports progress to <paramref name="report"/>
    /// each time a task finishes, in completion order. Returns the
    /// completed results in completion order (NOT array order).
    /// </summary>
    public static async Task<List<T>> RunWithProgressAsync<T>(
        Task<T>[] tasks,
        Action<int, int, T> report)
    {
        var completed = new List<T>(tasks.Length);
        int n = tasks.Length;
        int k = 0;
        // Task.WhenEach yields each task in the order it completes.
        await foreach (var finished in Task.WhenEach(tasks))
        {
            T result = await finished;
            k++;
            completed.Add(result);
            report(k, n, result);
        }
        return completed;
    }
}

public static class IntermediateDemo
{
    public static async Task RunAsync()
    {
        Console.WriteLine("Ch09 Intermediate — Task.WhenEach progress indicator");
        Console.WriteLine(new string('─', 60));

        var rng = new Random(7);
        // Eight tasks with randomised delays 100–800 ms.
        var tasks = Enumerable.Range(0, 8)
            .Select(i =>
            {
                int ms = 100 + rng.Next(700);
                return DelayedAsync(i, ms);
            })
            .ToArray();

        var order = await ProgressIndicator.RunWithProgressAsync(
            tasks,
            report: (k, n, label) =>
                Console.WriteLine($"  [progress] {k}/{n} completed → {label}"));

        Console.WriteLine();
        Console.WriteLine($"  Array order   : 0,1,2,3,4,5,6,7");
        Console.WriteLine($"  Finish order  : {string.Join(",", order.Select(o => o.Split(':')[0]))}");
    }

    private static async Task<string> DelayedAsync(int id, int delayMs)
    {
        await Task.Delay(delayMs);
        return $"{id}:done after {delayMs} ms";
    }
}
