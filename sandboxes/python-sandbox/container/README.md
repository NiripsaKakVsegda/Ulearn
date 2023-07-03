# python_sandbox

## Работа с Docker

### Создание образа
`docker build -t ulearn_sandbox .`
### Запуск контейнера
`docker run -it -p 80:80 --rm ulearn_sandbox bash`

## Как пользоваться

### cbr_parody api
Папка cbr_api защищена от шаловливых ручек студентов chmod'ом 600

В папке cbr_api запускаем сервер:

`python run_server.py`

Внутри контейнера он работает по адресу cbr.ru. Вне контейнера по 0.0.0.0. Проверить внутри контайнера можно следующим образом:

`curl http://cbr.ru`

### django
Создаем проект: 

`django-admin startproject <project_name>` 

Делаем миграции: 

`python manage.py migrate` 

Запускаем: 

`python manage.py runserver 0.0.0.0:80` 

После этого можно даже будет посмотреть на работу по адресу 127.0.0.1 
