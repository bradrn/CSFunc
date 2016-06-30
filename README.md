# CSFunc
**All libraries and executables are avaliable from the releases tab.**

CSFunc is a library that makes it possible to use functional programming techniques in C#. Features include:
* Algebraic data types (found in `CSFunc.Types`)
* Other data types to support functional programming (found in `CSFunc.Types`)
* Code Contracts (found in `CSFunc.CodeContracts`)

### Algebraic Data Types
Algebraic data types are classes which can take one of many values, similar to enums. However, unlike enums, each value can store data. A typical example of an algebraic data type is `Maybe<a>`, which can take a value of either `Just<a>`, which represents the presence of a value of type `a`, or `Nothing`, which represents an absence of a value. CSFunc includes the following algebraic data types:
* `Maybe a = Just a | Nothing`
* `Either a b = Left a | Right b` (different from the Haskell type `Either`, which in CSFunc is named `Error`. )
* `Error TResult TError = Result TResult | Error TError`

(Each algebraic data type is, for simplicity, listed in a syntax similar to that used by Haskell, the only difference being that the `data` keyword at the beginning is left off. For the Haskell syntax see, for example, [here](https://wiki.haskell.org/Algebraic_data_type) and [here](http://learnyouahaskell.com/making-our-own-types-and-typeclasses))
To create an instance of a data type, invoke a static method on it.  To do something with the value, use the `Match` method. which has a number of arguments, one for each option, each of which is a delegate. Each argument of the delegate corresponds to the appropriate part of the option. It is convenient to use named arguments, and I often format the code in such a way that it actually looks like an extention to C# syntax. There is also a `State` property which contains the state of an object. An example is shown below:
```csharp
Maybe<int> value1 = Maybe<int>.Just(1);
Maybe<int> value2 = Maybe<int>.Nothing();
int result1 = value1.Match
              (
                Just: v => v,
                Nothing: () => 0
              );
// result1 = 1
int result2 = value2.Match
              (
                Just: v => v,
                Nothing: () => 0
              );
// result2 = 0

result1.Match
(
  Just: v => { Console.WriteLine(v); return Unit.Nil; },  // Unit is a type discussed later in 'Other Data Types' and is primarily meant as a replacement for void when a method is expected to return a value.
  Nothing: () => Unit.Nil;
)
// Prints '1'

result2.Match
(
  Just: v => { Console.WriteLine(v); return Unit.Nil; },
  Nothing: () => Unit.Nil;
)
// Does nothing

if (result1.State == MaybeState.Just) Console.WriteLine("Just"); // Prints 'Just'
```
`Maybe` and `Either` are _functors_, which means they can be mapped over, or have a `Map<T1>` method which takes a single argument of type `Func<T, T1>` and 'maps' this function over the data type. (This is the same function as the LINQ method `Select`, only extended to other types.) An example is provided below:
```csharp
Maybe<int> value1 = Maybe<int>.Just(1);
Maybe<int> value2 = Maybe<int>.Nothing();
Maybe<int> result1 = value1.Map(v => v + 1); // result1 = Maybe<int>.Just(2)
Maybe<int> result2 = value2.Map(v => v + 1); // result2 = Maybe<int>.Nothing()
```
`Maybe` and `Either` are also _monads_. (The concept of a _monad_ is a more difficult one than that of a functor. For a good introduction to monads, see MathematicalOrchid's answer in <http://stackoverflow.com/questions/44965/what-is-a-monad>. From here on, I assume you know what a monad is.) The bind and return functions are named `Bind` and `Return` respectively. Monads can also be used from LINQ, which is itself a monad (the bind function corresponds to LINQ `SelectMany`). For example:
```csharp
Maybe<int> value1 = Maybe<int>.Just(1);
Maybe<int> value2 = Maybe<int>.Nothing();

Maybe<string> result1 = value1.Bind(v => Maybe<string>.Return(v.ToString())); // result1 = Maybe<string>.Just("1")
Maybe<string> result2 = value2.Bind(v => Maybe<string>.Return(v.ToString())); // result2 = Maybe<string>.Nothing()

Maybe<string> result1LINQ = from v in value1
                            select Maybe<string>.Return(v.ToString());
// result1LINQ = Maybe<string>.Just("1")
Maybe<string> result2LINQ = from v in value2
                            select Maybe<string>.Return(v.ToString());
// result1LINQ = Maybe<string>.Nothing()
```
To create a new algebraic data type, use the 'algen' program, which is a REPL environment which accepts a specification in the form above and prints to the console a class which you can paste into your project.

### Other Data Types
CSFunc currently contains, in addition to the types discussed above, two other types: `Unit` and `State<a, s>`. `Unit` is defined as follows:
```csharp
public class Unit
{
  private Unit() { }
  public static Unit Nil => new Unit();
  public override string ToString() => "Unit";
}
```
It is used as a replacement for `void` when a value needs to be returned e.g. in an argumeent to a `Match` method whose only action is to print something out (See, for example, the first example in the "Algebraic Data Types" section).

`State<s, a>` is the _state monad_. This type basically encapsulates a function that takes a single argument of type `s` and returns a `Tuple<a, s>`. This function represents a stateful computation, where the input is the current state, represented by a value of type `s`, and the output is a tuple of the result of the state, of type `a`, and the new state. `State` is a monad - two stateful computations can be composed using the `Bind` function, and a stateful computation that does not change the state but presents a value as the result is made using the `Return` function, where the value presented is the argument. Because it is a monad, `State` can also be used with LINQ. To run a stateful computation, use tthe `runState` function on it, and to make one use the `RunState` function. (The similarity of the names is from Haskell, where both are named `runState`. An example is provided below, implementing a stack as a state:
```csharp
using System.Collections.Immutable;
using System.LINQ;

State<ImmutableList<int>, Unit> push(int value) => State<ImmutableList<int>, Unit>.RunState(state => Tuple.Create(Unit.Nil, state.Add(value)));
  // ImmutableList is found in the library System.Collections.Immutable, most easily avaliable via NuGet. This library is useful
  // for functional programming, which relies heavily on lists and immutable types.

static State<ImmutableList<int>, int> pop() => State<ImmutableList<int>, int>.RunState(state => Tuple.Create(state[state.Count - 1], state.Take(state.Count - 1).ToImmutableList()));

static void StackManip()
{
    Tuple<int, ImmutableList<int>> result = (from _1 in push(1)
                                             from _2 in push(2)
                                             from _3 in push(3)
                                             from topmost in pop()
                                             select topmost).runState(ImmutableList<int>.Empty);
    Console.WriteLine(result.Item1); // prints 3
    Console.WriteLine();
    result.Item2.ForEach(i => Console.WriteLine(i)); // prints 1 [newline] 2
}
```

`State<s, a>` also defines two static convenience methods, `get` and `put`. `get` takes the current state and presents it as the result, and `put` takes a single argument which it presents as the new state. Using these, the example above can be written more concisely as:
```csharp
using System.Collections.Immutable;
using System.LINQ;

static State<ImmutableList<int>, Unit> push(int value) => from state in State<ImmutableList<int>, Unit>.get()
                                                          from _ in State<ImmutableList<int>, Unit>.put(state.Add(value))
                                                          select Unit.Nil;

static State<ImmutableList<int>, int> pop() => from state in State<ImmutableList<int>, Unit>.get()
                                               from _ in State<ImmutableList<int>, int>.put(state.Take(state.Count - 1).ToImmutableList())
                                               select state[state.Count - 1];

static void StackManip()
{
    Tuple<int, ImmutableList<int>> result = (from _1 in push(1)
                                             from _2 in push(2)
                                             from _3 in push(3)
                                             from topmost in pop()
                                             select topmost).runState(ImmutableList<int>.Empty);
    Console.WriteLine(result.Item1); // prints 3
    Console.WriteLine();
    result.Item2.ForEach(i => Console.WriteLine(i)); // prints 1 [newline] 2
}
```
### Code Contracts
CSFunc also includes a functional implementation of preconditions and postconditions, found in the `CSFunc.CodeContracts` namespace. This implementation of code contracts consists of three methods: `Requires`, `Ensures`, and `Do`. Usage is as follows:
```csharp
using CSFunc.CodeContracts;                               // REQUIRED

T[] Add<T>(T[] array, T item) => Contracts.               // Use C# 6-style expression-bodied functions.
    Requires<T[]>(array != null, "Array is null").        // Requires takes two arguments: a boolean expression, and an
                                                          // error message to display if the contract is not satisfied.
                                                          // The type argument is the return type of the method.
    Ensures<T[]>(o => o.Contains(item), "Result does not contain item").
                                                          // Ensures is similar to Requires, but instead of a boolean expression,
                                                          // it takes a delegate with a single parameter, which is the output of
                                                          // the method.
    Do<T[]>(() =>                                         // Do takes a singe argument, which is the method body.
    {
        System.Collections.Generic.List<T> result = new System.Collections.Generic.List<T>(array);
        result.Add(item);
        return result.ToArray();
    });

        void Test()
        {
            int[] array = new int[] { 1, 2, 3 };
            int[] newarray = Add(array, 4); // newarray contains 1, 2, 3, 4
            int[] array2 = null;
            int[] newarray2 = Add(array2, 4); // throws ContractException with message "Array is null"
}
```
This is just a chain of methods; unformatted, it looks like this:
```csharp
T[] Add<T>(T[] array, T item) =>
    Contracts.Requires<T[]>(array != null, "Array is null")
    .Ensures<T[]>(o => o.Contains(item), "Result does not contain item")
    .Do({
          System.Collections.Generic.List<T> result = new System.Collections.Generic.List<T>(array);
          result.Add(item);
          return result.ToArray();
        });
```
However, I prefer to format it like this:
```csharp
T[] Add<T>(T[] array, T item) => Contracts.
    Requires<T[]>(array != null, "Array is null").
    Ensures<T[]>(o => o.Contains(item), "Result does not contain item").
    Do(
    {
      System.Collections.Generic.List<T> result = new System.Collections.Generic.List<T>(array);
      result.Add(item);
      return result.ToArray();
    });
```
