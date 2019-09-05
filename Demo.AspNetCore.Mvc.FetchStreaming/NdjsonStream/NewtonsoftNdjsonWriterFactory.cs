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
using Newtonsoft.Json;
using Lib.AspNetCore.Mvc.Ndjson;

namespace Demo.AspNetCore.Mvc.FetchStreaming.NdjsonStream
{
    internal class NewtonsoftNdjsonWriterFactory : INdjsonWriterFactory
    {
        private class NewtonsoftNdjsonArrayPool : IArrayPool<char>
        {
            private readonly ArrayPool<char> _inner;

            public NewtonsoftNdjsonArrayPool(ArrayPool<char> inner)
            {
                if (inner == null)
                {
                    throw new ArgumentNullException(nameof(inner));
                }

                _inner = inner;
            }

            public char[] Rent(int minimumLength)
            {
                return _inner.Rent(minimumLength);
            }

            public void Return(char[] array)
            {
                if (array == null)
                {
                    throw new ArgumentNullException(nameof(array));
                }

                _inner.Return(array);
            }
        }

        private class NewtonsoftNdjsonWriter : INdjsonWriter
        {
            private readonly TextWriter _textResponseStreamWriter;
            private readonly JsonTextWriter _jsonResponseStreamWriter;
            private readonly JsonSerializer _jsonSerializer;

            public NewtonsoftNdjsonWriter(TextWriter textResponseStreamWriter, JsonSerializerSettings jsonSerializerSettings, NewtonsoftNdjsonArrayPool jsonArrayPool)
            {
                _textResponseStreamWriter = textResponseStreamWriter;

                _jsonResponseStreamWriter = new JsonTextWriter(textResponseStreamWriter)
                {
                    ArrayPool = jsonArrayPool,
                    CloseOutput = false,
                    AutoCompleteOnClose = false
                };

                _jsonSerializer = JsonSerializer.Create(jsonSerializerSettings);
            }

            public async Task WriteAsync(object value)
            {
                _jsonSerializer.Serialize(_jsonResponseStreamWriter, value);
                await _textResponseStreamWriter.WriteAsync("\n");
                await _textResponseStreamWriter.FlushAsync();
            }

            public void Dispose()
            {
                _textResponseStreamWriter?.Dispose();
                ((IDisposable)_jsonResponseStreamWriter)?.Dispose();
            }
        }

        private static readonly string CONTENT_TYPE = new MediaTypeHeaderValue("application/x-ndjson")
        {
            Encoding = Encoding.UTF8
        }.ToString();

        private readonly IHttpResponseStreamWriterFactory _httpResponseStreamWriterFactory;
        private readonly MvcNewtonsoftJsonOptions _options;
        private readonly NewtonsoftNdjsonArrayPool _jsonArrayPool;

        public NewtonsoftNdjsonWriterFactory(IHttpResponseStreamWriterFactory httpResponseStreamWriterFactory, IOptions<MvcNewtonsoftJsonOptions> options, ArrayPool<char> innerJsonArrayPool)
        {
            _httpResponseStreamWriterFactory = httpResponseStreamWriterFactory ?? throw new ArgumentNullException(nameof(httpResponseStreamWriterFactory));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            if (innerJsonArrayPool == null)
            {
                throw new ArgumentNullException(nameof(innerJsonArrayPool));
            }

            _jsonArrayPool = new NewtonsoftNdjsonArrayPool(innerJsonArrayPool);
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

            return new NewtonsoftNdjsonWriter(_httpResponseStreamWriterFactory.CreateWriter(response.Body, Encoding.UTF8), _options.SerializerSettings, _jsonArrayPool);
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
