\ board.fs

\ load the MCU board definitions
include ../nucleo-l073rz/board.fs
compiletoflash

\ additions for the SX1272MB2xAS add-on board
\ include ../flib/spi/sx1272.fs \ will come here when stable

( board end, size: ) here dup hex. swap - .
cornerstone <<<board>>> \ redefine, includes add-on definitions
compiletoram
