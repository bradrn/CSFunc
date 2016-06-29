using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSFunc.Types;

namespace CSFunc.CodeContracts
{
    public static class Contracts
    {
        public static ImmutableList<ContractInputPredicate> Requires(this ImmutableList<ContractInputPredicate> prev,
                                                                                    bool pred,
                                                                                    string message) =>
            prev.Add(new ContractInputPredicate(pred, message));

        public static ImmutableList<ContractInputPredicate> Requires(bool pred, string message) =>
            ImmutableList<ContractInputPredicate>.Empty.Add(new ContractInputPredicate(pred, message));

        public static ImmutableList<Tuple<ContractOutputPredicate<T>, string>> Ensures<T>(
            this ImmutableList<Tuple<ContractOutputPredicate<T>, string>> prev, ContractOutputPredicate<T> pred, string message) =>
                prev.Add(Tuple.Create(pred, message));

        public static ImmutableList<Either<ContractInputPredicate, Tuple<ContractOutputPredicate<T>, string>>> Ensures<T>(
            this ImmutableList<ContractInputPredicate> prev, ContractOutputPredicate<T> pred, string message) =>
                prev.Select(cip => Either<ContractInputPredicate, Tuple<ContractOutputPredicate<T>, string>>.Left(cip))
                    .ToImmutableList()
                    .Add(Either<ContractInputPredicate, Tuple<ContractOutputPredicate<T>, string>>.Right(Tuple.Create(pred, message)));

        public static T Do<T>(this ImmutableList<Either<ContractInputPredicate, Tuple<ContractOutputPredicate<T>, string>>> contracts, Func<T> f)
        {
            T result = f();
            bool satisfied = true;
            foreach (var contract in contracts)
            {
                contract.Match
                (
                    Left: cip =>
                    {
                        satisfied = satisfied && cip.Value;
                        if (!satisfied) throw new ContractException(cip.Message);
                        return Unit.Nil;
                    },
                    Right: cop =>
                    {
                        satisfied = satisfied && cop.Item1(result);
                        if (!satisfied) throw new ContractException(cop.Item2);
                        return Unit.Nil;
                    }
                );
            }
            return result;
        }

        public delegate bool ContractOutputPredicate<T>(T output);
        public struct ContractInputPredicate { public bool Value { get; set; } public string Message { get; set; } public ContractInputPredicate(bool value, string message) { Value = value;  Message = message; } }
    }

    public class ContractException : Exception
    {
        public ContractException() : base() { }
        public ContractException(string message) : base(message) { }
    }
}
