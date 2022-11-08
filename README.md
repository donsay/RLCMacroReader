# RLCMacroReader
This is a VS2019 CSharp Net Core 3.1 Console app for reading the macros from an RLC Club repeater controller.
It executes command 054 on macros 200-499 (Automatic) and 500-999 (user defined), and saves the data to 
text file specified on the command line.

Usage:
RLCMacroReader [cfg] - show comm settings
RLCMacroReader [cfg <portname,baudrate,parity,databits,stopbits>] - set comm settings, ie:com3,19200,None,8,1
RLCMacroReader [f <filename>] - read macros, and save to the file named <filename>

