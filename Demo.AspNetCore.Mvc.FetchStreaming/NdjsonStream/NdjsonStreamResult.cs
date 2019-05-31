using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.AspNetCore.Mvc.FetchStreaming.NdjsonStream
{
    public class NdjsonStreamResult : ActionResult, IStatusCodeActionResult
    {
        private INdjsonWriter _ndjsonTextWriter;
        private readonly TaskCompletionSource<bool> _readyTaskCompletionSource = new TaskCompletionSource<bool>();
        private readonly TaskCompletionSource<bool> _completeTaskCompletionSource = new TaskCompletionSource<bool>();

        public int? StatusCode { get; set; }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            INdjsonWriterFactory ndjsonTextWriterFactory = context.HttpContext.RequestServices.GetRequiredService<INdjsonWriterFactory>();
            using (_ndjsonTextWriter = ndjsonTextWriterFactory.CreateWriter(context, this))
            {
                _readyTaskCompletionSource.SetResult(true);

                await _completeTaskCompletionSource.Task;
            }
        }

        public async Task WriteAsync(object value)
        {
            if (!_readyTaskCompletionSource.Task.IsCompletedSuccessfully)
            {
                await _readyTaskCompletionSource.Task;
            }

            await _ndjsonTextWriter.WriteAsync(value);
        }

        public void Complete()
        {
            _completeTaskCompletionSource.SetResult(true);
        }
    }
}
