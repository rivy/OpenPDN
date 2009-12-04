@rem Use this to count the number of lines of code in Paint.NET
@rem First number in the last row is the number of lines of code.
@rem This counts all our .cs, .c, .cpp, .h files
pushd ..
dir /b /s /a-d *.cs *.c *.cpp *.h > _list.txt
src\BuildTools\wc @_list.txt
del _list.txt
popd
pause
