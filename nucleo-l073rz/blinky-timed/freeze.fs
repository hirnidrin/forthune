\ frozen application, this runs tests and wipes to a clean slate if they pass

include ../always.fs
include ../board.fs
include core.fs

compiletoflash
include dev.fs

\ autostart the application on boot/reset
\ unattended -> see board.fs
: init ( -- )
  init unattended blinky ;
