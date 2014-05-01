using System.Reflection;

[assembly: AssemblyTitle("FileZapper")]
[assembly: AssemblyDescription("Finds and removes duplicate files from specified folders")]
[assembly: AssemblyProduct("FileZapper")]
[assembly: AssemblyCopyright("Copyright © 2014 Peter Wetzel")]

[assembly: AssemblyVersion("2.0.1.0")]
[assembly: AssemblyFileVersion("2.0.1.0")]
/*
 * Version 2.0.1.0
 * - Added custom exceptions
 * - Updated CSV code for obsolete methods in library
 * Version 2.0.0.0
 * - Updated to .NET 4.5 framework
 * - Logic pulled into new library assembly
 * - Added new NUnit test assembly
 * - Dropped SQL usage (file data is no longer carried over between runs)
 * - Exceptions now logged via log4net
 * - Added config settings
 * Version 1.0.0.0
 * - Initial release
*/