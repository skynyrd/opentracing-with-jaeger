using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;

namespace JaegerWrapper
{
    public class TraceBuilder : ITraceBuilder
    {
        private readonly ITracer _tracer;
        private string _spanName = "default-span";
        private readonly IDictionary<StringTag, string> _tags = new Dictionary<StringTag, string>();
        private readonly IList<IDictionary<string, object>> _logs = new List<IDictionary<string, object>>();
        private bool _willCallService;
        private HttpClient _httpClient;

        public TraceBuilder(ITracer tracer)
        {
            _tracer = tracer;
        }

        public ITracer GetInnerTracer()
        {
            return _tracer;
        }

        public TraceBuilder WithSpanName(string name)
        {
            _spanName = name;
            return this;
        }

        public TraceBuilder WithTag(StringTag key, string value)
        {
            _tags.Add(key, value);
            return this;
        }

        public TraceBuilder WithLog(IDictionary<string, object> log)
        {
            _logs.Add(log);
            return this;
        }

        public TraceBuilder WithHttpCall(HttpClient client, string url, HttpMethod httpMethod)
        {
            if(!_tags.ContainsKey(Tags.HttpUrl))
                _tags.Add(Tags.HttpUrl, url);
            if(!_tags.ContainsKey(Tags.HttpMethod))
                _tags.Add(Tags.HttpMethod, httpMethod.ToString());
            if(!_tags.ContainsKey(Tags.SpanKind))
                _tags.Add(Tags.SpanKind, Tags.SpanKindClient);

            _willCallService = true;
            _httpClient = client;
            
            return this;
        }
        
        public void TraceIt(Action actualWork)
        {
            using (var scope = _tracer.BuildSpan(_spanName).StartActive(true))
            {
                AddTagsAndLogsIfAny(scope);
                InjectIfCallingService(scope);
                
                actualWork();
            }
        }

        public T TraceIt<T>(Func<T> actualWork)
        {
            using (var scope = _tracer.BuildSpan(_spanName).StartActive(true))
            {
                AddTagsAndLogsIfAny(scope);
                InjectIfCallingService(scope);
                return actualWork();
            }
        }

        private void InjectIfCallingService(IScope scope)
        {
            if (_willCallService)
            {
                var dictionary = new Dictionary<string, string>();
                _tracer.Inject(scope.Span.Context, BuiltinFormats.HttpHeaders, new TextMapInjectAdapter(dictionary));
                foreach (var entry in dictionary)
                    _httpClient.DefaultRequestHeaders.Add(entry.Key, entry.Value);
            }

            _willCallService = false;
        }

        private void AddTagsAndLogsIfAny(IScope scope)
        {
            if (_tags.Any())
            {
                foreach (var tagKeyAndValue in _tags)
                {
                    scope.Span.SetTag(tagKeyAndValue.Key, tagKeyAndValue.Value);
                }
            }

            if (_logs.Any())
            {
                foreach (var logDictionary in _logs)
                {
                    scope.Span.Log(logDictionary);
                }
            }
        }
    }}