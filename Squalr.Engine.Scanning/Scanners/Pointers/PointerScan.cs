﻿using Squalr.Engine.OS;

namespace Squalr.Engine.Scanning.Scanners.Pointers
{
    using Squalr.Engine.Logging;
    using Squalr.Engine.Scanning.Scanners.Pointers.Structures;
    using Squalr.Engine.Scanning.Snapshots;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using static Squalr.Engine.TrackableTask;

    /// <summary>
    /// Scans for pointers in the target process.
    /// </summary>
    public static class PointerScan
    {
        /// <summary>
        /// The name of this scan.
        /// </summary>
        private const String Name = "Pointer Scan";

        /// <summary>
        /// Performs a pointer scan for a given address.
        /// </summary>
        /// <param name="address">The address for which to perform a pointer scan.</param>
        /// <param name="maxOffset">The maximum pointer offset.</param>
        /// <param name="depth">The maximum pointer search depth.</param>
        /// <param name="alignment">The pointer scan alignment.</param>
        /// <returns>Atrackable task that returns the scan results.</returns>
        public static TrackableTask<PointerBag> Scan(UInt64 address, UInt32 maxOffset, Int32 depth, Int32 alignment)
        {
            TrackableTask<PointerBag> pointerScanTask = TrackableTask<PointerBag>.Create(PointerScan.Name, out UpdateProgress updateProgress, out CancellationToken cancellationToken);

            return pointerScanTask.With(Task.Factory.StartNew<PointerBag>(() =>
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        PointerSize pointerSize = Processes.Default.IsOpenedProcess32Bit() ? PointerSize.Byte4 : PointerSize.Byte8;

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        // Step 1) Create a snapshot of the target address
                        Snapshot targetAddress = new Snapshot(new SnapshotRegion[] { new SnapshotRegion(new ReadGroup(address, pointerSize.ToSize(), pointerSize.ToDataType(), alignment), 0, pointerSize.ToSize()) });

                        // Step 2) Collect static pointers
                        Snapshot staticPointers = SnapshotManager.GetSnapshot(Snapshot.SnapshotRetrievalMode.FromModules, pointerSize.ToDataType());
                        TrackableTask<Snapshot> valueCollector = ValueCollector.CollectValues(staticPointers);
                        staticPointers = valueCollector.Result;

                        // Step 3) Collect heap pointers
                        Snapshot heapPointers = SnapshotManager.GetSnapshot(Snapshot.SnapshotRetrievalMode.FromHeaps, pointerSize.ToDataType());
                        TrackableTask<Snapshot> heapValueCollector = ValueCollector.CollectValues(heapPointers);
                        heapPointers = heapValueCollector.Result;

                        // Step 3) Build levels
                        IList<Level> levels = new List<Level>();

                        if (depth > 0)
                        {
                            // Create 1st level with target address and static pointers
                            levels.Add(new Level(targetAddress, staticPointers));

                            // Initialize each level with all static addresses and all heap addresses
                            for (Int32 index = 0; index < depth - 1; index++)
                            {
                                levels.Add(new Level(heapPointers, staticPointers));
                            }
                        }

                        // Exit if canceled
                        cancellationToken.ThrowIfCancellationRequested();

                        // Step 4) Rebase to filter out unwanted pointers
                        PointerBag newPointerBag = new PointerBag(levels, maxOffset, pointerSize);
                        TrackableTask<PointerBag> pointerRebaseTask = PointerRebase.Scan(newPointerBag, readMemory: false, performUnchangedScan: false);
                        PointerBag rebasedPointerBag = pointerRebaseTask.Result;

                        // Exit if canceled
                        cancellationToken.ThrowIfCancellationRequested();

                        stopwatch.Stop();
                        Logger.Log(LogLevel.Info, "Pointer scan complete in: " + stopwatch.Elapsed);

                        return rebasedPointerBag;
                    }
                    catch (OperationCanceledException ex)
                    {
                        Logger.Log(LogLevel.Warn, "Pointer scan canceled", ex);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "Error performing pointer scan", ex);
                    }

                    return null;
                }, cancellationToken));
        }
    }
    //// End class
}
//// End namespace