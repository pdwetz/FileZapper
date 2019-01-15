FileZapper Release Notes
==============

Version 3.2.0.0
- For large files (2 GB+), now uses MD5 in place of Farmhash to avoid IO error

Version 3.1.0.0
- Now targets .Net Core 2.2

Version 3.0.0.0
- Now targets .Net Core 2.1
- Deletes are now final. No more recycle bin support.
- Support for multiple hashing options. Defaults to Farmhash.
- Logging now uses Serilog
- Uses new MS extensions for command line, config, etc.

Version 2.2.1.0
- Now targets .NET framework v4.7
- Nugets updated
- Minor string handling changes

Version 2.2.0.0
- Added new boolean setting "DupeCheckIgnoresHierarchy" to help speed up process; now by default will only sample hashes from same folder.
- Bug fix: full hashing algorithm now properly skips files with no sample hash
- Now targets .NET framework v4.6.1 (was v4.5)
- Nuget update
- Updated tests to be compatible with v3 of NUnit
- Added and refined tests

Version 2.1.0.0
- Added custom exceptions
- Added new phase to do hashes on small samples of possible file matches
- Fixed logging
 
Version 2.0.0.0
- Updated to .NET 4.5 framework
- Logic pulled into new library assembly
- Added new NUnit test assembly
- Dropped SQL usage (file data is no longer carried over between runs)
- Exceptions now logged via log4net
- Added config settings
