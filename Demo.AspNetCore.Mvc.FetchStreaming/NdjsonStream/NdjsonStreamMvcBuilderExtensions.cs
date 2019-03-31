using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Demo.AspNetCore.Mvc.FetchStreaming.NdjsonStream
{
    public static class NdjsonStreamMvcBuilderExtensions
    {
        public static IMvcBuilder AddNdjsonStreamResult(this IMvcBuilder builder)
        {
            builder.Services.TryAddSingleton<INdjsonTextWriterFactory, NdjsonTextWriterFactory>();

            return builder;
        }
    }
}
