FROM postgres:alpine

ENV \
	POSTGRES_USERNAME=postgres \
	POSTGRES_PASSWORD=postgres \
	POSTGRES_DB=postgres \
	TZ=Europe/Ekaterinburg \
	DEBIAN_FRONTEND=noninteractive
	
run apk add openjdk17 && \
    apk add maven
    
ARG JAR_FILE=*.jar
COPY ${JAR_FILE} application.jar