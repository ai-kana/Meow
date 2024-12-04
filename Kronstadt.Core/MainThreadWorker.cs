using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;
using Action = System.Action;

namespace Kronstadt.Core;

public class MainThreadWorker : MonoBehaviour
{
    private static ConcurrentQueue<WorkWrapper> WorkQueue = new();
    private static ConcurrentQueue<IEnumerator> CoroutineQueue = new();
    private static ConcurrentQueue<IEnumerator> CancelCoroutineQueue = new();

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if (WorkQueue.TryDequeue(out WorkWrapper work))
        {
            work.Run();
        }

        if (CoroutineQueue.TryDequeue(out IEnumerator result))
        {
            StartCoroutine(result);
        }

        if (CancelCoroutineQueue.TryDequeue(out IEnumerator cancel))
        {
            StopCoroutine(cancel);
        }
    }

    public static void Enqueue(Action work)
    {
        WorkQueue.Enqueue(new(work));
    }

    public static void EnqueueCoroutine(IEnumerator routine)
    {
        CoroutineQueue.Enqueue(routine);
    }

    public static void CancelCoroutine(IEnumerator routine)
    {
        CancelCoroutineQueue.Enqueue(routine);
    }

    /// <summary>
    /// This is dogshit but fine since its really only used one shutdown when server stops anyway :P;
    /// Or at least should be...
    /// </summary>
    public static void EnqueueSync(Action work)
    {
        WorkWrapper wrapper = new(work);

        WorkQueue.Enqueue(wrapper);

        while (!wrapper.Finished);
    }

    private class WorkWrapper
    {
        public bool Finished {get; private set;}
        private Action _Work;

        public void Run()
        {
            _Work();
            Finished = true;
        }
        
        public WorkWrapper(Action work)
        {
            _Work = work;
            Finished = false;
        }
    }
}
