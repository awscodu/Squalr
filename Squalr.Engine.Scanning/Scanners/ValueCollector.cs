﻿namespace Squalr.Engine.Scanning.Scanners
{
    using Squalr.Engine.Logging;
    using Squalr.Engine.Scanning.Snapshots;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using static Squalr.Engine.TrackableTask;

    /// <summary>
    /// Collect values for a given snapshot. The values are assigned to a new snapshot.
    /// </summary>
    public static class ValueCollector
    {
        /// <summary>
        /// The name of this scan.
        /// </summary>
        private const String Name = "Value Collector";

        public static TrackableTask<Snapshot> CollectValues(Snapshot snapshot)
        {
            return TrackableTask<Snapshot>
                .Create(ValueCollector.Name, out UpdateProgress updateProgress, out CancellationToken cancellationToken)
                .With(Task.Factory.StartNew<Snapshot>(() =>
                {
                    try
                    {
                        Int32 processedRegions = 0;

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        ParallelOptions options = ParallelSettings.ParallelSettingsFastest.Clone();
                        options.CancellationToken = cancellationToken;

                        // Read memory to get current values for each region
                        Parallel.ForEach(
                            snapshot.OptimizedReadGroups,
                            options,
                            (readGroup) =>
                            {
                                // Check for canceled scan
                                cancellationToken.ThrowIfCancellationRequested();

                                // Read the memory for this region
                                readGroup.ReadAllMemory();

                                // Update progress every N regions
                                if (Interlocked.Increment(ref processedRegions) % 32 == 0)
                                {
                                    updateProgress((float)processedRegions / (float)snapshot.RegionCount * 100.0f);
                                }
                            });

                        cancellationToken.ThrowIfCancellationRequested();
                        stopwatch.Stop();
                        Logger.Log(LogLevel.Info, "Values collected in: " + stopwatch.Elapsed);
                        return snapshot;
                    }
                    catch (OperationCanceledException ex)
                    {
                        Logger.Log(LogLevel.Warn, "Scan canceled", ex);
                        return null;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "Error performing scan", ex);
                        return null;
                    }
                }, cancellationToken));
        }
    }
    //// End class
}
//// End namespace