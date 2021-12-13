# Получим директорию с установленным пакетом из первого аргумента
installationDirectoryPath=$1

# Пропишем путь до исполняемого файла в файл задачи. Разрешены только полные пути.
sudo sed -i "s@%ExecStart%@/bin/bash -c `dotnet $installationDirectoryPath/RunCheckerJob.dll`@" deploy/runcheckerjob.service

# Скопируем файл задачи в папку для созданных вручную задач
sudo cp deploy/runcheckerjob.service /etc/systemd/system

# Добавим задачу в автозапуск при старте системы
sudo systemctl enable runcheckerjob 

# Перезапустим демона задач, чтобы он перечитал файлы с диска
sudo systemctl daemon-reload

# Перезапустим или стартанем задачу
sudo systemctl restart runcheckerjob

(
	# Временно sandboxes - рабочая папка
	cd sandboxes/
	# Выполним build.sh всех docker-контейнеров
	sh build.sh
)