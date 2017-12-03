# Unity Standard Streams
This project is designed to allow stupid-easy integration of light data communication between Unity and external applications through commandline Standard Input and Standard Output (familiar to us nerds as StdOut & StdIn)

The project introduces two classes;
* StreamedProcess - with which you can spawn and communicate with a single process in any code
* StreamedProcessPool - which allows you to spawn an arbitrary number of identical processes which accept pooled orders from a controlling script that you write

This isn't meant to be a comprehensive multiprocess communication framework or somesuch - but a stupid-easy integration of light, very simple marco-polo, call and response type communication.
You hook up handler function to accept StdOut, write to process StdIn, and after the process has done it's thing, it drops into your StdOut function.
In StreamedProcessPool processes that haven't responded yet are marked as busy and the next available process is sent to instead, or accumulating a queue if all are.

