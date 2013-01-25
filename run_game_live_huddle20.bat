python "%~dp0tools\playgame.py" -So --engine_seed 42 --player_seed 42 --end_wait=0.25 --verbose --log_dir game_logs --turns 1000 --map_file "%~dp0tools\maps\huddle20.map" %* "CherryMillAnt\bin\Release\CherryMillAnt.exe" "python ""%~dp0dommebot\MyBot.py3""" | java -jar tools\visualizer.jar
pause
echo "hi"