using System;
using System.Diagnostics;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BenchmarkPhysics
{
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/_Project/Scenes/Laboratory_Main.unity");
        Physics.SyncTransforms();
        const int iterations = 10000;
        var buffer = new Collider[16];
        for(int i=0;i<100;i++) Physics.OverlapSphereNonAlloc(Vector3.zero,30,buffer,1<<9);
        var watch = new Stopwatch();
        long before = GC.GetAllocatedBytesForCurrentThread();
        watch.Start();
        int count = 0;
        for(int i=0;i<iterations;i++) count += Physics.OverlapSphere(Vector3.zero,30,1<<9).Length;
        watch.Stop();
        long baseline = GC.GetAllocatedBytesForCurrentThread()-before;
        double baselineMs = watch.Elapsed.TotalMilliseconds;
        watch.Restart();
        before = GC.GetAllocatedBytesForCurrentThread();
        int reusedCount = 0;
        for(int i=0;i<iterations;i++) reusedCount += Physics.OverlapSphereNonAlloc(Vector3.zero,30,buffer,1<<9);
        long reused = GC.GetAllocatedBytesForCurrentThread()-before;
        watch.Stop();
        if(count != reusedCount || count != iterations*4) throw new Exception("Benchmark receiver mismatch.");
        UnityEngine.Debug.Log($"LAZARO_BENCHMARK editor iterations={iterations} baseline_bytes={baseline} nonalloc_bytes={reused} baseline_ms={baselineMs:F3} nonalloc_ms={watch.Elapsed.TotalMilliseconds:F3} receivers_per_query=4");
    }
    public static void Release()
    {
        Run();
        BuildWindows.Release();
    }
}
