using Microsoft.AspNetCore.Mvc;

namespace Demo.AspNetCore.Mvc.FetchStreaming.NdjsonStream
{
    internal interface INdjsonWriterFactory
    {
        INdjsonWriter CreateWriter(ActionContext context, NdjsonStreamResult result);
    }
}
