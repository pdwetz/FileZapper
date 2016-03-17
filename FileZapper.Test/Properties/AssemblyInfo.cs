using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("FileZapper.Test")]
[assembly: AssemblyDescription("Finds and removes duplicate files from specified folders")]
[assembly: AssemblyProduct("FileZapper")]
[assembly: AssemblyCopyright("Copyright © 2016 Peter Wetzel")]

[assembly: ComVisible(false)]
[assembly: Guid("bad1b3f8-92b1-49c9-a5e8-cb20a15ef804")]

[assembly: AssemblyVersion("2.2.0.0")]
[assembly: AssemblyFileVersion("2.2.0.0")]
/*
 * Version 2.1.0.0
 * - Added custom exceptions
 * - Added new phase to do hashes on small samples of possible file matches
 * - Fixed logging
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
