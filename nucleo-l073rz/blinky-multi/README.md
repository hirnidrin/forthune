### Blinky app for the ST Nucleo-L073RZ board

Blinky app using the multitasking (multi.fs) environment. Notes:
* 3 tasks: blink, blink-rate, standard interactive console.
* Press the blue USER button to change the blinking speed.

Application structure and development workflow are as proposed by Jean-Claude Wippler. Quick intro:
* http://jeelabs.org/2016/06/thoughts-about-app-structure/
* http://jeelabs.org/article/1720a/
* https://github.com/jeelabs/folie

The board is loaded with the "spezial" build of the Forth core, see [forthune/cores](https://github.com/hirnidrin/forthune/tree/master/cores).
I use the USART2 serial connection that is made available by the ST-Link/V2-1 USB-serial adapter of the Nucleo-L073RZ board.
