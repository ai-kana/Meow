using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;

namespace Kronstadt.Core.Workers;

public class CommandQueue : MonoBehaviour
{
    private static readonly ConcurrentQueue<IWork> _Work = new();
    private static readonly ConcurrentQueue<IEnumerator> _RoutineWork = new();
    private static readonly ConcurrentQueue<IEnumerator> _RoutineCancel = new();

    public static void Enqueue(IWork work)
    {
        _Work.Enqueue(work);
    }

    public static void EnqueueCoroutine(IEnumerator work)
    {
        _RoutineWork.Enqueue(work);
    }

    public static void CancelCoroutine(IEnumerator work)
    {
        _RoutineCancel.Enqueue(work);
    }

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if (_Work.TryDequeue(out IWork work))
        {
            work.Start();
        }

        if (_RoutineWork.TryDequeue(out IEnumerator routine))
        {
            StartCoroutine(routine);
        }

        if (_RoutineCancel.TryDequeue(out routine))
        {
            StopCoroutine(routine);
        }
    }
}
