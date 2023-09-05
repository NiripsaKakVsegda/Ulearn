FROM python:3.11-slim

ENV TZ=Europe/Ekaterinburg

RUN apt-get update && apt-get -y upgrade && apt-get install -y wget && apt-get install -y curl \
	&& apt-get install -y gcc && apt-get install -y nano && rm -rf /var/lib/apt/lists/*
COPY ./app/ /app/

WORKDIR app

COPY requirements.txt .

RUN pip install --no-cache-dir -r /app/requirements.txt

RUN useradd student && chmod 700 /app/tests && chmod 700 /app/main.py && chmod 777 /app/student_works
