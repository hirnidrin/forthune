\ add your main application code here...
\ integrate stable code slowly from ex/*.fs scratch/exploration files

compiletoram? [if]  forgetram  [then]

: led-toggle ( -- ) LED iox! ;

: change-blink-speed ( -- )
  button? if
    0 tmd-inte-addr dup  \ timer #0 interval variable
    @ 600 < if
      1000 over !        \ write straight into it...
    else
      333 over !         \ ...to change the blink speed
    then
    drop  begin pause button? 0= until  \ wait for button release
  then ;

: blinky ( -- )
  timed-init
  ['] led-toggle 1000 0 call-every          \ timed slot#0 toggles the LED
  ['] change-blink-speed 200 1 call-every ; \ timed slot#1 checks the button
