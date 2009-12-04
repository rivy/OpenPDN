@rem Usage: compress buildToolsDir output.zip filespec
"%1\7z" u -tzip %2 %3 -mx9 -mfb257 -mmt -mpass15
