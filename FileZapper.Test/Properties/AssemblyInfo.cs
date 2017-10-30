using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("FileZapper.Test")]
[assembly: AssemblyDescription("Finds and removes duplicate files from specified folders")]
[assembly: AssemblyProduct("FileZapper")]
[assembly: AssemblyCopyright("Copyright © 2017 Peter Wetzel")]

[assembly: ComVisible(false)]
[assembly: Guid("bad1b3f8-92b1-49c9-a5e8-cb20a15ef804")]

[assembly: AssemblyVersion("2.2.1.0")]
[assembly: AssemblyFileVersion("2.2.1.0")]
/*
 * Version 2.2.1.0
 * - Now targets .NET framework v4.7
 * - Nugets updated
 * - Minor string handling changes
 * Version 2.2.0.0
 * - Added new boolean setting "DupeCheckIgnoresHierarchy" to help speed up process; now by default will only sample hashes from same folder.
 * - Bug fix: full hashing algorithm now properly skips files with no sample hash
 * - Now targets .NET framework v4.6.1 (was v4.5)
 * - Nuget update
 * - Updated tests to be compatible with v3 of NUnit
 * - Added and refined tests
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
