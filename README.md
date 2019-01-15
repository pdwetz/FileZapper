FileZapper
==============

Finds and removes duplicate files.

# Usage
To view command help

	FileZapper -h

Example of processing all files in two root folders (and all subdirectories)

	FileZapper -i -o 1000000 -f E:\temp\zaptest1 E:\temp\zaptest2

Example for ignoring large files

	FileZapper -o 1000000 -f E:\temp\zaptest1

Example for ignoring zip files

	FileZapper -x ".zip" -f E:\temp\zaptest1

Example for always deleting files with given extension

	FileZapper -y ".tmp" -f E:\temp\zaptest1
	
# History 
This was primarily a project to scratch an itch of mine and isn't particularly meant for mass consumption. Everything is processed in memory with the results dumped into CSV files for review.

# Features
- Console application
- Utilizes parallel aspects of .Net; tested with physical 4 core Intel cpu.
- Performs multiple phases for determining and removing duplicate files via hashing and a simple scoring mechanism. Includes an initial sampling pass to limit number of full file hashes.
- Supports MD5, CRC, and Farmhash (default)
- Allows you to skip phases if necessary; mainly for testing/debugging purposes.

# Performance
- For large file systems, processing can be intensive for CPU and I/O and take hours to run.
- For small tests, it should run very quickly (a few seconds at most).

# Notes
For a simplistic python port, see PyFileZapper (https://github.com/pdwetz/PyFileZapper).
