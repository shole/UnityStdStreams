# Unity Standard Streams
This project is designed to allow stupid-easy integration of light data communication between Unity and external applications through commandline Standard Input and Standard Output (familiar to us nerds as StdOut & StdIn)

The project introduces two classes;
* StreamedProcess - with which you can spawn and communicate with a single process in any code
* StreamedProcessPool - which allows you to spawn an arbitrary number of identical processes which accept pooled orders from a controlling script that you write

This isn't meant to be a comprehensive multiprocess communication framework or somesuch - but a stupid-easy integration of light, very simple marco-polo, call and response type communication.
You hook up handler function to accept StdOut, write to process StdIn, and after the process has done it's thing, it drops into your StdOut function.
In StreamedProcessPool processes that haven't responded yet are marked as busy and the next available process is sent to instead, or accumulating a queue if all are.

## Usage
See *Example/testscene.unity* for an example of usage.

For minimal use you need a *StreamedProcessPool* component with your process and processpool settings, and your own script that controls the processpool.

In your script you need to get the ProcessPool reference and call *ref.StdIn(str)* with a string to send a message.

To receive messages hook a method to *ref.StdOut* of type *bool StdOut(StreamedProcess proc, string message)*.
The return value denotes if process should now be ready to accept more input, usually detected with a special string like "ready" etc.

Also, this is fine; *ref.StdOut=(process,message)=>{ Debug.Log(message); return true; }*

Seriously though, read the example.

## Licence
This project is licenced with Creative Commons By Attribution 4.0 licence with attribution for Paavo Happonen, Teatime Research Ltd.
