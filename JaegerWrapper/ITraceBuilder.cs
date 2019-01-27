using System;
using System.Collections.Generic;
using OpenTracing;
using OpenTracing.Tag;

namespace JaegerWrapper
{
    public interface ITraceBuilder
    {
        TraceBuilder WithSpanName(string name);
        TraceBuilder WithTag(StringTag key, string value);
        TraceBuilder WithLog(IDictionary<string, object> log);
        void TraceIt(Action actualWork);
        T TraceIt<T>(Func<T> actualWork);
        ITracer GetInnerTracer();
    }
}