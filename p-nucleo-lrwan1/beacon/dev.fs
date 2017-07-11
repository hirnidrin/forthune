\ add your main application code here...
\ integrate stable code slowly from ex/*.fs scratch/exploration files

compiletoram? [if]  forgetram  [then]

\ this part will go to ../board.fs when stable
include ../../flib/spi/sx1272.fs

\ here comes the app
