echo off
rem Проверяет наличие docker и билдит все контейнеры
rem На вход скрипту можно передать путь до папки sandboxes. Либо можно не передавать, и запкускать скрипт из неё
rem Отключили вывод исполняемых команд на консоль
rem Проверим, есть ли команда docker. Если есть, %errorlevel% будет равен 0, иначе 1. 1>nul отключает печать вывода команды where на stdout, 2>nul на stderr
where docker 1>nul 2>nul
if %errorlevel% neq 0 (
	echo Docker is not installed or not in PATH
) else (
	rem %1 - первый аргумент командной строки
	rem Ищем все файлы, чей путь подходит под регулярное выражение .Сам путь будет в %%I
	for /F "delims=" %%I in ('"dir /B /S | findstr /E "%1\\.*\\container\\build\.bat""') do (
		rem Сменим рабочую директорию на то, что лежит в %%I, но без имени файла.
		pushd %%~dpI
		echo Start build container with %%I
		call %%I
		rem Откатим смену рабочей директории
		popd
	)
)