FROM postgres:latest

ENV \
	POSTGRES_USERNAME=postgres \
	POSTGRES_PASSWORD=postgres \
	POSTGRES_DB=postgres \
	TZ=Europe/Ekaterinburg \
	DEBIAN_FRONTEND=noninteractive
	
run apt update && \
	apt install -y openjdk-17-jdk && \
    apt install -y maven
	
WORKDIR /
RUN mkdir /app
WORKDIR app