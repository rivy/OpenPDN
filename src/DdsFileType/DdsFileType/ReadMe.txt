DdsFileType : A file type plugin for Paint.NET
----------------------------------------------

Overview
--------

This is a file type plugin for Paint.NET, which allows for the
loading & saving of a limited number of types of DDS file. 
It supports DXT1, DXT3, DXT5, A8R8G8B8, X8R8G8B8, A8B8G8R8, X8B8G8R8,
A4R4G4B4, A1R5G5B5, R8G8B8, and R5G6B5 formats, and can generate
mipmaps for you, should you so wish.

Options
-------

When saving a DDS file, you can adjust options using the GUI. Use
the drop-down list to select the type of file you wish to output.

You can customise the compressor type and colour error metric to use
when outputting DXT files. For compressor type you can choose from a
slow (but high quality) iterative fit, a slightly faster non-iterative
cluster fit, or a faster (but lower quality) range fit.The error
metric setting allows you to tune the compression behaviour based on
whether it's a texture that's going to be viewed normally (where the
eyes colour perception properties are taken into consideration), or
whether it's going to be used as input to some other function/process
(where the actual numeric values are more important than how it looks).

An additional flag can also be set when using the cluster fit options, 
that that weights the colour of each pixel by its alpha value. For
images rendered using alpha blending, this can significantly increase
the perceived quality.

For all image formats, you can enable the generation of mipmaps. This
creates all levels down to 1x1.

Installation
------------

Place the three DLLs in the binary package into the 'FileTypes'
directory in your Paint.NET installation location.

License
-------

This work is distributed under the terms and conditions of the MIT 
license. This license is specified at the top of each source file and
must be preserved in its entirety.

Feedback & Bug Reporting
------------------------

Feedback should be sent to me via the contact form on my homepage,
http://www.dmashton.co.uk, or via posts on the Paint.NET forums.
