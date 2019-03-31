using Microsoft.AspNetCore.Mvc;

namespace Demo.AspNetCore.Mvc.FetchStreaming.NdjsonStream
{
    internal interface INdjsonTextWriterFactory
    {
        INdjsonTextWriter CreateWriter(ActionContext context, NdjsonStreamResult result);
    }
}
