/********************************************************
*                                                        *
*   © Copyright (C) Microsoft. All rights reserved.      *
*                                                        *
*********************************************************/

namespace Microsoft.VisualStudio.Threading
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Utility methods for working across threads.
    /// </summary>
    public static class ThreadingTools
    {
        /// <summary>
        /// Wraps a task with one that will complete as cancelled based on a cancellation token,
        /// allowing someone to await a task but be able to break out early by cancelling the token.
        /// </summary>
        /// <typeparam name="T">The type of value returned by the task.</typeparam>
        /// <param name="task">The task to wrap.</param>
        /// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
        /// <returns>The wrapping task.</returns>
        public static Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            if (task == null)
                throw new NullReferenceException();

            if (!cancellationToken.CanBeCanceled || task.IsCompleted)
            {
                return task;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return TaskFromCanceled<T>(cancellationToken);
            }

            return WithCancellationSlow(task, cancellationToken);
        }

        /// <summary>
        /// Wraps a task with one that will complete as cancelled based on a cancellation token,
        /// allowing someone to await a task but be able to break out early by cancelling the token.
        /// </summary>
        /// <param name="task">The task to wrap.</param>
        /// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
        /// <returns>The wrapping task.</returns>
        public static Task WithCancellation(this Task task, CancellationToken cancellationToken)
        {
            if (task == null)
                throw new NullReferenceException();

            if (!cancellationToken.CanBeCanceled || task.IsCompleted)
            {
                return task;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return TaskFromCanceled(cancellationToken);
            }

            return WithCancellationSlow(task, continueOnCapturedContext: false, cancellationToken: cancellationToken);
        }

        internal static Task WithCancellation(this Task task, bool continueOnCapturedContext, CancellationToken cancellationToken)
        {
            if (task == null)
                throw new NullReferenceException();

            if (!cancellationToken.CanBeCanceled || task.IsCompleted)
            {
                return task;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return TaskFromCanceled(cancellationToken);
            }

            return WithCancellationSlow(task, continueOnCapturedContext, cancellationToken);
        }

#if !TRYSETCANCELEDCT
        internal static bool TrySetCanceled<T>(this TaskCompletionSource<T> tcs, CancellationToken cancellationToken)
        {
            return LightUps<T>.TrySetCanceled != null
                ? LightUps<T>.TrySetCanceled(tcs, cancellationToken)
                : tcs.TrySetCanceled();
        }
