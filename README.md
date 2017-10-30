FileZapper
==============

Finds and removes duplicate files. For a simplistic python port, see PyFileZapper (https://github.com/pdwetz/PyFileZapper).

This was primarily a project to scratch an itch of mine and isn't particularly meant for mass consumption. While the initial release utilized a SQL server instance for tracking changes and doing the core set-based operations, the upgraded version has no data store ties at all. Everything is processed in memory with the results dumped into CSV files for review.

# Features
- Console application... with colors!
- Utilizes parallel aspects of .NET 4+ framework; tested with physical 4 core Intel cpu.
- Performs multiple phases for determining and removing duplicate files via MD5 hashing and a simple scoring mechanism. Includes an initial sampling pass to limit number of full file hashes.
- Allows you to skip phases if necessary; mainly for testing/debugging purposes.
- For large file systems, processing can be intensive for CPU and I/O and take hours to run.
- For small tests, it should run very quickly (a few seconds at most).

Planned work (mostly TODO's in the code) with no real time table:
- Migrate to .Net Core (including switching to latest commandline and config logic)
- Do perf differences on Crc32 vs. MD5 (possibly switching, or making it a setting to determine which is used)
- Profile parallel logic to make sure we're not improperly hitting bottlenecks (including testing of partitioning logic)
- Test various buffer sizes when hashing for optimal size (could vary depending on file size)

# Release Notes
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
