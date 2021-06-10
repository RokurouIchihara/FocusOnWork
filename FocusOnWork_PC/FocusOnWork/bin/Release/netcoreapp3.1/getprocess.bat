@echo off
tasklist /nh > process.txt

setlocal enabledelayedexpansion

set n=0

for /f "tokens=1, 3, 5" %%a in (process.txt) do (
	if not %%b == Services (
		if !n! == 0 (
			echo %%a %%c > result.txt
		) else (
			echo %%a %%c >> result.txt
		)
	)

	set /a n=n+1
)

set n =

endlocal