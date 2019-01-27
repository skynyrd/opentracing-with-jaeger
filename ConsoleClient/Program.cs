using System;
using System.Net.Http;
using Jaeger;
using Jaeger.Samplers;
using JaegerWrapper;
using Microsoft.Extensions.Logging;

namespace ConsoleClient
{
    class Program
    {
        private static int _testId = 12;
        
        static void Inner(Tracer tracer)
        {
            var traceBuilder = new TraceBuilder(tracer);
            
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:7333")
            };
            
            var url = $"/aworld/id/{_testId}";


            traceBuilder.WithSpanName("MainWork")
                .WithHttpCall(client, url, HttpMethod.Get)
                .TraceIt(() =>
                {
                    var response = client.GetAsync(url).Result;

                    if (!response.IsSuccessStatusCode)
                        throw new Exception("uncovered area for the demo.");

                    var responseBody = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(responseBody);
                });
        }

        static void Main(string[] args)
        {
            using (var loggerFactory = new LoggerFactory())
            {
                using (var tracer = InitTracer("ConsoleClient", loggerFactory))
                {
                    Inner(tracer);
                }
            }
        }
        
        private static Tracer InitTracer(string serviceName, ILoggerFactory loggerFactory)
        {
            var samplerConfiguration = new Configuration.SamplerConfiguration(loggerFactory)
                .WithType(ConstSampler.Type)
                .WithParam(1);

            var reporterConfiguration = new Configuration.ReporterConfiguration(loggerFactory)
                .WithLogSpans(true);

            return (Tracer)new Configuration(serviceName, loggerFactory)
                .WithSampler(samplerConfiguration)
                .WithReporter(reporterConfiguration)
                .GetTracer();
        }
    }
}