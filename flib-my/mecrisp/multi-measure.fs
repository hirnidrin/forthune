\ Simple measurement tool to get a ballpark estimate of task switching frequency.
\ - counts and prints the number of completed round-robin rounds within a given time span
\ - can run along any number of tasks... more tasks -> less rounds
\ - runs as task, does not block the interactive console task
\ - just invoke rr-measure& on top of an already running multitask environment

task: rr-measure
\ task params
1000 variable rr-span  \ how long to measure, in ms, default 1 second
0 variable rr-count    \ result var that reports the counted rounds
\
: rr-measure&
  rr-measure activate
  0 rr-count !
  millis rr-span @ +   \ upper loop boundary (end time in ms)
  begin
    1 rr-count +!
    dup millis <       \ time's up?
    pause
  until drop
  rr-count @  . ." rounds in " rr-span @ . ." ms." cr
  stop ;

\ example: num of multitask rounds per second
\
\ 1000 rr-span !  multitask
\ rr-measure&
\ 76050 rounds in 1000 ms.  \ sample result with 2 tasks (interactive + measure)
