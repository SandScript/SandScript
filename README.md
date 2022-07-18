
#  SandScript

>  Yet another language with "Script" in the name

This repository contains SandScript, a statically typed interpreted language hosted by CSharp. The idea for this language began as a personal project to learn about the creation of languages and the technology and knowledge that goes into creating them.

##  Current State

This project is by no means production ready. It has many missing analysis steps, documentation is missing, and the API is not final.

## Requirements

[C# 10](https://devblogs.microsoft.com/dotnet/welcome-to-csharp-10/)
[.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

## Features

* Strong static typing
* Fast and easy interop between CSharp and SandScript
* Easy to implement into projects
* Extendable

## Planned Features

* Standard library of methods available to SandScript
* Dot (.) operator for accessors and static object traversal (I.E. namespace.method).
* Fleshed out optimization pass to eliminate dead code and unused variables/methods.

## Known Issues

* Nested method calls will fail at Semantic Analysis.
* Semantic Analysis does not check that all code paths return a value in cases where it is required.
* Overall, Semantic Analysis is missing many checks.

## Contributing

Contributions are very welcome! See an issue you think you can solve? Feel free to submit a fix of your own. See something missing that you think will be beneficial? Start a conversation in the [issues](https://github.com/SandScript/SandScript/issues) section.
