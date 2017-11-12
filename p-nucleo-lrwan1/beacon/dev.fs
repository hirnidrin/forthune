\ add your main application code here...
\ integrate stable code slowly from ex/*.fs scratch/exploration files

compiletoram? [if]  forgetram  [then]

\ this part will go to ../board.fs when stable
include ../../flib-my/spi/sx1272lora.fs

\ here comes the app
