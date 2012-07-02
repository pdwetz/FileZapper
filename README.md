FileZapper
==============

Finds and removed duplicate files
--------------

This was primarily a project to scratch an itch of mine and isn't particularly meant for mass consumption. It uses some newer techs, but is fairly simple and straight forward. The only external library used is Dapper for streamlining database calls.

- Requires a SQL database; tested with local SQL Express instance. Setup scripts are in Database folder.
- Utilizes parallel aspects of .NET 4.0 framework; tested with 4 core Intel cpu.
- Performs multiple phases for determining and removing duplicate files via MD5 hashing and a simple scoring mechanism.
- Allows you to skip phases if necessary; mainly for debugging purposes.
- For large file systems, processing can be intensive for CPU and I/O (and take hours to run).
- For small tests, it should run very quickly (a few seconds at most).
