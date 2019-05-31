using System;
using System.IO;
using System.Text;
using System.Buffers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace Demo.AspNetCore.Mvc.FetchStreaming.NdjsonStream
{
    //internal class NdjsonTextWriterFactory : INdjsonTextWriterFactory
    //{
    //    private class NdjsonTextWriter : INdjsonTextWriter
    //    {
    //        private readonly TextWriter _textResponseStreamWriter;
    //        private readonly JsonTextWriter _jsonResponseStreamWriter;
    //        private readonly JsonSerializer _jsonSerializer;

    //        public NdjsonTextWriter(TextWriter textResponseStreamWriter, JsonSerializerSettings jsonSerializerSettings, JsonArrayPool<char> jsonArrayPool)
    //        {
    //            _textResponseStreamWriter = textResponseStreamWriter;

    //            _jsonResponseStreamWriter = new JsonTextWriter(textResponseStreamWriter)
    //            {
    //                ArrayPool = jsonArrayPool,
    //                CloseOutput = false,
    //                AutoCompleteOnClose = false
    //            };

    //            _jsonSerializer = JsonSerializer.Create(jsonSerializerSettings);
    //        }

    //        public async Task WriteAsync(object value)
    //        {
    //            _jsonSerializer.Serialize(_jsonResponseStreamWriter, value);
    //            await _textResponseStreamWriter.WriteAsync("\n");
    //            await _textResponseStreamWriter.FlushAsync();
    //        }

    //        public void Dispose()
    //        {
    //            _textResponseStreamWriter?.Dispose();
    //            ((IDisposable)_jsonResponseStreamWriter)?.Dispose();
    //        }
    //    }

    //    private static readonly string DEFAULT_CONTENT_TYPE = new MediaTypeHeaderValue("application/x-ndjson")
    //    {
    //        Encoding = Encoding.UTF8
    //    }.ToString();

    //    private readonly IHttpResponseStreamWriterFactory _httpResponseStreamWriterFactory;
    //    private readonly MvcJsonOptions _options;
    //    private readonly JsonArrayPool<char> _jsonArrayPool;

    //    public NdjsonTextWriterFactory(IHttpResponseStreamWriterFactory httpResponseStreamWriterFactory, IOptions<MvcJsonOptions> options, ArrayPool<char> innerJsonArrayPool)
    //    {
    //        _httpResponseStreamWriterFactory = httpResponseStreamWriterFactory ?? throw new ArgumentNullException(nameof(httpResponseStreamWriterFactory));
    //        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    //        if (innerJsonArrayPool == null)
    //        {
    //            throw new ArgumentNullException(nameof(innerJsonArrayPool));
    //        }
    //        _jsonArrayPool = new JsonArrayPool<char>(innerJsonArrayPool);
    //    }

    //    public INdjsonTextWriter CreateWriter(ActionContext context, NdjsonStreamResult result)
    //    {
    //        if (context == null)
    //        {
    //            throw new ArgumentNullException(nameof(context));
    //        }

    //        if (result == null)
    //        {
    //            throw new ArgumentNullException(nameof(result));
    //        }

    //        HttpResponse response = context.HttpContext.Response;

    //        ResponseContentTypeHelper.ResolveContentTypeAndEncoding(result.ContentType, response.ContentType, DEFAULT_CONTENT_TYPE, out var resolvedContentType, out var resolvedContentTypeEncoding);
    //        response.ContentType = resolvedContentType;

    //        if (result.StatusCode != null)
    //        {
    //            response.StatusCode = result.StatusCode.Value;
    //        }

    //        DisableResponseBuffering(context.HttpContext);

    //        return new NdjsonTextWriter(_httpResponseStreamWriterFactory.CreateWriter(response.Body, resolvedContentTypeEncoding), _options.SerializerSettings, _jsonArrayPool);
    //    }

    //    private static void DisableResponseBuffering(HttpContext context)
    //    {
    //        IHttpBufferingFeature bufferingFeature = context.Features.Get<IHttpBufferingFeature>();
    //        if (bufferingFeature != null)
    //        {
    //            bufferingFeature.DisableResponseBuffering();
    //        }
    //    }
    //}
}
