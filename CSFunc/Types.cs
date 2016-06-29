using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSFunc.Types
{
    #region Maybe<a>
    // Maybe a = Just a | Nothing
    public class Maybe<a>
    {
        private class JustImpl<a1>
        {
            public a1 Value1 { get; set; } = default(a1);
            public JustImpl(a1 value1)
            {
                Value1 = value1;
            }
        }
        private class NothingImpl
        {
            public NothingImpl()
            {
            }
        }
        public MaybeState State { get; set; }
        private JustImpl<a> JustField;
        private JustImpl<a> JustValue { get { return JustField; } set { JustField = value; NothingField = null; State = MaybeState.Just; } }
        private NothingImpl NothingField;
        private NothingImpl NothingValue { get { return NothingField; } set { NothingField = value; JustField = null; State = MaybeState.Nothing; } }
        private Maybe() { }
        public static Maybe<a> Just(a value1)
        {
            Maybe<a> result = new Maybe<a>();
            result.JustValue = new JustImpl
            <a>(value1);
            return result;
        }
        public static Maybe<a> Nothing()
        {
            Maybe<a> result = new Maybe<a>();
            result.NothingValue = new NothingImpl
            ();
            return result;
        }
        public a1 Match<a1>(Func<a, a1> Just, Func<a1> Nothing)
        {
            switch (State)
            {
                case MaybeState.Just: return Just(JustValue.Value1);
                case MaybeState.Nothing: return Nothing();
            }
            return default(a1);
        }
        public Maybe<b> Map<b>(Func<a, b> map) => Match
                                                  (
                                                      Nothing: () => Maybe<b>.Nothing(),
                                                      Just: (v) => Maybe<b>.Just(map(v))
                                                  );

        public Maybe<b> Bind<b>(Func<a, Maybe<b>> f) => Match
                                                        (
                                                            Nothing: () => Maybe<b>.Nothing(),
                                                            Just: (v) => f(v)
                                                        );
        public static Maybe<a> Return(a value) => Just(value);
        public override string ToString() => Match
                                             (
                                                Just: v => "Just " + v.ToString(),
                                                Nothing: () => "Nothing"
                                             );
    }
    public enum MaybeState { Just, Nothing }

    public static class _MaybeLINQ
    {
        public static Maybe<b> Select<a, b>(this Maybe<a> m, Func<a, b> map) => m.Map(map);
        public static Maybe<b> SelectMany<a, b>(this Maybe<a> m, Func<a, Maybe<b>> f) => m.Bind(f);
        public static Maybe<c> SelectMany<a, b, c>(this Maybe<a> m, Func<a, Maybe<b>> f, Func<a, b, c> f2) =>
            m.Bind(x =>
            f(x).Bind(y =>
            Maybe<c>.Return(f2(x, y))));
            // do {x <- m; y <- f(x); return f2(x, y)}
    }
    #endregion

    #region State<a, s>
    public class State<s, a>
    {
        private Func<s, Tuple<a, s>> func { get; set; }
        private State() { }
        public static State<s, a> RunState(Func<s, Tuple<a, s>> value)
        {
            State<s, a> result = new State<s, a>(); result.func = value; return result;
        }
        public Tuple<a, s> runState(s state) => func(state);
        public State<s, b> Map<b>(Func<a, b> f) => State<s, b>.RunState(state =>
        {
            Tuple<a, s> resultIntermediate = this.runState(state);
            return Tuple.Create(f(resultIntermediate.Item1), resultIntermediate.Item2);
        });
        public static State<s, a> Return(a value) => State<s, a>.RunState((state) => Tuple.Create(value, state));
        public State<s, b> Bind<b>(Func<a, State<s, b>> f) => State<s, b>.RunState((state) =>
        {
            Tuple<a, s> resultIntermediate = this.runState(state);
            State<s, b> g = f(resultIntermediate.Item1);
            return g.runState(resultIntermediate.Item2);
        });
        public static State<s, s> get() => State<s, s>.RunState(state => Tuple.Create(state, state));
        public static State<s, Unit> put(s newState) => State<s, Unit>.RunState(state => Tuple.Create(Unit.Nil, newState));
    }

    public static class _StateLINQ
    {

        public static State<s, b> Select<a, b, s>(this State<s, a> m, Func<a, b> map) => m.Map(map);
        public static State<s, b> SelectMany<a, b, s>(this State<s, a> m, Func<a, State<s, b>> f) => m.Bind(f);
        public static State<s, c> SelectMany<a, b, c, s>(this State<s, a> m, Func<a, State<s, b>> f, Func<a, b, c> f2) =>
            m.Bind(x =>
            f(x).Bind(y =>
            State<s, c>.Return(f2(x, y))));
    }
    #endregion

    #region Unit
    public class Unit
    {
        private Unit() { }
        public static Unit Nil => new Unit();
        public override string ToString() => "Unit";
    }
    #endregion

    #region Either<a, b>
    // Either a b = Left a | Right b
    public class Either<a, b>
    {
        private class LeftImpl<a1>
        {
            public a1 Value1 { get; set; } = default(a1);
            public LeftImpl(a1 value1)
            {
                Value1 = value1;
            }
        }
        private class RightImpl<b1>
        {
            public b1 Value1 { get; set; } = default(b1);
            public RightImpl(b1 value1)
            {
                Value1 = value1;
            }
        }
        public EitherState State { get; set; }
        private LeftImpl<a> LeftField;
        private LeftImpl<a> LeftValue { get { return LeftField; } set { LeftField = value; RightField = null; State = EitherState.Left; } }
        private RightImpl<b> RightField;
        private RightImpl<b> RightValue { get { return RightField; } set { RightField = value; LeftField = null; State = EitherState.Right; } }
        private Either() { }
        public static Either<a, b> Left(a value1)
        {
            Either<a, b> result = new Either<a, b>();
            result.LeftValue = new LeftImpl
            <a>(value1);
            return result;
        }
        public static Either<a, b> Right(b value1)
        {
            Either<a, b> result = new Either<a, b>();
            result.RightValue = new RightImpl
            <b>(value1);
            return result;
        }
        public a1 Match<a1>(Func<a, a1> Left, Func<b, a1> Right)
        {
            switch (State)
            {
                case EitherState.Left: return Left(LeftValue.Value1);
                case EitherState.Right: return Right(RightValue.Value1);
            }
            return default(a1);
        }
        public override string ToString() => Match
                                             (
                                                Left: v => "Left " + v.ToString(),
                                                Right: v => "Right " + v.ToString()
                                             );
    }
    public enum EitherState
    {
        Left, Right
    }
    #endregion

    #region Error<TResult, TError>
    // Error TResult TError = Result TResult | Throw TError
    public class Error<TResult, TError>
    {
        private class ResultImpl<a1>
        {
            public a1 Value1 { get; set; } = default(a1);
            public ResultImpl(a1 value1)
            {
                Value1 = value1;
            }
        }
        private class ThrowImpl<b1>
        {
            public b1 Value1 { get; set; } = default(b1);
            public ThrowImpl(b1 value1)
            {
                Value1 = value1;
            }
        }
        public ErrorState State { get; set; }
        private ResultImpl<TResult> ResultField;
        private ResultImpl<TResult> ResultValue { get { return ResultField; } set { ResultField = value; ThrowField = null; State = ErrorState.Result; } }
        private ThrowImpl<TError> ThrowField;
        private ThrowImpl<TError> ThrowValue { get { return ThrowField; } set { ThrowField = value; ResultField = null; State = ErrorState.Throw; } }
        private Error() { }
        public static Error<TResult, TError> Result(TResult value1)
        {
            Error<TResult, TError> result = new Error<TResult, TError>();
            result.ResultValue = new ResultImpl<TResult>(value1);
            return result;
        }
        public static Error<TResult, TError> Throw(TError value1)
        {
            Error<TResult, TError> result = new Error<TResult, TError>();
            result.ThrowValue = new ThrowImpl<TError>(value1);
            return result;
        }
        public a1 Match<a1>(Func<TResult, a1> Result, Func<TError, a1> Throw)
        {
            switch (State)
            {
                case ErrorState.Result: return Result(ResultValue.Value1);
                case ErrorState.Throw: return Throw(ThrowValue.Value1);
            }
            return default(a1);
        }
        public Error<TResult2, TError> Map<TResult2>(Func<TResult, TResult2> f) => Match
                                                                                   (
                                                                                        Result: r => Error<TResult2, TError>.Result(f(r)),
                                                                                        Throw: e => Error<TResult2, TError>.Throw(e)
                                                                                   );
        public Error<TResult2, TError> Bind<TResult2>(Func<TResult, Error<TResult2, TError>> f) => Match
                                                                                   (
                                                                                        Result: r => f(r),
                                                                                        Throw: e => Error<TResult2, TError>.Throw(e)
                                                                                   );
        public static Error<TResult, TError> Return(TResult value) => Result(value);
        public override string ToString() => Match
                                             (
                                                 Result: r => "Result " + r.ToString(),
                                                 Throw: e => "Throw " + e.ToString()
                                             );
    }
    public enum ErrorState
    {
        Result, Throw
    }

    public static class _ErrorLINQ
    {
        public static Error<TResult2, TError> Select<TResult, TResult2, TError>(this Error<TResult, TError> m, Func<TResult, TResult2> f) => m.Map(f);
        public static Error<TResult2, TError> SelectMany<TResult, TResult2, TError>(this Error<TResult, TError> m, Func<TResult, Error<TResult2, TError>> f) =>
            m.Bind(f);
        public static Error<TResult3, TError> SelectMany<TResult, TResult2, TResult3, TError>(this Error<TResult, TError> m, Func<TResult, Error<TResult2, TError>> f, Func<TResult, TResult2, TResult3> f2) =>
            m.Bind(x =>
            f(x).Bind(y =>
            Error<TResult3, TError>.Return(f2(x, y))));
        // from x in m
        // from y in f(x)
        // select Error<TResult3, TError>.Return(f2(x, y))))
    }

    public static class Error
    {
        public static Error<T, Exception> Try<T>(Func<T> f)
        {
            try { return Error<T, Exception>.Result(f()); }
            catch (Exception e) { return Error<T, Exception>.Throw(e); }
        }
        public static Error<Unit, Exception> Try(Action a) => Error.Try(() => { a(); return Unit.Nil; });

        public static Error<T, Exception> Catch<T>(this Error<T, Exception> @this, Predicate<Exception> p, Action a) =>
            @this.Match(Result: _ => @this,
                        Throw: e => { if (p(e)) a(); return @this; });
    }
    #endregion
}
