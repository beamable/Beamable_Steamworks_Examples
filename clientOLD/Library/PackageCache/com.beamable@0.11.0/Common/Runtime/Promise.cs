using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Beamable.Common
{
   public abstract class PromiseBase
   {
      protected Action<Exception> errbacks;
      public bool HadAnyErrbacks { protected set; get; }

      protected Exception err;
      protected object _lock = new object();

      private int _doneSignal = 0; // https://stackoverflow.com/questions/29411961/c-sharp-and-thread-safety-of-a-bool
      protected bool done
      {
         get => (System.Threading.Interlocked.CompareExchange(ref _doneSignal, 1, 1) == 1);
         set
         {
            if (value) System.Threading.Interlocked.CompareExchange(ref _doneSignal, 1, 0);
            else System.Threading.Interlocked.CompareExchange(ref _doneSignal, 0, 1);
         }
      }

      public static readonly Unit Unit = new Unit();

      public static Promise<Unit> SuccessfulUnit => Promise<Unit>.Successful(Unit);

      public bool IsCompleted => done;

      private static PromiseEvent OnPotentialUncaughtError;

      public static void SetPotentialUncaughtErrorHandler(PromiseEvent handler)
      {
         OnPotentialUncaughtError =
            handler; // this overwrites it everytime, blowing away any other listeners. This allows someone to override the functionality.
      }

      protected void InvokeUncaughtPromise()
      {
         OnPotentialUncaughtError?.Invoke(this, err);
      }

   }

   public delegate void PromiseEvent(PromiseBase promise, Exception err);

   /// <summary>
   /// This class defines the %Beamable %Promise.
   ///
   /// A promise is an object that may produce a single value some time in the future:
   /// either a resolved value, or a reason that it’s not resolved (e.g., a network error occurred).
   ///
   /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
   ///
   /// #### Related Links
   /// - None
   ///
   /// ![img beamable-logo]
   ///
   /// </summary>
   public class Promise<T> : PromiseBase, ICriticalNotifyCompletion
   {
      private Action<T> _callbacks;
      private T _val;


      public void CompleteSuccess(T val)
      {
         lock (_lock)
         {
            if (done)
            {
               return;
            }

            _val = val;
            done = true;
            try
            {
               _callbacks?.Invoke(val);
            }
            catch (Exception e)
            {
               BeamableLogger.LogException(e);
            }

            _callbacks = null;
            errbacks = null;
         }
      }

      public void CompleteError(Exception ex)
      {
         lock (_lock)
         {
            if (done)
            {
               return;
            }

            err = ex;
            done = true;

            try
            {
               if (!HadAnyErrbacks)
               {
                  InvokeUncaughtPromise();
               }
               else
               {
                  errbacks?.Invoke(ex);
               }
            }
            catch (Exception e)
            {
               BeamableLogger.LogException(e);
            }

            _callbacks = null;
            errbacks = null;
         }
      }

      public Promise<T> Then(Action<T> callback)
      {
         lock (_lock)
         {
            if (done)
            {
               if (err == null)
               {
                  try
                  {
                     callback(_val);
                  }
                  catch (Exception e)
                  {
                     BeamableLogger.LogException(e);
                  }
               }
            }
            else
            {
               _callbacks += callback;
            }
         }

         return this;
      }

      public Promise<T> Error(Action<Exception> errback)
      {
         lock (_lock)
         {
            HadAnyErrbacks = true;
            if (done)
            {
               if (err != null)
               {
                  try
                  {
                     errback(err);
                  }
                  catch (Exception e)
                  {
                     BeamableLogger.LogException(e);
                  }
               }
            }
            else
            {
               errbacks += errback;
            }
         }

         return this;
      }

      public Promise<TU> Map<TU>(Func<T, TU> callback)
      {
         var result = new Promise<TU>();
         Then(value =>
            {
               try
               {
                  var nextResult = callback(value);
                  result.CompleteSuccess(nextResult);
               }
               catch (Exception ex)
               {
                  result.CompleteError(ex);
               }
            })
            .Error(ex => result.CompleteError(ex));
         return result;
      }

      public PromiseU FlatMap<PromiseU, U>(Func<T, PromiseU> callback, Func<PromiseU> factory)
         where PromiseU : Promise<U>
      {
         var pu = factory();
         FlatMap(callback)
            .Then(pu.CompleteSuccess)
            .Error(pu.CompleteError);
         return pu;
      }

      public Promise<TU> FlatMap<TU>(Func<T, Promise<TU>> callback)
      {
         var result = new Promise<TU>();
         Then(value =>
         {
            try
            {
               callback(value)
                  .Then(valueInner => result.CompleteSuccess(valueInner))
                  .Error(ex => result.CompleteError(ex));
            }
            catch (Exception ex)
            {
               result.CompleteError(ex);
            }
         }).Error(ex => { result.CompleteError(ex); });
         return result;
      }

      public static Promise<T> Successful(T value)
      {
         return new Promise<T>
         {
            done = true,
            _val = value
         };
      }

      public static Promise<T> Failed(Exception err)
      {
         return new Promise<T>
         {
            done = true,
            err = err
         };
      }

      void ICriticalNotifyCompletion.UnsafeOnCompleted(Action continuation)
      {
         Then(_ => continuation());
         Error(_ => continuation());
      }

      void INotifyCompletion.OnCompleted(Action continuation)
      {
         ((ICriticalNotifyCompletion) this).UnsafeOnCompleted(continuation);
      }


      public T GetResult()
      {
         if (err != null)
            throw err;
         return _val;
      }

      public Promise<T> GetAwaiter()
      {
         return this;
      }
   }

   public class SequencePromise<T> : Promise<IList<T>>
   {
      private Action<SequenceEntryException> _entryErrorCallbacks;
      private Action<SequenceEntrySuccess<T>> _entrySuccessCallbacks;

      private ConcurrentBag<SequenceEntryException> _errors = new ConcurrentBag<SequenceEntryException>();
      private ConcurrentBag<SequenceEntrySuccess<T>> _successes = new ConcurrentBag<SequenceEntrySuccess<T>>();

      private ConcurrentDictionary<int, object> _indexToResult = new ConcurrentDictionary<int, object>();

      public int SuccessCount => _successes.Count;
      public int ErrorCount => _errors.Count;
      public int Total => _errors.Count + _successes.Count;
      public int Count { get; }

      public float Ratio => HasProcessedAllEntries ? 1 : Total / (float) Count;
      public bool HasProcessedAllEntries => Total == Count;

      public IEnumerable<T> SuccessfulResults => _successes.Select(s => s.Result);

      public SequencePromise(int count)
      {
         Count = count;
         if (Count == 0)
         {
            CompleteSuccess();
         }
      }

      public SequencePromise<T> OnElementError(Action<SequenceEntryException> handler)
      {
         foreach (var existingError in _errors)
         {
            handler?.Invoke(existingError);
         }

         _entryErrorCallbacks += handler;
         return this;
      }

      public SequencePromise<T> OnElementSuccess(Action<SequenceEntrySuccess<T>> handler)
      {
         foreach (var success in _successes)
         {
            handler?.Invoke(success);
         }

         _entrySuccessCallbacks += handler;
         return this;
      }

      public void CompleteSuccess()
      {
         base.CompleteSuccess(SuccessfulResults.ToList());
      }

      public void ReportEntryError(SequenceEntryException exception)
      {
         if (_indexToResult.ContainsKey(exception.Index) || exception.Index >= Count) return;

         _errors.Add(exception);
         _indexToResult.TryAdd(exception.Index, exception);
         _entryErrorCallbacks?.Invoke(exception);

         CompleteError(exception.InnerException);
      }

      public void ReportEntrySuccess(SequenceEntrySuccess<T> success)
      {
         if (_indexToResult.ContainsKey(success.Index) || success.Index >= Count) return;

         _successes.Add(success);
         _indexToResult.TryAdd(success.Index, success);
         _entrySuccessCallbacks?.Invoke(success);

         if (HasProcessedAllEntries)
         {
            CompleteSuccess();
         }
      }

      public void ReportEntrySuccess(int index, T result) =>
         ReportEntrySuccess(new SequenceEntrySuccess<T>(index, result));

      public void ReportEntryError(int index, Exception err) =>
         ReportEntryError(new SequenceEntryException(index, err));
   }

   public static class Promise
   {

      public static SequencePromise<T> ObservableSequence<T>(IList<Promise<T>> promises)
      {
         var result = new SequencePromise<T>(promises.Count);

         if (promises == null || promises.Count == 0)
         {
            result.CompleteSuccess();
            return result;
         }

         for (var i = 0; i < promises.Count; i++)
         {
            var index = i;
            promises[i].Then(reply =>
            {
               result.ReportEntrySuccess(new SequenceEntrySuccess<T>(index, reply));

               if (result.Total == promises.Count)
               {
                  result.CompleteSuccess();
               }
            }).Error(err =>
            {
               result.ReportEntryError(new SequenceEntryException(index, err));
               result.CompleteError(err);
            });
         }

         return result;
      }

      public static Promise<List<T>> Sequence<T>(IList<Promise<T>> promises)
      {
         var result = new Promise<List<T>>();
         var replies = new ConcurrentDictionary<int, T>();

         if (promises == null || promises.Count == 0)
         {
            result.CompleteSuccess(replies.Values.ToList());
            return result;
         }

         for (var i = 0; i < promises.Count; i++)
         {
            var index = i;

            promises[i].Then(reply =>
            {
               replies.TryAdd(index, reply);

               if (replies.Count == promises.Count)
               {
                  result.CompleteSuccess(replies.Values.ToList());
               }
            }).Error(err => result.CompleteError(err));
         }

         return result;
      }

      public static Promise<List<T>> Sequence<T>(params Promise<T>[] promises)
      {
         return Sequence((IList<Promise<T>>) promises);
      }

      /// <summary>
      /// Given a list of promise generator functions, process the whole list, but serially.
      /// Only one promise will be active at any given moment.
      /// </summary>
      /// <param name="generators"></param>
      /// <typeparam name="T"></typeparam>
      /// <returns>A single promise of Unit to represent the completion of the processing. Any other side effects need to be handled separately</returns>
      public static Promise<Unit> ExecuteSerially<T>(List<Func<Promise<T>>> generators)
      {
         async System.Threading.Tasks.Task Execute()
         {
            for (var i = 0; i < generators.Count; i++)
            {
               var generator = generators[i];
               var promise = generator();
               await promise;
            }
         }

         return Execute().ToPromise();
      }

      private class AtomicInt
      {
         private int v;

         public int Value => System.Threading.Interlocked.CompareExchange(ref v, 0, 0);

         public void Increment()
         {
            System.Threading.Interlocked.Increment(ref v);
         }

         public void Decrement()
         {
            System.Threading.Interlocked.Decrement(ref v);
         }
      }

      public static SequencePromise<T> ExecuteRolling<T>(int maxProcessSize, List<Func<Promise<T>>> generators)
      {
         var current = new AtomicInt();
         var running = new AtomicInt();

         var completePromise = new SequencePromise<T>(generators.Count);

         object locker = generators;

         void ProcessUpToLimit()
         {
            lock (locker)
            {
               var runningCount = running.Value;
               var currentCount = current.Value;

               while (runningCount < maxProcessSize && currentCount < generators.Count)
               {
                  var index = currentCount;
                  var generator = generators[index];

                  current.Increment();
                  running.Increment();
                  var promise = generator();


                  promise.Then(result =>
                  {
                     running.Decrement();
                     completePromise.ReportEntrySuccess(index, result);
                     ProcessUpToLimit();
                  });

                  promise.Error(err =>
                  {
                     running.Decrement();
                     completePromise.ReportEntryError(index, err);
                     ProcessUpToLimit();
                  });

                  runningCount = running.Value;
                  currentCount = current.Value;
               }
            }
         }

         ProcessUpToLimit();
         return completePromise;
      }

      /// <summary>
      /// Given a list of promise generator functions, process the list, but in a rolling fasion.
      /// At any given moment, the highest number of promises running will equal maxProcessSize. As soon a promise finishes, a new promise may start.
      /// </summary>
      /// <param name="maxProcessSize"></param>
      /// <param name="generators"></param>
      /// <typeparam name="T"></typeparam>
      /// <returns></returns>
      public static SequencePromise<T> ExecuteRolling2<T>(int maxProcessSize, List<Func<Promise<T>>> generators)
      {
         var current = 0;
         var running = 0;

         var completePromise = new SequencePromise<T>(generators.Count);

         void ProcessUpToLimit()
         {
            var runningCount = System.Threading.Interlocked.CompareExchange(ref running, 0, 0);
            var currentCount = System.Threading.Interlocked.CompareExchange(ref current, 0, 0);

            while (runningCount < maxProcessSize && currentCount < generators.Count)
            {
               var index = currentCount;
               var generator = generators[index];

               System.Threading.Interlocked.Increment(ref current);
               System.Threading.Interlocked.Increment(ref running);
               var promise = generator();

               promise.Then(result =>
               {
                  System.Threading.Interlocked.Decrement(ref running);

                  completePromise.ReportEntrySuccess(index, result);
                  // lock (generators)
                  ProcessUpToLimit();

               });

               //
               // promise.Error(err =>
               // {
               //    completePromise.ReportEntryError(index, err);
               //    System.Threading.Interlocked.Decrement(ref running);
               //    // lock (generators)
               //
               //    ProcessUpToLimit();
               // });

               runningCount = System.Threading.Interlocked.CompareExchange(ref running, 0, 0);
               currentCount = System.Threading.Interlocked.CompareExchange(ref current, 0, 0);
            }
         }

         ProcessUpToLimit();
         return completePromise;
      }

      /// <summary>
      /// Given a list of promise generator functions, process the list, but in batches of some size.
      /// The batches themselves will run one at a time. Every promise in the current batch must finish before the next batch can start.
      /// </summary>
      /// <param name="maxBatchSize"></param>
      /// <param name="generators"></param>
      /// <typeparam name="T"></typeparam>
      /// <returns>A single promise of Unit to represent the completion of the processing. Any other side effects need to be handled separately</returns>
      public static Promise<Unit> ExecuteInBatch<T>(int maxBatchSize, List<Func<Promise<T>>> generators)
      {
         var batches = new List<List<Func<Promise<T>>>>();

         // create batches...
         for (var i = 0; i < generators.Count; i += maxBatchSize)
         {
            var start = i;
            var minBatchSize = generators.Count - start;
            var count = minBatchSize < maxBatchSize ? minBatchSize : maxBatchSize; // min()
            var batch = generators.GetRange(start, count);
            batches.Add(batch);
         }

         Promise<List<T>> ProcessBatch(List<Func<Promise<T>>> batch)
         {
            // start all generators in batch...
            return Promise.Sequence(batch.Select(generator => generator()).ToList());
         }

         // run each batch, serially...
         var batchRunners = batches.Select(batch => new Func<Promise<List<T>>>(() => ProcessBatch(batch))).ToList();

         return ExecuteSerially(batchRunners);
      }
   }

   public class SequenceEntryException : Exception
   {
      public int Index { get; }

      public SequenceEntryException(int index, Exception inner) : base($"index[{index}]. {inner.Message}", inner)
      {
         Index = index;
      }
   }

   public class SequenceEntrySuccess<T>
   {
      public int Index { get; private set; }
      public T Result { get; private set; }

      public SequenceEntrySuccess(int index, T result)
      {
         Index = index;
         Result = result;
      }
   }


   public static class PromiseExtensions
   {
      public static Promise<T> Recover<T>(this Promise<T> promise, Func<Exception, T> callback)
      {
         var result = new Promise<T>();
         promise.Then(value => result.CompleteSuccess(value))
            .Error(err => result.CompleteSuccess(callback(err)));
         return result;
      }

      public static Promise<T> RecoverWith<T>(this Promise<T> promise, Func<Exception, Promise<T>> callback)
      {
         var result = new Promise<T>();
         promise.Then(value => result.CompleteSuccess(value)).Error(err =>
         {
            try
            {
               var nextPromise = callback(err);
               nextPromise.Then(value => result.CompleteSuccess(value)).Error(errInner =>
               {
                  result.CompleteError(errInner);
               });
            }
            catch (Exception ex)
            {
               result.CompleteError(ex);
            }
         });
         return result;
      }

      public static Promise<Unit> ToPromise(this System.Threading.Tasks.Task task)
      {
         var promise = new Promise<Unit>();

         async System.Threading.Tasks.Task Helper()
         {
            try
            {
               await task;
               promise.CompleteSuccess(PromiseBase.Unit);

            }
            catch (Exception ex)
            {
               promise.CompleteError(ex);
            }
         }

         var _ = Helper();

         return promise;
      }

      public static Promise<T> ToPromise<T>(this System.Threading.Tasks.Task<T> task)
      {
         var promise = new Promise<T>();

         async Task Helper()
         {
            try
            {
               var result = await task;
               promise.CompleteSuccess(result);
            }
            catch (Exception ex)
            {
               promise.CompleteError(ex);
            }
         }

         var _ = Helper();

         return promise;
      }

      public static Promise<Unit> ToUnit<T>(this Promise<T> self)
      {
         return self.Map(_ => PromiseBase.Unit);
      }


   }

   public class UncaughtPromiseException : Exception
   {
      public PromiseBase Promise { get; }

      public UncaughtPromiseException(PromiseBase promise, Exception ex) : base(
         $"Uncaught promise innerMsg=[{ex.Message}]", ex)
      {
         Promise = promise;
      }
   }

   /// <summary>
   /// This struct defines the %Beamable %Unit.
   ///
   /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
   ///
   /// #### Related Links
   /// - None
   ///
   /// ![img beamable-logo]
   ///
   /// </summary>
   public readonly struct Unit
   {
   }
}