#endif

        internal static Task TaskFromCanceled(CancellationToken cancellationToken)
        {
            return TaskFromCanceled<EmptyStruct>(cancellationToken);
        }

        internal static Task<T> TaskFromCanceled<T>(CancellationToken cancellationToken)
        {
#if TRYSETCANCELEDCT
            return Task.FromCanceled<T>(cancellationToken);
#else
            var tcs = new TaskCompletionSource<T>();
            tcs.TrySetCanceled(cancellationToken);
            return tcs.Task;
#endif
        }

        internal static Task TaskFromException(Exception exception)
        {
            return TaskFromException<EmptyStruct>(exception);
        }

        internal static Task<T> TaskFromException<T>(Exception exception)
        {
#if TRYSETCANCELEDCT
            return Task.FromException<T>(exception);
#else
            var tcs = new TaskCompletionSource<T>();
            tcs.TrySetException(exception);
            return tcs.Task;
#endif
        }

        /// <summary>
        /// Cancels a <see cref="TaskCompletionSource{TResult}.Task"/> if a given <see cref="CancellationToken"/> is canceled.
        /// </summary>
        /// <typeparam name="T">The type of value returned by a successfully completed <see cref="Task{TResult}"/>.</typeparam>
        /// <param name="taskCompletionSource">The <see cref="TaskCompletionSource{TResult}"/> to cancel.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <param name="cancellationCallback">A callback to invoke when cancellation occurs.</param>
        internal static void AttachCancellation<T>(this TaskCompletionSource<T> taskCompletionSource, CancellationToken cancellationToken, ICancellationNotification cancellationCallback = null)
        {
            if (taskCompletionSource == null)
                throw new NullReferenceException();

            if (cancellationToken.CanBeCanceled)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    taskCompletionSource.TrySetCanceled(cancellationToken);
                }
                else
                {
                    var tuple = new CancelableTaskCompletionSource<T>(taskCompletionSource, cancellationToken, cancellationCallback);
                    tuple.CancellationTokenRegistration = cancellationToken.Register(
                        s =>
                        {
                            var t = (CancelableTaskCompletionSource<T>)s;
                            t.TaskCompletionSource.TrySetCanceled(t.CancellationToken);
                            t.CancellationCallback?.OnCanceled();
                        },
                        tuple,
                        useSynchronizationContext: false);
                    taskCompletionSource.Task.ContinueWith(
                        (_, s) =>
                        {
                            var t = (CancelableTaskCompletionSource<T>)s;
                            t.CancellationTokenRegistration.Dispose();
                        },
                        tuple,
                        CancellationToken.None,
                        TaskContinuationOptions.ExecuteSynchronously,
                        TaskScheduler.Default);
                }
            }
        }

        /// <summary>
        /// Wraps a task with one that will complete as cancelled based on a cancellation token,
        /// allowing someone to await a task but be able to break out early by cancelling the token.
        /// </summary>
        /// <typeparam name="T">The type of value returned by the task.</typeparam>
        /// <param name="task">The task to wrap.</param>
        /// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
        /// <returns>The wrapping task.</returns>
        private static async Task<T> WithCancellationSlow<T>(Task<T> task, CancellationToken cancellationToken)
        {
            if (task == null)
                throw new NullReferenceException();
            if (!cancellationToken.CanBeCanceled)
                throw new ArgumentException();

            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            // Rethrow any fault/cancellation exception, even if we awaited above.
            // But if we skipped the above if branch, this will actually yield
            // on an incompleted task.
            return await task.ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps a task with one that will complete as cancelled based on a cancellation token,
        /// allowing someone to await a task but be able to break out early by cancelling the token.
        /// </summary>
        /// <param name="task">The task to wrap.</param>
        /// <param name="continueOnCapturedContext">A value indicating whether *internal* continuations required to respond to cancellation should run on the current <see cref="SynchronizationContext"/>.</param>
        /// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
        /// <returns>The wrapping task.</returns>
        private static async Task WithCancellationSlow(this Task task, bool continueOnCapturedContext, CancellationToken cancellationToken)
        {
            if (task == null)
                throw new NullReferenceException();
            if (!cancellationToken.CanBeCanceled)
                throw new ArgumentException();

            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(continueOnCapturedContext))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            // Rethrow any fault/cancellation exception, even if we awaited above.
            // But if we skipped the above if branch, this will actually yield
            // on an incompleted task.
            await task.ConfigureAwait(continueOnCapturedContext);
        }

        internal interface ICancellationNotification
        {
            void OnCanceled();
        }

        /// <summary>
        /// A state object for tracking cancellation and a TaskCompletionSource.
        /// </summary>
        /// <typeparam name="T">The type of value returned from a task.</typeparam>
        /// <remarks>
        /// We use this class so that we only allocate one object to support all continuations
        /// required for cancellation handling, rather than a special closure and delegate for each one.
        /// </remarks>
        private class CancelableTaskCompletionSource<T>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CancelableTaskCompletionSource{T}"/> class.
            /// </summary>
            /// <param name="taskCompletionSource">The task completion source.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <param name="cancellationCallback">A callback to invoke when cancellation occurs.</param>
            internal CancelableTaskCompletionSource(TaskCompletionSource<T> taskCompletionSource, CancellationToken cancellationToken, ICancellationNotification cancellationCallback)
            {
                this.TaskCompletionSource = taskCompletionSource ?? throw new ArgumentNullException(nameof(taskCompletionSource));
                this.CancellationToken = cancellationToken;
                this.CancellationCallback = cancellationCallback;
            }

            /// <summary>
            /// Gets the cancellation token.
            /// </summary>
            internal CancellationToken CancellationToken { get; }

            /// <summary>
            /// Gets the Task completion source.
            /// </summary>
            internal TaskCompletionSource<T> TaskCompletionSource { get; }

            internal ICancellationNotification CancellationCallback { get; }

            /// <summary>
            /// Gets or sets the cancellation token registration.
            /// </summary>
            internal CancellationTokenRegistration CancellationTokenRegistration { get; set; }
        }
    }
}
