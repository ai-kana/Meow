namespace Kronstadt.Core.Workers;

public class Work : IWork
{
    private readonly Action _Work;
    public Work(Action work)
    {
        _Work = work;
    }

    public void Start()
    {
        _Work.Invoke();
    }
}

public class Work<T1> : IWork
{
    private readonly Action<T1> _Work;
    private readonly T1 _Arg1;
    public Work(Action<T1> work, T1 arg1)
    {
        _Work = work;
        _Arg1 = arg1;
    }

    public void Start()
    {
        _Work.Invoke(_Arg1);
    }
}

public class Work<T1, T2> : IWork
{
    private readonly Action<T1, T2> _Work;
    private readonly T1 _Arg1;
    private readonly T2 _Arg2;
    public Work(Action<T1, T2> work, T1 arg1, T2 arg2)
    {
        _Work = work;
        _Arg1 = arg1;
        _Arg2 = arg2;
    }

    public void Start()
    {
        _Work.Invoke(_Arg1, _Arg2);
    }
}

public class Work<T1, T2, T3> : IWork
{
    private readonly Action<T1, T2, T3> _Work;
    private readonly T1 _Arg1;
    private readonly T2 _Arg2;
    private readonly T3 _Arg3;
    public Work(Action<T1, T2, T3> work, T1 arg1, T2 arg2, T3 arg3)
    {
        _Work = work;
        _Arg1 = arg1;
        _Arg2 = arg2;
        _Arg3 = arg3;
    }

    public void Start()
    {
        _Work.Invoke(_Arg1, _Arg2, _Arg3);
    }
}

public class Work<T1, T2, T3, T4> : IWork
{
    private readonly Action<T1, T2, T3, T4> _Work;
    private readonly T1 _Arg1;
    private readonly T2 _Arg2;
    private readonly T3 _Arg3;
    private readonly T4 _Arg4;
    public Work(Action<T1, T2, T3, T4> work, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        _Work = work;
        _Arg1 = arg1;
        _Arg2 = arg2;
        _Arg3 = arg3;
        _Arg4 = arg4;
    }

    public void Start()
    {
        _Work.Invoke(_Arg1, _Arg2, _Arg3, _Arg4);
    }
}

public class Work<T1, T2, T3, T4, T5> : IWork
{
    private readonly Action<T1, T2, T3, T4, T5> _Work;
    private readonly T1 _Arg1;
    private readonly T2 _Arg2;
    private readonly T3 _Arg3;
    private readonly T4 _Arg4;
    private readonly T5 _Arg5;
    public Work(Action<T1, T2, T3, T4, T5> work, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        _Work = work;
        _Arg1 = arg1;
        _Arg2 = arg2;
        _Arg3 = arg3;
        _Arg4 = arg4;
        _Arg5 = arg5;
    }

    public void Start()
    {
        _Work.Invoke(_Arg1, _Arg2, _Arg3, _Arg4, _Arg5);
    }
}

public class Work<T1, T2, T3, T4, T5, T6> : IWork
{
    private readonly Action<T1, T2, T3, T4, T5, T6> _Work;
    private readonly T1 _Arg1;
    private readonly T2 _Arg2;
    private readonly T3 _Arg3;
    private readonly T4 _Arg4;
    private readonly T5 _Arg5;
    private readonly T6 _Arg6;
    public Work(Action<T1, T2, T3, T4, T5, T6> work, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        _Work = work;
        _Arg1 = arg1;
        _Arg2 = arg2;
        _Arg3 = arg3;
        _Arg4 = arg4;
        _Arg5 = arg5;
        _Arg6 = arg6;
    }

    public void Start()
    {
        _Work.Invoke(_Arg1, _Arg2, _Arg3, _Arg4, _Arg5, _Arg6);
    }
}

public class Work<T1, T2, T3, T4, T5, T6, T7> : IWork
{
    private readonly Action<T1, T2, T3, T4, T5, T6, T7> _Work;
    private readonly T1 _Arg1;
    private readonly T2 _Arg2;
    private readonly T3 _Arg3;
    private readonly T4 _Arg4;
    private readonly T5 _Arg5;
    private readonly T6 _Arg6;
    private readonly T7 _Arg7;
    public Work(Action<T1, T2, T3, T4, T5, T6, T7> work, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        _Work = work;
        _Arg1 = arg1;
        _Arg2 = arg2;
        _Arg3 = arg3;
        _Arg4 = arg4;
        _Arg5 = arg5;
        _Arg6 = arg6;
        _Arg7 = arg7;
    }

    public void Start()
    {
        _Work.Invoke(_Arg1, _Arg2, _Arg3, _Arg4, _Arg5, _Arg6, _Arg7);
    }
}

public class Work<T1, T2, T3, T4, T5, T6, T7, T8> : IWork
{
    private readonly Action<T1, T2, T3, T4, T5, T6, T7, T8> _Work;
    private readonly T1 _Arg1;
    private readonly T2 _Arg2;
    private readonly T3 _Arg3;
    private readonly T4 _Arg4;
    private readonly T5 _Arg5;
    private readonly T6 _Arg6;
    private readonly T7 _Arg7;
    private readonly T8 _Arg8;
    public Work(Action<T1, T2, T3, T4, T5, T6, T7, T8> work, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        _Work = work;
        _Arg1 = arg1;
        _Arg2 = arg2;
        _Arg3 = arg3;
        _Arg4 = arg4;
        _Arg5 = arg5;
        _Arg6 = arg6;
        _Arg7 = arg7;
        _Arg8 = arg8;
    }

    public void Start()
    {
        _Work.Invoke(_Arg1, _Arg2, _Arg3, _Arg4, _Arg5, _Arg6, _Arg7, _Arg8);
    }
}
