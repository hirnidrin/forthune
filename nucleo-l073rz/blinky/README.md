### Blinky app for the ST Nucleo-L073RZ board

Blinky app with a twist. Notes:
* Press the blue USER button to change the blinking speed.
* Uses timer callbacks, not just a simple loop... *"Complexity sells better." (E.W. Dijkstra)*

Application structure and development workflow are as proposed by Jean-Claude Wippler. Quick intro:
* http://jeelabs.org/2016/06/thoughts-about-app-structure/
* http://jeelabs.org/article/1720a/
* https://github.com/jeelabs/folie

The board is loaded with the "spezial" build of the Forth core, see [forthune/cores](https://github.com/hirnidrin/forthune/tree/master/cores)
