-- Перед выполнением переключись на базу ulearn

CREATE EXTENSION IF NOT EXISTS "uuid-ossp"; --генерация гуидов
CREATE EXTENSION IF NOT EXISTS "pg_trgm"; --gin индекс с триграммами для поиска по регвыру

CREATE OR REPLACE FUNCTION immutable_concat_ws(text, VARIADIC text[])
RETURNS text AS 'text_concat_ws' LANGUAGE internal IMMUTABLE PARALLEL SAFE;

GRANT CREATE ON DATABASE ulearn TO ulearn; -- иначе при миграции выполняет CREATE SCHEMA IF NOT EXISTS antiplagiarism и не хватает прав 

CREATE SCHEMA IF NOT EXISTS antiplagiarism;
ALTER SCHEMA antiplagiarism OWNER TO ulearn;