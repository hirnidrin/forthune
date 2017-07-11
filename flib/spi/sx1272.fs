\ Semtech SX1272 driver
\ Lora mode only, work in progress

\ pin mappings SX1272 chip <--> MCU GPIO
\ spi mappings already defined when loading board.fs / spi.fs
[ifndef] RST6  PA0  constant RST6  [then]  \ pin 6 "RESET" of the chip
[ifndef] DIO0  PA10 constant DIO0  [then]
[ifndef] DIO1  PB3  constant DIO1  [then]
[ifndef] DIO2  PB5  constant DIO2  [then]
[ifndef] DIO3  PB4  constant DIO3  [then]

\ chip register map
\ common settings
$00 constant SX:FIFO
$01 constant SX:OP

$06 constant SX:FRFMSB
$07 constant SX:FRFMID
$08 constant SX:FRFLSB

$3C constant SX:TEMP
$42 constant SX:VERSION

\ register bit helpers
\ for SX:OP
$7F constant SX:OP_LRM  \ LongRangeMode bit mask
  0 7 lshift constant SX:OP_LRM_FSK
  1 7 lshift constant SX:OP_LRM_LORA
$CF constant SX:OP_ASR  \ AccesSharedReg bit mask
  0 6 lshift constant SX:OP_ASR_LORA
  1 6 lshift constant SX:OP_ASR_FSK
$F8 constant SX:OP_M  \ Mode 3 bit mask
  0 0 lshift constant SX:OP_M_SLEEP
  1 0 lshift constant SX:OP_M_STDBY
  2 0 lshift constant SX:OP_M_FSTX
  3 0 lshift constant SX:OP_M_TX
  4 0 lshift constant SX:OP_M_FSRX
  5 0 lshift constant SX:OP_M_RX
  6 0 lshift constant SX:OP_M_RXSINGLE
  7 0 lshift constant SX:OP_M_CAD

: sx!op ( cval cmask -- )  \ write some bits to RegOpMode
  SX:OP spi2>  and or  SX:OP $80 or >spi2 ;  \ need to wait til chip ready?

: sx-sleep ( -- )  \ put radio module to sleep
  SX:OP_M_SLEEP SX:OP_M sx!op ;

: sx-init ( -- )
  spi-init
;
