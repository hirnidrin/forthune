\ core libraries

<<<board>>>
compiletoflash
( core start: ) here dup hex.

\ include app specific library files here, from flib and ex dirs
include ../../flib/mecrisp/multi.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
