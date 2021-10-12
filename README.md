# General 1D Cellular Automaton Simulator ("Failed" attempt)
The capabilities of the program are shown off in a Youtube video here: 

The code is meant to be used in conjunction with Unity Game Engine, especially its "inspector". 

The code can simulate any simple 1-dimensional cellular automaton. The aim was to try to use Compute Shaders, written in HLSL, to speed up the simulation. However, in the end the overhead of moving data between the CPU and GPU made the program massively slower than a multithreaded CPU-only counterpart.

The author strongly discourages the use of this code.