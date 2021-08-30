# Проверяет наличие docker и билдит все контейнеры
# Рабочая директория должна быть sandboxes
# Проверим, что docker существует
if !(type docker >/dev/null 2>&1)
then
	echo >&2 "Docker is not installed"
else
	# Выполним build.sh всех docker-контейнеров
	for filename in */container/build.sh
	do
		# Если не найдено ни одного файла, в filename передастся сам шаблон, поэтому проверяем, что файл существует
		[ -f "$filename" ] || continue
		# Путь до файла скрипта без имени файла
		container_dir=$(dirname $filename)
		echo Start build container with $filename
		# В скобках временно сменим рабочую директорию, после выхода из скобок она вернется на старую
		# Рабочая директория должна быть директорией с файлом build.sh, чтобы пути в нем были относительно неё
		(
			cd $container_dir
			sh build.sh
		)
	done
fi
