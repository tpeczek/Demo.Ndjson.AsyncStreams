using System;
using System.Threading.Tasks;

namespace Demo.AspNetCore.Mvc.FetchStreaming.NdjsonStream
{
    internal interface INdjsonTextWriter : IDisposable
    {
        Task WriteAsync(object value);
    }
}
