-- TRIGGER ON INSERT index_data
CREATE OR REPLACE FUNCTION iris_project_info.trigger_tmp_table()
	RETURNS TRIGGER 
	LANGUAGE plpgsql
	AS $$
	DECLARE
		unix_time_prev DOUBLE PRECISION;
	BEGIN
		DROP TABLE IF EXISTS tmp_table;
		CREATE TEMPORARY TABLE tmp_table(
			id SERIAL PRIMARY KEY,
			buchst CHARACTER VARYING(2),
			lon DOUBLE PRECISION,
			lat DOUBLE PRECISION,
			unix_time DOUBLE PRECISION
		);
		RETURN NULL;
	END;
	$$

CREATE TRIGGER trigger_name
	BEFORE INSERT
	ON iris_project_info.index_data
	FOR EACH STATEMENT
		EXECUTE PROCEDURE iris_project_info.trigger_tmp_table()



-- TRIGGER ON INSERT pic_data
CREATE OR REPLACE FUNCTION iris_project_info.trigger_function()
	RETURNS TRIGGER 
	LANGUAGE plpgsql
	AS $$
	DECLARE
		unix_time_prev DOUBLE PRECISION;
	BEGIN
		-- getting position
		NEW.position := st_setsrid(
			st_geomfromtext('POINT(' || NEW.lon || ' ' || NEW.lat || ')'),
			4326
		);
		
		-- getting time_previous
		SELECT unix_time FROM tmp_table ORDER BY id DESC LIMIT 1 INTO unix_time_prev;
		NEW.time_previous = NEW.unix_time - unix_time_prev;
		
		-- inserting some info to tmp table
		INSERT INTO tmp_table (buchst, lon, lat, unix_time) VALUES (NEW.buchst, NEW.lon, NEW.lat, NEW.unix_time);
		
		RETURN NEW;
	END;
	$$

CREATE TRIGGER trigger_name
	BEFORE INSERT
	ON iris_project_info.pic_data
	FOR EACH ROW
		EXECUTE PROCEDURE iris_project_info.trigger_function()
/*	
TRIGGER ON INSERT pic
	wyciagnac info z ostatniego rekordu z tmp tabeli

TRIGGER ON UPDATE index
		
*/



-- jakies tymczasowe do testów
truncate iris_project_info.index_data cascade;
truncate iris_project_info.pic_data;
SELECT * FROM iris_project_info.index_data;
SELECT * FROM iris_project_info.pic_data;

SELECT * FROM iris_project_info.pic_data
where id_drogi = 'P 2147P' and vnk = '3027008' and nnk = '3127006'
order by buchst, station, time_previous;


INSERT INTO iris_project_info.pic_data(unix_time, lon, lat, pic_data_id, index_data_id) VALUES (3, 17.271107, 52.53429, gen_random_uuid(), 'fd8d046f-3a80-4c5f-a59a-51d9e86dc9ce');

insert into iris_project_info.index_data(index_data_id) values (gen_random_uuid());

DROP TABLE IF EXISTS tmp_table;
CREATE TEMPORARY TABLE tmp_table(
	id SERIAL PRIMARY KEY,
	lon DOUBLE PRECISION,
	lat DOUBLE PRECISION,
	unix_time DOUBLE PRECISION
);

SELECT * FROM tmp_table ORDER BY id DESC


