# RankWidthApproximate
This program was written for the master's thesis *A simulated annealing method for computing rank-width*.
It implements a simulated annealing based algorithm for approximating rank-width, F4-rank-width and maximum matching-width,
and outputs the corresponding decomposition in dot graph format.
The program was tested on both Windows and Linux, and can make use of SIMD instructions (AVX2 recommended) and multiple cores.

## Usage
`RankWidthApproximate.exe [options] graph.dgf`
 
The following options are available:
- **-ac** Enables adaptive cooling.
- **-d** Approximate the F4-rank-width of a directed input graph (can not be used in combination with **-mm**).
- **-d2u** Approximate the rank-width or maximum matching-width of a directed input graph by first converting it to undirected (each arc becomes an undirected edge).
- **-is *seed*** Sets the seed for the random generator for the initial solution (by default the same random generator is used as for the search process).
- **-mm** Approximate the maximum matching-width of an undirected input graph (or directed converted to undirected with **-d2u**).
- **-s *seed*** Sets the seed for the random generator (random by default).
- **-t *temperature*** Sets the initial temperature (default is 5.0).
- **-td *delta*** Enables the thresholding heuristic with the given threshold delta.
- **-tl *seconds*** Stops the search process if it is still running after the given number of seconds (the search process also stops when the temperature drops below 0.05).
- **-v** Output more information during the search process.

## Input graph format
The program supports a number of variations of the text based DIMACS graph format, for both undirected and directed graphs.

### Undirected
For undirected graphs the following rules apply:
- Lines starting with `c`, `n` or `x` are ignored.
- Before any edges are defined, a header is expected in the following format:
    ```
    p format vtxCount edgeCount
    ```
    The `format` field is ignored by the program, but usually contains the word `edge`. A file should only have a single header line.

- Edges can be defined as either
    ```
    vtxId1 vtxId2
    ```
    or
    ```
    e vtxId1 vtxId2
    ```
    for an edge between `vtxId1` and `vtxId2`. There are expected to be as many edge definitions as specified in the header. In case of the `e` format, any other input after `vtxId2` is ignored.

- Vertex ids can be anything without spaces. As such it does not matter if vertices are numbered from 0, from 1 or identified with any other number, letter or label.

Some examples of valid inputs:
```
p edge 4 4
1 2
2 3
3 4
4 1
```
```
c 5 vertex cycle
p edge 5 5
e 0 1
e 1 2
e 2 3
e 3 4
e 0 4
```
```
x low 19.00
p edge 3 3
n 92 1.00
n 26 1.00
n 93 1.00
e 26 93
e 26 92
e 92 93
```
```
p edge 7 11
e A B
e A C
e B D
e B E
e C D
e D F
e D E
e D G
e C G
e E F
e F G
```

### Directed
For directed graphs the format is mostly similar to the undirected format, except that the header now contains the number of arcs instead of the number of edges, and the way the arcs are defined differs slightly from the way edges were defined. They are defined as either
```
vtxId1 vtxId2
```
or
```
a vtxId1 vtxId2
```
for an arc from `vtxId1` to `vtxId2`. In case of the `a` format, any other input after `vtxId2` (such as weights) is ignored.

## Tests
Some unit tests are provided that verify some parts of the program.

## Build flags
There are some build flags that can be enabled to have extra logging:
- `LOG_PROGRESS` At the end, print when the first decomposition of each width was found
- `CACHE_HISTOGRAM` At the end, print a (textual) histogram of cache hit rates per iteration

## Citing
When using this program for research work, please cite my thesis as follows:
```
@MastersThesis{nouwt2022rankwidth,
    title = {A simulated annealing method for computing rank-width},
    author = {Nouwt, Florian},
    year = {2022},
    month = May,
    school = {Utrecht University}
}
```
