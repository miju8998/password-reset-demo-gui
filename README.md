# Password Reset Demo GUI

This project is a C# Windows Forms application for demonstrating password creation, SHA256 hashing with static salt, and brute-force password recovery using both single-thread and multi-thread methods.

## Features

- Graphical interface using WinForms
- Random password generation with length [4-6)
- SHA256 password hashing with constant static salt
- Brute-force attack from length 1 up to maximum length 6
- Single-thread brute-force test
- Multi-thread brute-force test using CPU cores - 1
- Progress display
- Elapsed time display
- Found password output
- Performance log comparing single-thread and multi-thread execution

## GitHub Versions

Version 1 - GUI, password generation, SHA256 hashing, and validation.
Version 2 - single-thread and multi-thread brute force implementation.
Version 3 - performance logging, stop function, UML diagram, and final testing.
Commit note: Version 2 confirms single-thread and multi-thread brute force functionality.
