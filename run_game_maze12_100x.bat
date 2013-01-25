@echo off
set ronde = 0
:aanvang
cls
set /a ronde = ronde + 1
echo Ronde %ronde%...
start "GAME" /MIN /WAIT python "%~dp0tools\playgame.py" -So --engine_seed 42 --player_seed 42 --end_wait=0.25 --verbose --log_dir game_logs --turns 1000 --map_file "%~dp0tools\maps\maze12.map" %* "CherryMillAnt\bin\Release\CherryMillAnt.exe" "python ""%~dp0dommebot\MyBot.py3"""
if not %ronde% == 100 goto aanvang
cls
echo Klaar!
pause