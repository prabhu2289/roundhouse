﻿@echo off

SET DIR=%~d0%~p0%

SET deploy.settings="%DIR%..\settings\${environment}.settings"

"%DIR%NAnt\nant.exe" /f:"%DIR%scripts\database.deploy" -D:deploy.settings=%deploy.settings% -D:restore=true
::"%DIR%NAnt\nant.exe" /f:"%DIR%scripts\database.deploy" -D:dirs.db.project=db -D:server.name=(local) -D:database.name=TestRoundhousE

pause