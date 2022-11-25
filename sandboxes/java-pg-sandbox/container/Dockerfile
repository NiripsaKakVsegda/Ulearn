FROM postgres:alpine

ENV \
    POSTGRES_USERNAME=postgres \
	POSTGRES_PASSWORD=postgres \
	PGDATA=/var/lib/pgsql/data/ \
	POSTGRES_DB=java-rtf \
	DEBIAN_FRONTEND=noninteractive \
	DOCKER_ENTRYPOINT=/usr/local/bin/docker-entrypoint.sh \
	TZ=Europe/Ekaterinburg
	
COPY ./db/ /db/
COPY postgres.conf.d/* /etc/postgres/conf.d/

RUN set -euo pipefail \
    && apk add --no-cache --upgrade grep \
    && echo '1) Run postgres DB internally for init cluster:' \
            && bash -c "$DOCKER_ENTRYPOINT postgres --autovacuum=off &" \
            && sleep 10 \
		&& echo '2) Configure postgres: use conf.d directory:' \
            && sed -i "s@#include_dir = '...'@include_dir = '/etc/postgres/conf.d/'@" "$PGDATA/postgresql.conf" \
        && echo '3) Populate DB data: Restore DB backup:' \
            && find db -type f -name *.sql -print0 | xargs -I {} psql -U postgres -nxq -f {} \
            && rm -rf db \
        && echo '4) Vacuum full and analyze (no reindex need then):' \
            && time vacuumdb -U postgres --full --all --analyze --freeze \
        && echo '5) Stop postgres:' \
            && su-exec postgres pg_ctl -D "$PGDATA" -m fast -w stop \
            && sleep 10 \
        && echo '6) Cleanup pg_xlog required to do not include it in image:' \
            && su-exec postgres pg_resetwal -o \
                $( LANG=C pg_controldata $PGDATA | grep -oP '(?<=NextOID:\s{10})\d+' ) -x \
                $( LANG=C pg_controldata $PGDATA | grep -oP '(?<=NextXID:\s{10}0[/:])\d+' ) -f $PGDATA
				
FROM ubuntu:20.04

RUN apt update && \
	apt install --yes openjdk-17-jdk && \
	apt install tzdata
	
	
WORKDIR /
RUN mkdir /app
WORKDIR app



