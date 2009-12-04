PdnBench, v2.6 Release
----------------------
This is a command-line utility that runs through several benchmarks that
exercise various aspects of Paint.NET. Every benchmark is multithreaded, and
takes advantage of multiprocessor or multicore systems.

To use, copy the executables to the directory where Paint.NET is installed,
and then run it from the command-line.

Use "pdnbench /?" for a list of command-line parameters.

PdnBench.exe will run in 32-bit mode on 32-bit systems, or 64-bit on 64-bit
systems (must have a 64-bit CPU and a 64-bit OS).

PdnBench_32bitOnly.exe is the same program but it will run in 32-bit mode even
on 64-bit systems. This is useful for comparing 32-bit and 64-bit performance.

The source code is available as part of the main Paint.NET source code
distribution, which is available at the main website:

    http://www.eecs.wsu.edu/paint.net