# python_sandbox

## Работа с Docker

### Создание образа
`docker build -t ulearn_sandbox .`
### Запуск контейнера
`docker run -it --rm ulearn_sandbox bash`

## Как пользоваться

### django
Создаем проект: 

`django-admin startproject <project_name>` 

Делаем миграции: 

`python manage.py migrate` 

Запускаем: 

`python manage.py runserver 0.0.0.0:8000` 

После этого можно даже будет посмотреть на работу по адресу 127.0.0.1 
