RLCMacroReader is provided FREE of charge.

Reasonable effort has been made to insure that it will not have any adverse effects on connected devices.
However, the author assumes no liability for any loss of data.

Setup:
The application uses a serial port, which must be configured before use.
Use
rlcmacroreader cfg <port,baud,parity,databits,stopbits>

For example,
rlcmacroreader cfg com1,9600,n,8,1

9600,n,8,1 is the default setup for the RLC Club controller.

Running the application:
The application executes controller command 054 on macro numbers 200-999, and writes the data
to a user specified file. Only macros that have data will be saved. The date and time are automatically
appended to the file name.

Note: if command 054 has been renamed in the controller, this application may behave unpredictably.

For example,
rlcmacroreader f macros.txt

In some cases, the macro command(s) may be under a user level. The application will prompt for a user number
and a password. If no user levels have been created, and the 054 command has not been assigned a user level,
just press <Enter> twice to skip, and the application will attempt to read the macros without logging in to
the controller. If a user level has been assigned to command 054, and no user/password supplied, the application
will display an error message and halt.

