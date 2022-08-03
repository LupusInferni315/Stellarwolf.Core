# Stellarwolf.Core
The core library of the Stellar Wolf Framework

# Chaos Engine
The Chaos Engine is a multipurpose Pseudo-Random Number Generator
It contains the ability to generate singular, as well as fill and create arrays of
 - 32-bit integers (int)
 - Single-precision floating-point numbers (float)
 - Double-precision floating-point numbers (double)
 - 8-bit integers (byte)
 - Booleans

It also contains the ability to generate booleans based on the probability of returning true.

It can shuffle arrays and lists as well as select random elements out of a list or array
 - If the type of list or array implements `IWeighted` the weight of the item will be taken into account when selecting a random element.
 
It can also select random elements out of an `Enum` as well.
 - If you have defined an enum for this purpose you can attach the `WeightAttribute` to the value, which will be taken into account when selecting a value.
 - If a value in an `Enum` does not have the `WeightAttribute` the weight will default to 1.
 - A `Weight` of 0 ensures that a value cannot be randomly selected.
 
The state of the generator can be saved to an `int[]` to be handled seperately or written directly to a `Stream` and can be loaded the same ways.

The class provides a `ThreadStatic` instance for sharing a single instance across an entire application.

The seed of the class can be provided in either the form of an integer or a string

 If the seed is a string the class will attempt to convert it into an integer before seeding the generator (ie 315 is the same as "315").
 
The class can be reset, or reseeded without creating a new instance.

# Die Type
The `DieType` enum provides an easy to read format for rolling dice.
 
The enum can be used in conjunction with the `Dice` struct to store the count and type of dice to be rolled multiple times.

# Dice
The `Dice` struct provides a way to store and roll dice

It also provides static methods to roll dice at runtime without the need to create a `Dice` object, it also allows the caller to roll 'Non-Standard' dice such as a D3.

# IWeighted
An object that inherits from `IWeighted` defines a weight to be applied to the instance for controlling how often an element is selected when selecting random elements from a list.

# WeightAttribute
An enum value with a `WeightAttribute` defines a weight to be applied to the instance for controlling how often an element is selected when selecting random elements from the enum.
