\ Hardware I2C driver for STM32F103.
\ Needs: ring.fs

\ Define pins
[ifndef] SCL  PB6 constant SCL  [then]
[ifndef] SDA  PB7 constant SDA  [then]

\ Buffers
[ifndef] i2c-bufsize 128 constant i2c-bufsize [then]

i2c-bufsize 4 + buffer: i2c.txbuf
i2c-bufsize 4 + buffer: i2c.rxbuf
i2c.txbuf i2c-bufsize init-ring
i2c.rxbuf i2c-bufsize init-ring

\ Variables
0 variable i2c.addr
0 variable i2c.cnt
0 variable i2c.collect

\ Register definitions
$40005400 constant I2C1
$40005800 constant I2C2
     I2C1 $00 + constant I2C1-CR1
     I2C1 $04 + constant I2C1-CR2
     I2C1 $10 + constant I2C1-DR
     I2C1 $14 + constant I2C1-SR1
     I2C1 $18 + constant I2C1-SR2
     I2C1 $1C + constant I2C1-CCR
     I2C1 $20 + constant I2C1-TRISE
\ $40021000 constant RCC
     RCC $10 + constant RCC-APB1RSTR
     RCC $14 + constant RCC-AHBENR

    [ifndef] DMA1 $40020000 constant DMA1 [then]

    DMA1      constant DMA1-ISR
    DMA1 4  + constant DMA1-IFCR
    
    DMA1 20 6 1- * + 
    dup $08 + constant DMA1-CCR6
    dup $0c + constant DMA1-CNDTR6
    dup $10 + constant DMA1-CPAR6
    dup $14 + constant DMA1-CMAR6
    drop
    
    DMA1 20 7 1- * + 
    dup $08 + constant DMA1-CCR7
    dup $0c + constant DMA1-CNDTR7
    dup $10 + constant DMA1-CPAR7
    dup $14 + constant DMA1-CMAR7
    drop
         
    $E000E000 constant NVIC  
    NVIC $100 + constant NVIC_ISER0 

\ Register constants
3  bit constant APB2-GPIOB-EN
0  bit constant APB2-AFIO-EN
21 bit constant APB1-I2C1-EN
21 bit constant APB1-RST-I2C1

0  bit constant i2c-SR1-SB
1  bit constant i2c-SR1-ADDR
2  bit constant i2c-SR1-BTF
6  bit constant i2c-SR1-RxNE
7  bit constant i2c-SR1-TxE
10 bit constant i2c-SR1-AF
0  bit constant i2c-SR2-MSL 
1  bit constant i2c-SR2-BUSY

\ Bit setting
: i2c-ACK-1 ( -- ) 10 bit I2C1-CR1 hbis! ;
: i2c-ACK-0 ( -- ) 10 bit I2C1-CR1 hbic! ;

\ Status checks
: i2c-SR1-flag? I2C1-SR1 hbit@ ;
: i2c-SR2-flag? I2C1-SR2 hbit@ ;
: i2c-SR1-wait  ( u -- )   begin dup i2c-SR1-flag?    until drop ;
: i2c-SR2-!wait ( u -- )   begin dup i2c-SR2-flag? 0= until drop ; 

\ debugging
: i2c? cr I2C1-CR1 h@ hex. I2C1-CR2 h@ hex. I2C1-SR1 h@ hex. I2C1-SR2 h@ hex. ;

\ Low level register setting and reading
: i2c-start! ( -- ) 8 bit I2C1-CR1 hbis! ;
: i2c-stop!  ( -- ) 9 bit I2C1-CR1 hbis! ;
: i2c-start  ( -- ) \ set start bit and wait for start condition
  i2c-start! i2c-SR1-SB i2c-SR1-wait ;
: i2c-stop   ( -- ) \ stop and wait
  i2c-stop! i2c-SR2-MSL i2c-SR2-!wait ; 
