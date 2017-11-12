\ add your main application code here...
\ integrate stable code slowly from ex/*.fs scratch/exploration files

compiletoram? [if]  forgetram  [then]

\ toggle the led state
: led-toggle ( -- ) LED iox! ;

\ blink the led
task: blink
333 variable blink-pulse
: blink& ( -- )
  blink activate
  begin
    led-toggle
    blink-pulse @ ms
  again ;

\ change the the blink rate when the blue user button is pressed
task: blink-rate
: blink-rate& ( -- )
  blink-rate activate
  begin
    button? if
      blink-pulse @ 600 < if
        1000 blink-pulse !
      else
        333 blink-pulse !
      then
      begin pause button? 0= until  \ wait for button release
    then
    pause
  again ;

\ run the thing
: blinky ( -- )
  blink& blink-rate& multitask ;

\ Note
\ Num of completed multitask rounds per second with these blinky tasks
\ as measured using the rr-measure& tool in flib-my/multi-measure
\
\ 1000 rr-span !  multitask
\
\ rr-measure&  ok.
\ 76050 rounds in 1000 ms. \ 2 tasks (interactive + measure)
\ blink& rr-measure&  ok.
\ 53789 rounds in 1000 ms. \ 3 tasks ... + blink
\ blink-rate& rr-measure&  ok.
\ 37464 rounds in 1000 ms. \ 4 tasks ... + blink-rate
