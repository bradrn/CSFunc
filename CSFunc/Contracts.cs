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
        public static Tuple<ImmutableList<ContractInputPredicate>, ImmutableList<ContractOutputPredicate<T>>> Requires<T>(bool pred, string message) =>
            Tuple.Create(ImmutableList<ContractInputPredicate>.Empty.Add(new ContractInputPredicate(pred, message)), ImmutableList<ContractOutputPredicate<T>>.Empty);

        public static Tuple<ImmutableList<ContractInputPredicate>, ImmutableList<ContractOutputPredicate<T>>> Requires<T>(
                    this Tuple<ImmutableList<ContractInputPredicate>, ImmutableList<ContractOutputPredicate<T>>> prev,
                    bool pred,
                    string message) =>
            Tuple.Create(prev.Item1.Add(new ContractInputPredicate(pred, message)), prev.Item2);

        public static Tuple<ImmutableList<ContractInputPredicate>, ImmutableList<ContractOutputPredicate<T>>> Ensures<T>(ContractOutputPredicate<T>.ContractOutputPredicateDelegate pred, string message) =>
            Tuple.Create(ImmutableList<ContractInputPredicate>.Empty, ImmutableList<ContractOutputPredicate<T>>.Empty.Add(new ContractOutputPredicate<T>(pred, message)));

        public static Tuple<ImmutableList<ContractInputPredicate>, ImmutableList<ContractOutputPredicate<T>>> Ensures<T>(
                    this Tuple<ImmutableList<ContractInputPredicate>, ImmutableList<ContractOutputPredicate<T>>> prev,
                    ContractOutputPredicate<T>.ContractOutputPredicateDelegate pred,
                    string message) =>
                Tuple.Create(prev.Item1, prev.Item2.Add(new ContractOutputPredicate<T>(pred, message)));

        public static T Do<T>(this Tuple<ImmutableList<ContractInputPredicate>, ImmutableList<ContractOutputPredicate<T>>> contracts, Func<T> f)
        {
            foreach (ContractInputPredicate cip in contracts.Item1) if (!cip.Value) throw new ContractException(cip.Message);
            T result = f();
            foreach (ContractOutputPredicate<T> cop in contracts.Item2) if (!cop.Value(result)) throw new ContractException(cop.Message);
            return result;
        }

        public struct ContractInputPredicate { public bool Value { get; set; } public string Message { get; set; } public ContractInputPredicate(bool value, string message) { Value = value;  Message = message; } }
        public struct ContractOutputPredicate<T> { public delegate bool ContractOutputPredicateDelegate(T output); public ContractOutputPredicateDelegate Value { get; set; } public string Message { get; set; } public ContractOutputPredicate(ContractOutputPredicateDelegate value, string message) { Value = value; Message = message; } }
    }

    public class ContractException : Exception
    {
        public ContractException() : base() { }
        public ContractException(string message) : base(message) { }
    }
}
