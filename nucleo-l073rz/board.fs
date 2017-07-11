\ board definitions for ST Nucleo-L073RZ
\ needs always.fs

eraseflash
compiletoflash
( board start: ) here dup hex.

include ../flib/mecrisp/calltrace.fs
include ../flib/mecrisp/cond.fs
include ../flib/mecrisp/hexdump.fs
include ../flib/stm32l0/io.fs
include ../flib/pkg/pins64.fs
include ../flib/stm32l0/hal.fs
include ../flib/stm32l0/adc.fs
include ../flib/stm32l0/timer.fs
include ../flib/stm32l0/pwm.fs
include ../flib/stm32l0/i2c.fs
include ../flib/stm32l0/sleep.fs

PB6 variable ssel  \ SPI1 chip select pin on Nucleo-L073RZ board, can be changed at run time
include ../flib/stm32l0/spi.fs

\ the green user LED on the Nucleo-L073RZ board
PA5 constant LED
: led-on LED ios! ;
: led-off LED ioc! ;

\ the blue USER button on the Nucleo-L073RZ board... return true if pressed
PC13 constant BUTTON
: button? ( -- flag ) BUTTON io@ 0= ;

: hello ( -- ) flash-kb . ." KB <Nucleo-L073RZ> " hwid hex.
  $30000 compiletoflash here -  flashvar-here compiletoram here -
  ." ram/flash: " . . ." free " ;

: init ( -- )  \ board initialisation
  init  \ explicitly init usart comms, only required in the Mecrisp "spezial" branch
  ['] ct-irq irq-fault !  \ show call trace in unhandled exceptions
  $00 hex.empty !  \ empty flash shows up as $00 iso $FF on these chips
  OMODE-PP LED io-mode!
  IMODE-FLOAT BUTTON io-mode!
\ 16MHz ( set by Mecrisp on startup to get an accurate USART baud rate )
  8 RCC-CCIPR !  \ set USART2 clock to HSI16, independent of sysclk
  1000 systick-hz
  hello ." ok." cr
;

: rx-connected? ( -- f )  \ true if USART2 RX is connected (and idle)
  IMODE-LOW PA3 io-mode!  PA3 io@ 0<>  OMODE-AF-PP PA3 io-mode!
  dup if 1 ms serial-key? if serial-key drop then then \ flush any input noise
;

: fake-key? ( -- f )  \ check for RX pin being pulled high
  rx-connected? if reset then false ;

\ unattended quits to the interpreter if the RX pin is connected, not floating
\ else it replaces the key? hook with a test to keep checking for RX reconnect
\ if so, it will reset to end up in the interpreter on the next startup
\ for use with a turnkey app in flash, i.e. ": init init unattended ... ;"

: unattended
  rx-connected? if quit then \ return to command prompt
  ['] fake-key? hook-key? ! ;

( board end, size: ) here dup hex. swap - .
cornerstone <<<board>>>
compiletoram