: i2c-DR!    ( u -- )  I2C1-DR c! ;            \ Writes data register

\ STM Events
: i2c-EV8_2 i2c-SR1-BTF  i2c-SR1-wait ;
: i2c-EV6a i2c-SR1-ADDR i2c-SR1-AF or i2c-SR1-wait ; \ performs the wait, does not clear ADDR
: i2c-EV6b I2C1-SR1 h@ drop I2C1-SR2 h@ drop ;       \ clears ADDR
: i2c-EV6 i2c-EV6a i2c-EV6b ;                        \ Performs full EV6 action
: i2c-EV6_3
  i2c-EV6a
  i2c-SR1-AF i2c-SR1-flag?                   \ Put NAK on stack
  i2c-SR1-AF i2c1-SR1 hbic!                  \ Clear the NAK fl
  i2c-ACK-0
  i2c-EV6b
  i2c-stop! ;
: i2c-EV7   i2c-SR1-RxNE i2c-SR1-wait ;

: i2c-addr     ( u --)  \ Start a new transaction
  i2c-SR2-BUSY begin pause dup i2c-SR2-flag? 0= until drop 

  i2c.txbuf i2c-bufsize init-ring
  i2c.rxbuf i2c-bufsize init-ring
  shl i2c.addr !

  i2c-ACK-1                                   \ reset ack in case we had an rx-1 before
;

: i2c-dma-enable ( channel -- )
  11 bit I2C1-CR2  hbis!                      \ DMAEN
  20 * DMA1 20 - 8 + +                        \ DMA1-CCRch 6 120+dma1-20+8
  0 bit swap bis!
;

: i2c-dma-disable ( channel -- )
  11 bit I2C1-CR2 hbic!
  0 bit
  swap 20 * DMA1 20 - 8 + +                   \ DMA1-CCRch
  bic!
;

: i2c-irq-err? ( channel -- )                 \ Was the irq triggered by error?
  1- 4 * 3 + bit DMA1-ISR bit@ inline ;       \ TEIFx 


: i2c-irq-done? ( channel -- )                \ Was the irq for complete xfer?
  1- 4 * 1 + bit DMA1-ISR bit@ inline ;       \ TCIFx

: i2c-send-addr             ( rx? -- nak )
  i2c.addr @  or i2c-DR!                     \ calculate address
  i2c-EV6a                                   \ Wait for addr to happen
  i2c-SR1-AF i2c-SR1-flag?                   \ Put NAK on stack
  i2c-SR1-AF i2c1-SR1 hbic!                  \ Clear the NAK fl
  i2c-EV6b                                   \ end addr
;

: i2c-rx-1
  i2c.addr @ 1 or i2c-DR!   \ Read address
  i2c-EV6_3
  i2c-EV7
  I2C1-DR @ i2c.rxbuf >ring
;

