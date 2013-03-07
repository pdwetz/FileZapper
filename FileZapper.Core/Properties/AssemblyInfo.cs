using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("FileZapper.Core")]
[assembly: AssemblyDescription("Finds and removes duplicate files from specified folders")]
[assembly: AssemblyCompany("WetzNet Software")]
[assembly: AssemblyProduct("FileZapper")]
[assembly: AssemblyCopyright("Copyright © 2013 Peter Wetzel")]

[assembly: ComVisible(false)]
[assembly: Guid("aff92b8c-1d0e-4df7-897e-6b5c2d92e69b")]

[assembly: AssemblyVersion("2.0.0.0")]
[assembly: AssemblyFileVersion("2.0.0.0")]
/*
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