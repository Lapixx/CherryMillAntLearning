python "%~dp0tools\playgame.py" -So --engine_seed 42 --player_seed 42 --end_wait=0.25 --verbose --log_dir game_logs --turns 1000 --map_file "%~dp0tools\maps\maze\maze_02p_02.map" %* "CherryMillAnt\bin\Release\CherryMillAnt.exe" "python ""%~dp0tools\sample_bots\python\LeftyBot.py""" | java -jar tools\visualizer.jar
pause
echo "hi"