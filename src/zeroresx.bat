@echo off
rem This will zero out all the resx files. It will cut down on the exe and dll sizes.
type nul > zerobyte
for %%f in (*.resx) do copy /y zerobyte %%f
del zerobyte