\ Dispatches all the DMA interrupts TODO call of collect not yet working
: i2c-irq-dispatch ( -- )
  ipsr dup $fe and  32 <> if drop i2c.collect @ execute exit then  ( ipsr)
  \ i2c.irq-handler @ execute
  case
    32 of \ TX event: stop or RX 
      6 i2c-irq-done? if
        6 i2c-dma-disable
        21 bit DMA1-IFCR  bis!                    \ CTCIF6
        i2c.cnt @ case
          0 of
            i2c-EV8_2 i2c-stop
            begin 9 bit I2C1-CR1 hbit@ 0= until   \ Wait for STOP to clear
          endof
          1 of
            \ Wait for TxE: don't clobber last byte
            i2c-SR1-TxE i2c-SR1-wait
            i2c-start                             \ Restart
            i2c-rx-1 drop                         \ discard nak
          endof
          ( #rx )
          \ Wait for TxE: don't clobber last byte
          i2c-SR1-TxE i2c-SR1-wait
          \ Configure DMA 7
          7 i2c-dma-enable
          \ Start rx (will wait for I2C1 to be ready)
          12 bit I2C1-CR2  hbis!                  \ LAST
          \ Send restart  
          i2c-start                               \ Restart
          1 i2c-send-addr drop
        endcase
      else i2c-stop then                        \ Error irq: give up
    endof
    33 of \ RX event: always stop
      7 i2c-irq-done? if
        7 i2c-dma-disable
        25 bit DMA1-IFCR  bis!                    \ CTCIF7
        i2c-stop
        begin 9 bit I2C1-CR1 hbit@ 0= until       \ Wait for STOP to clear
        \ Let RX buffer know of new data
        i2c.cnt @ i2c.rxbuf 1+ c!
      else i2c-stop then                          \ Error irq: give up
    endof
  endcase
  i2c.collect @ execute
;

\ APB1 speed
: apb1-hz ( -- u )
  RCC-CFGR @ 
  dup $80 and if 
    dup $70 and 4 rshift
    dup 2 bit and if 1+ then
    1+ bit 
  else 1 
  then   \ HPRE prescaler
  swap ( HPRE CFGR)
  dup 10 bit and if $300 and 8 rshift 1+ bit else drop 1 then   \ PPRE1 prescaler
  * clock-hz @ swap /
;

: i2c-standard ( -- )   \ Configure I2C for Standard Mode (~100kHz)
  0  bit  I2C1-CR1 hbic!                          \ Disable peripheral
  i2c-SR2-BUSY begin pause dup i2c-SR2-flag? 0= until drop 
  
  $3F                   I2C1-CR2   hbic!          \ CLEAR FREQ field
  apb1-hz 1000    /                               \ APB1 speed (kHz)
  dup     1000    / dup I2C1-CR2   hbis!          \ Set current clock speed (MHz)
  1+                    I2C1-TRISE h!             \ APB1(MHz)+1 for 1000ns SCL
  ( kHz)  2 100 * /     I2C1-CCR   h!             \ Select 100 kHz Fast mode
  0  bit 10 bit or      I2C1-CR1   hbis!          \ Enable peripheral & ACK
;

\ Configure I2C for Fast Mode
: i2c-fast     ( -- )   \ Configure I2C for fast mode (~400kHz)
  0  bit  I2C1-CR1 hbic!                          \ Disable peripheral
  i2c-SR2-BUSY begin pause dup i2c-SR2-flag? 0= until drop 
  
  $3F                         I2C1-CR2   hbic!      \ CLEAR FREQ field
  apb1-hz 1000    /                                 \ APB1 speed (kHz)
  dup     1000    / dup       I2C1-CR2   hbis!      \ Set current clock speed (MHz)
  3 / 1+                      I2C1-TRISE h!         \ APB1(MHz)/3+1 for 333ns SCL
  ( kHz) 5000 + 25 400 * /                          \ Calculate ccr, round up
  $c000 or                    I2C1-CCR   h!         \ Select 400kHz Fast Mode 16:9 duty
  0  bit 10 bit or            I2C1-CR1   hbis!      \ Enable peripheral & ACK
;

: i2c-init     ( -- )   \ Init and reset I2C. Default to 100 kHz
  \ Reset I2C1
  APB1-RST-I2C1 RCC-APB1RSTR bis!
  APB1-RST-I2C1 RCC-APB1RSTR bic!

  \ init clocks
  APB2-GPIOB-EN APB2-AFIO-EN or RCC-APB2ENR bis!
  APB1-I2C1-EN                  RCC-APB1ENR bis!

  \ init GPIO
  IMODE-FLOAT SCL io-mode!  \ edited: manual says use floating input
  IMODE-FLOAT SDA io-mode!
  OMODE-AF-OD OMODE-FAST + SCL io-mode!  \ I²C requires external pullup
  OMODE-AF-OD OMODE-FAST + SDA io-mode!  \     resistors on SCL and SDA

  \ Reset I2C peripheral
   15 bit I2C1-CR1 hbis!
   15 bit I2C1-CR1 hbic!

  \ Enable I2C peripheral
  21 bit RCC-APB1ENR bis!  \ set I2C1EN
  i2c-standard
  i2c-SR2-BUSY begin pause dup i2c-SR2-flag? 0= until drop 

  0  bit RCC-AHBENR bis!   \ Enable DMA peripheral clock
  0  bit DMA1-CCR6  bic!   \ Disable it for now (ch 6 = I2C1 TX)
  0  bit DMA1-CCR7  bic!   \ Disable it for now (ch 7 = I2C1 RX)

  i2c.collect @ 0= if
    irq-collection @ dup ['] unhandled = if
      drop
      ['] nop
    then
    i2c.collect ! 
    ['] i2c-irq-dispatch irq-collection !
  then
 ;

\ The meat of the I2C driver is in this function
: i2c-xfer     ( n -- nak ) \ Prepares for reading an nbyte reply.
  i2c.cnt !                                  \ Store #rx
  i2c.txbuf ring# 0=      ( #tx=0   )

  \ Configure I2C1 peripheral
  12 bit I2C1-CR2  hbic!                     \ LAST
  \ Configure DMA 6
  %0011000010011010 DMA1-CCR6   !
  16 bit            NVIC_ISER0  !
  i2c.txbuf 4 +     DMA1-CMAR6  !
  I2C1-DR           DMA1-CPAR6  !
  i2c.txbuf ring#   DMA1-CNDTR6 !            \ Count
  \ Configure DMA 7
  %0011000010001010 DMA1-CCR7   !   
  17 bit            NVIC_ISER0  !
  i2c.rxbuf 4 +     DMA1-CMAR7  !
  I2C1-DR           DMA1-CPAR7  !
  i2c.cnt @         DMA1-CNDTR7 !            \ Count

  if \ #TX=0
    i2c.cnt @ case
      0 of                                   \ Probe
        i2c-start
        0 i2c-send-addr
        i2c-stop
      endof
      1 of                                   \ Receive 1 byte
        i2c-start
        i2c-rx-1
        i2c-stop
      endof
      ( #rx)
      12 bit I2C1-CR2  hbis!                 \ LAST
      7 i2c-dma-enable
      i2c-start
      1 i2c-send-addr swap ( nak #rx)
      \ DMA will handle rx from here on
    endcase
  else \ #TX > 0
    6 i2c-dma-enable
    i2c-start
    0 i2c-send-addr
    \ DMA will handle tx from here, then stop
  then
  \ If nak, stop communication
  dup 0<> if
    i2c-stop
    i2c-SR1-AF i2c1-SR1 hbic!                \ Clear the NAK flag; necessary?
    9 bit I2C1-CR1 hbic! 
  then
;

: >i2c         ( u -- ) \ Queues byte u for transmission over i2c. Use after i2c-addr
  i2c.txbuf >ring ;

: i2c>         ( -- u ) \ Receives 1 byte from i2c. Use after i2c-xfer. Waits.
  begin i2c.rxbuf ring# 0<> until 
  i2c.rxbuf ring> ;

: i2c>h        ( -- u ) \ Receives 16 bit word from i2c, lsb first.
    i2c>   i2c>  8 lshift or ;

: i2c>h_inv    ( -- u ) \ Receives 16 bit word from i2c, msb first.
    i2c>  8 lshift i2c>  or ;

: i2c-probe ( addr -- nak )
  i2c-addr 0 i2c-xfer
  ;

: i2c.         ( -- )   \ scan and report all I2C devices on the bus
  128 0 do
    cr i h.2 ." :"
    16 0 do  space i j +
      dup $08 < over $77 > or if drop 2 spaces else
        dup i2c-probe if drop ." --" else h.2 then
      then
    loop
  16 +loop ;
