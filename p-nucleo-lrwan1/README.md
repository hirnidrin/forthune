### Apps based on the ST P-NUCLEO-LRWAN1 board combo

The combo consists of two boards:
* ST [Nucleo-L073RZ](http://www.st.com/en/evaluation-tools/nucleo-l073rz.html) MCU development board
* Semtech [SX1272MB2xAS](https://developer.mbed.org/components/SX1272MB2xAS/) FSK/Lora modem add-on board

Notes:
* The MCU board is loaded with the "spezial" build of the Forth core, see [forthune/cores](https://github.com/hirnidrin/forthune/tree/master/cores).
* The Forth core is configured for the USART2 serial connection that is made available by the ST-Link/V2-1 USB-serial adapter of the Nucleo-L073RZ board.
* Use the always.fs code from the forthune/nucleo-l073rz directory, as this is the same MCU board.

Application structure and development workflow are as engineered by Jean-Claude Wippler. Quick intro:
* http://jeelabs.org/2016/06/thoughts-about-app-structure/
* http://jeelabs.org/article/1720a/
* https://github.com/jeelabs/folie
