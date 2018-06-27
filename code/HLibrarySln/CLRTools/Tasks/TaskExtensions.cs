using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    public static class TaskExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]  // Causes compiler to optimize the call away
        public static void NoCare(this Task task) { }

        public static async void NoAwait(this Task task)
        {
            await task;
        }
        #region WithCancellation

        private struct Void { } // Because there isn't a non-generic TaskCompletionSource class.

        public static async Task<TResult> WithCancellation<TResult>(this Task<TResult> orignalTask, CancellationToken ct)
        {
            // Create a Task that completes when the CancellationToken is canceled
            var cancelTask = new TaskCompletionSource<Void>();

            // When the CancellationToken is cancelled, complete the Task
            using (ct.Register(t => ((TaskCompletionSource<Void>)t).TrySetResult(new Void()), cancelTask))
            {

                // Create another Task that completes when the original Task or when the CancellationToken's Task
                Task any = await Task.WhenAny(orignalTask, cancelTask.Task);

                // If any Task completes due to CancellationToken, throw OperationCanceledException         
                if (any == cancelTask.Task) ct.ThrowIfCancellationRequested();
            }

            // await original task (synchronously); if it failed, awaiting it 
            // throws 1st inner exception instead of AggregateException
            return await orignalTask;
        }

        public static async Task WithCancellation(this Task task, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<Void>();
            using (ct.Register(t => ((TaskCompletionSource<Void>)t).TrySetResult(default(Void)), tcs))
            {
                if (await Task.WhenAny(task, tcs.Task) == tcs.Task) ct.ThrowIfCancellationRequested();
            }
            await task;          // If failure, ensures 1st inner exception gets thrown instead of AggregateException
        }
        #endregion
    }
}
