using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Demo.AspNetCore.Mvc.FetchStreaming.NdjsonStream
{
    public static class NdjsonStreamMvcBuilderExtensions
    {
        public static IMvcBuilder AddNdjsonStreamResult(this IMvcBuilder builder)
        {
            builder.Services.TryAddSingleton<INdjsonWriterFactory, NdjsonWriterFactory>();

            return builder;
        }

        public static IMvcBuilder AddNewtonsoftNdjsonStreamResult(this IMvcBuilder builder)
        {
            builder.Services.TryAddSingleton<INdjsonWriterFactory, NewtonsoftNdjsonWriterFactory>();

            return builder;
        }
    }
}
