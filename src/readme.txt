Paint.NET Source Code "Read Me"
-------------------------------
There are a few important things to know about the Paint.NET source code
release. Think of this as a "Frequently Asked Questions" ... please read it!

* The source code is not supported in any way. It is released primarily so that
  others may study and learn from it, and so that plugin developers can have
  something to work with. 
  
* There is no design document, or other documentation of any kind.

* The installer is not included in the source code release. Its source code is
  private and *not* available upon request. We have our reasons for doing this,
  so please respect that and do not inquire further.

* The non-English RESX files (string resources) are not included in the source
  code release.
  
* The forms in the project do not work in the Visual Studio designer. We do 
  not use the designer, so this is not a bug nor is it something that will be
  "fixed."

* The Paint.NET development team does not accept unsolicited contributions.
  Please do not send us code or patches.

Prerequisites
-------------
1. Windows XP, Windows Vista, Windows Server 2003/2008 

2. Visual Studio .NET 2008 Professional Edition
   You should install the C++ x64 compiler as well, which may not come installed
   by default. The Express editions might work, but we don't know for sure since
   we don't use them.

Instructions
------------
For normal development work, use 'Debug' configuration. When you are working in
this mode, you should make sure that the /skipRepairAttempt command line 
parameter is present in the Debug tab of the 'paintdotnet' project's properties.
Otherwise, Paint.NET will see that some files are missing and attempt to repair
itself. Not all of these files are necessary when doing development or debugging.

Also, you will need to make sure that mt.exe and signtool.exe are in a
directory that is in your PATH. These are available as part of the Windows SDK
which can be found at Microsoft's website. Usually it's sufficient to copy
these to %SYSTEMROOT%, which is usually C:\Windows.
