# CSFunc
**All libraries and executables are avaliable from the releases tab.**

CSFunc is a library that makes it possible to use functional programming techniques in C#. Features include:
* Algebraic data types
* Pre/postconditions

### Algebraic Data Types
Algebraic data types are classes which can take one of many values, similar to enums. However, unlike enums, each value can store data. A typical example of an algebraic data type is `Maybe<a>`, which can take a value of either `Just<a>`, which represents the presence of a value of type `a`, or `Nothing`, which represents an absence of a value. CSFunc includes the following datatypes:
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
`Maybe` and `Either` are _functors_, which means they can be mapped over, or have a `Map<T1>` method which takes a single argument of type `Func<T, T1>` and 'maps' this function over the data type. An example is provided below:
```csharp
Maybe<int> value1 = Maybe<int>.Just(1);
Maybe<int> value2 = Maybe<int>.Nothing();
Maybe<int> result1 = value1.Map(v => v + 1); // result1 = Maybe<int>.Just(2)
Maybe<int> result2 = value2.Map(v => v + 1); // result2 = Maybe<int>.Nothing()
```
Note that this is the same as the `Select` method in LINQ, only extended to other types.
`Maybe` and `Either` are also _monads_. (The concept of a _monad_ is a more difficult one than that of a functor. For a good introduction to monads, see MathematicalOrchid's answer in <http://stackoverflow.com/questions/44965/what-is-a-monad>. From here on, I assume you know what a monad is.) The bind and return functions are named `Bind` and `Return` respectively. Monads can also be used from LINQ, which is itself a monad. For example:
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

### Pre- and Postconditions
**This section has not been written yet.**
