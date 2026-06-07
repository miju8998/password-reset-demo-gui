using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PasswordBruteForceGUI
{
    public class BruteForceWorker
    {
        private readonly CandidateGenerator generator;
        private readonly BruteForceValidator validator;

        public BruteForceWorker(CandidateGenerator generator, BruteForceValidator validator)
        {
            this.generator = generator;
            this.validator = validator;
        }

        public Task<AttackResult> RunSingleThreadAsync(
            int maxLength,
            CancellationToken token,
            Action<int, long> progressCallback)
        {
            return Task.Run(() =>
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                long attempts = 0;
                long totalCandidates = generator.TotalCandidates(maxLength);
                string foundPassword = null;
                bool stopped = false;

                for (int length = 1; length <= maxLength; length++)
                {
                    long count = generator.CountForLength(length);

                    for (long i = 0; i < count; i++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            stopped = true;
                            break;
                        }

                        string candidate = generator.GetCandidate(i, length);
                        attempts++;

                        if (validator.IsValid(candidate))
                        {
                            foundPassword = candidate;
                            break;
                        }

                        if (attempts % 200 == 0)
                        {
                            int percent = (int)Math.Min(100, (attempts * 100) / totalCandidates);
                            progressCallback?.Invoke(percent, attempts);
                        }
                    }

                    if (foundPassword != null || stopped)
                    {
                        break;
                    }
                }

                stopwatch.Stop();

                progressCallback?.Invoke(foundPassword != null ? 100 : 0, attempts);

                return new AttackResult
                {
                    Mode = "Single-thread",
                    FoundPassword = foundPassword,
                    Attempts = attempts,
                    ElapsedTime = stopwatch.Elapsed,
                    ThreadsUsed = 1,
                    Stopped = stopped
                };
            });
        }

        public Task<AttackResult> RunMultiThreadAsync(
            int maxLength,
            CancellationToken externalToken,
            Action<int, long> progressCallback)
        {
            return Task.Run(() =>
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                int threadCount = Math.Max(1, Environment.ProcessorCount - 1);
                long attempts = 0;
                long totalCandidates = generator.TotalCandidates(maxLength);
                string foundPassword = null;
                bool stopped = false;

                using (CancellationTokenSource linkedCts =
                    CancellationTokenSource.CreateLinkedTokenSource(externalToken))
                {
                    CancellationToken token = linkedCts.Token;

                    for (int length = 1; length <= maxLength; length++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            stopped = externalToken.IsCancellationRequested;
                            break;
                        }

                        long count = generator.CountForLength(length);
                        List<Task> tasks = new List<Task>();

                        for (int workerId = 0; workerId < threadCount; workerId++)
                        {
                            int localWorkerId = workerId;
                            int localLength = length;
                            long localCount = count;

                            tasks.Add(Task.Run(() =>
                            {
                                for (long i = localWorkerId; i < localCount; i += threadCount)
                                {
                                    if (token.IsCancellationRequested)
                                    {
                                        break;
                                    }

                                    string candidate = generator.GetCandidate(i, localLength);
                                    long currentAttempts = Interlocked.Increment(ref attempts);

                                    if (validator.IsValid(candidate))
                                    {
                                        if (Interlocked.CompareExchange(
                                            ref foundPassword,
                                            candidate,
                                            null) == null)
                                        {
                                            linkedCts.Cancel();
                                        }

                                        break;
                                    }

                                    if (currentAttempts % 500 == 0)
                                    {
                                        int percent = (int)Math.Min(100, (currentAttempts * 100) / totalCandidates);
                                        progressCallback?.Invoke(percent, currentAttempts);
                                    }
                                }
                            }));
                        }

                        Task.WaitAll(tasks.ToArray());

                        if (foundPassword != null)
                        {
                            break;
                        }
                    }
                }

                if (externalToken.IsCancellationRequested && foundPassword == null)
                {
                    stopped = true;
                }

                stopwatch.Stop();

                progressCallback?.Invoke(foundPassword != null ? 100 : 0, attempts);

                return new AttackResult
                {
                    Mode = "Multi-thread",
                    FoundPassword = foundPassword,
                    Attempts = attempts,
                    ElapsedTime = stopwatch.Elapsed,
                    ThreadsUsed = Math.Max(1, Environment.ProcessorCount - 1),
                    Stopped = stopped
                };
            });
        }
    }
}