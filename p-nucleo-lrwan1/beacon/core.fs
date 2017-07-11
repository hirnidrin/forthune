\ core libraries

<<<board>>>
compiletoflash
( core start: ) here dup hex.

\ include app specific library files here, from flib and ex dirs

\ for our simple blinky example, we want to use the timed library
\ include ../../flib/mecrisp/multi.fs
\ include ../../flib/any/timed.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
