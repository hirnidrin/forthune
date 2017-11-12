\ Create turnkey application:
\ - rebuild, write all code to flash
\ - autorun on power-on / reset, unless serial terminal is connected
\ - if serial terminal is connected, don't autorun, enter interactive mode

\ include ../always.fs
\ include ../board.fs
include core.fs

compiletoflash
include dev.fs

\ autostart the application on boot/reset
\ unattended -> see board.fs
: init ( -- )
  init unattended blinky ;
