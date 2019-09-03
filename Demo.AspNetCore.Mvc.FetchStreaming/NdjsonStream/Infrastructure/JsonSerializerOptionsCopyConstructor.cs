using System.Text.Json;
using System.Text.Encodings.Web;

namespace Demo.AspNetCore.Mvc.FetchStreaming.NdjsonStream.Infrastructure
{
    internal static class JsonSerializerOptionsCopyConstructor
    {
        public static JsonSerializerOptions Copy(this JsonSerializerOptions serializerOptions, JavaScriptEncoder encoder)
        {
            var copiedOptions = new JsonSerializerOptions
            {
                AllowTrailingCommas = serializerOptions.AllowTrailingCommas,
                DefaultBufferSize = serializerOptions.DefaultBufferSize,
                DictionaryKeyPolicy = serializerOptions.DictionaryKeyPolicy,
                IgnoreNullValues = serializerOptions.IgnoreNullValues,
                IgnoreReadOnlyProperties = serializerOptions.IgnoreReadOnlyProperties,
                MaxDepth = serializerOptions.MaxDepth,
                PropertyNameCaseInsensitive = serializerOptions.PropertyNameCaseInsensitive,
                PropertyNamingPolicy = serializerOptions.PropertyNamingPolicy,
                ReadCommentHandling = serializerOptions.ReadCommentHandling,
                WriteIndented = serializerOptions.WriteIndented
            };

            for (var i = 0; i < serializerOptions.Converters.Count; i++)
            {
                copiedOptions.Converters.Add(serializerOptions.Converters[i]);
            }

            copiedOptions.Encoder = encoder;

            return copiedOptions;
        }
    }
}
