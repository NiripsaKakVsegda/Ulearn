ROLLBACK;
BEGIN;
CREATE TEMP TABLE my_fist_temp_table -- стоит использовать наиболее уникальное имя
ON COMMIT DROP -- удаляем таблицу при завершении транзакции
AS
SELECT 1 AS id, CAST ('какие-то значения' AS TEXT) AS val;

------------ Дополнительные манипуляции с таблицей: ------------------

-- изменим таблицу, добавив столбец. Буду частенько затрагивать смежные темы
ALTER TABLE my_fist_temp_table
    ADD COLUMN is_deleted BOOLEAN NOT NULL DEFAULT FALSE;
-- для тех, кто не в курсе, чаще всего данные в таблицах не удаляются, а помечаются как удаленные подобным флагом

CREATE UNIQUE INDEX ON my_fist_temp_table (lower(val))
WHERE is_deleted = FALSE; -- можно даже создать индекс/ограничение, если это необходимо
-- данный индекс не позволит вставить дубликат(не зависимо от регистра) для столбца VAL, для не удаленных строк

-- манипулируем данными таблицы
UPDATE my_fist_temp_table
SET id=id+3;

-- проверяем/используем содержание таблицы
SELECT * FROM my_fist_temp_table;
--COMMIT;