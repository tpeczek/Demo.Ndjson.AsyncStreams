using System;
using System.Threading.Tasks;

namespace Demo.AspNetCore.Mvc.FetchStreaming.NdjsonStream
{
    internal interface INdjsonWriter : IDisposable
    {
        Task WriteAsync(object value);
    }
}
