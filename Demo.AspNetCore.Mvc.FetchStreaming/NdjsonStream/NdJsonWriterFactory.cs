using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Demo.AspNetCore.Mvc.FetchStreaming.NdjsonStream.Infrastructure;

namespace Demo.AspNetCore.Mvc.FetchStreaming.NdjsonStream
{
    internal class NdjsonWriterFactory : INdjsonWriterFactory
    {
        private class NdjsonWriter : INdjsonWriter
        {
            private static byte[] _newlineDelimiter = Encoding.UTF8.GetBytes("\n");

            private readonly Stream _writeStream;
            private readonly JsonSerializerOptions _jsonSerializerOptions;

            public NdjsonWriter(Stream writeStream, JsonOptions jsonOptions)
            {
                _writeStream = writeStream;

                _jsonSerializerOptions = jsonOptions.JsonSerializerOptions;
                if (_jsonSerializerOptions.Encoder is null)
                {
                    _jsonSerializerOptions = _jsonSerializerOptions.Copy(JavaScriptEncoder.UnsafeRelaxedJsonEscaping);
                }
            }

            public async Task WriteAsync(object value)
            {
                Type valueType = value?.GetType() ?? typeof(object);

                await JsonSerializer.SerializeAsync(_writeStream, value, valueType, _jsonSerializerOptions);
                await _writeStream.WriteAsync(_newlineDelimiter);
                await _writeStream.FlushAsync();
            }

            public void Dispose()
            { }
        }

        private static readonly string CONTENT_TYPE = new MediaTypeHeaderValue("application/x-ndjson")
        {
            Encoding = Encoding.UTF8
        }.ToString();

        private readonly JsonOptions _options;

        public NdjsonWriterFactory(IOptions<JsonOptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public INdjsonWriter CreateWriter(ActionContext context, NdjsonStreamResult result)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            HttpResponse response = context.HttpContext.Response;

            response.ContentType = CONTENT_TYPE;

            if (result.StatusCode != null)
            {
                response.StatusCode = result.StatusCode.Value;
            }

            DisableResponseBuffering(context.HttpContext);

            return new NdjsonWriter(response.Body, _options);
        }

        private static void DisableResponseBuffering(HttpContext context)
        {
            IHttpResponseBodyFeature responseBodyFeature = context.Features.Get<IHttpResponseBodyFeature>();
            if (responseBodyFeature != null)
            {
                responseBodyFeature.DisableBuffering();
            }
        }
    }
}
