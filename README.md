# CCam
CCam is a Windows-based application that has been developed using Visual Studio Community and the C# programming language. Visual Studio Community is a free and powerful IDE that supports multiple programming languages and provides an intuitive and user-friendly environment with various features such as code highlighting and debugging. CCam is a user-friendly and efficient application that enables users to capture and store images from their camera with ease. Capturing can be done manually by pressing a button or automatically via commands sent through serial communication. Initially developed for research purposes, CCam has a broader application scope beyond the laboratory and can be utilized for various experiments in different fields.

# Instructions for Use:

1. Select the camera and its resolution
2. Click Start to begin the live video
3. Click Stop to stop the live video
4. Click Capture to take a picture
5. Click Path to choose the Output Folder
6. Click Save to save the captured image
7. The file name can be entered in the Filename column

# Serial Usage:

1. Select the Port and Baudrate
2. Click Connect to start the serial communication
3. Click Disconnect to stop the serial communication

# Capture Command Format:
```
[CI:Filename]\n
or
[CI=Filename]\n
or
CI:Filename\n
or
CI=Filename\n
```
where '\n' is the endline character.

# Example Arduino code snippet
Here's an example Arduino code snippet for sending capture command through the serial communication:

```
// send command to serial port
Serial.print("CI="); // Header
Serial.println(FileName); // Filename string and end with endline (\n)
```
