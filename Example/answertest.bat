@echo off
setlocal enableextensions
:start
	REM ready for input
	echo READY
	set /p number=
	
	REM simulate processing time
	REM there is no real wait in batch, so just ping self
	ping 127.0.0.1 -n 2 > nul
	
	REM return answer
	set /a result=%number%*2
	echo %result%
goto start