CREATE SCHEMA iris_project_info;

CREATE EXTENSION IF NOT EXISTS postgis;

CREATE TABLE iris_project_info.index_data(
	index_data_id CHARACTER VARYING(36) UNIQUE NOT NULL,
	vnk CHARACTER VARYING(10),
	nnk CHARACTER VARYING(10), 
	von_stat INTEGER,
	bis_stat INTEGER,
	richtung CHARACTER VARYING(3),
	cam TEXT,
	datum TIMESTAMP WITHOUT TIME ZONE,
	version CHARACTER VARYING(2),
	bemerkung TEXT,
	volume CHARACTER VARYING(20),
	picpath TEXT,
	v_seither_km INTEGER,
	n_seither_km INTEGER,
	abs INTEGER,
	str_bez CHARACTER VARYING(20),
	laenge INTEGER,
	kierunek CHARACTER VARYING(10),
	nrodc CHARACTER VARYING(8),
	km_lokp INTEGER,
	km_lokk INTEGER,
	km_globp INTEGER,
	km_globk INTEGER,
	phoml CHARACTER VARYING(10),
	route GEOMETRY,
	begin_time TIMESTAMP WITHOUT TIME ZONE,
	end_time TIMESTAMP WITHOUT TIME ZONE,
	elapsed_time INTERVAL,
	avg_speed DOUBLE PRECISION,
	pic_count INTEGER,
	PRIMARY KEY(index_data_id)
);

CREATE TABLE iris_project_info.pic_data(
	pic_data_id CHARACTER VARYING(36) UNIQUE NOT NULL,
	index_data_id CHARACTER VARYING(36) NOT NULL,
	id_drogi CHARACTER VARYING(12),
	vnk CHARACTER VARYING(10),
	nnk CHARACTER VARYING(10), 
	abs INTEGER,
	version CHARACTER VARYING(2),
	buchst CHARACTER VARYING(2),
	station INTEGER,
	seiher_km INTEGER,
	filename TEXT,
	format INTEGER,
	datum TIMESTAMP WITHOUT TIME ZONE,
	lat DOUBLE PRECISION,
	latns CHARACTER VARYING(2),
	lon DOUBLE PRECISION,
	lonew CHARACTER VARYING(2),
	alt DOUBLE PRECISION,
	heading DOUBLE PRECISION,
	picpath TEXT,
	acc_lat DOUBLE PRECISION,
	acc_lon DOUBLE PRECISION,
	acc_alt DOUBLE PRECISION,
	acc_heading DOUBLE PRECISION,
	acc_roll DOUBLE PRECISION,
	acc_pitch DOUBLE PRECISION,
	roll DOUBLE PRECISION,
	pitch DOUBLE PRECISION,
	unix_time DOUBLE PRECISION,
	pic_id DOUBLE PRECISION,
	speed_previous DOUBLE PRECISION,
	time_previous DOUBLE PRECISION,
	distance_previous DOUBLE PRECISION,
	position GEOMETRY,
	PRIMARY KEY(pic_data_id),
	FOREIGN KEY(index_data_id) REFERENCES iris_project_info.index_data(index_data_id)
		ON UPDATE CASCADE ON DELETE SET NULL
);

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
;
CREATE TRIGGER trigger_insert_index
	BEFORE INSERT
	ON iris_project_info.index_data
	FOR EACH STATEMENT
		EXECUTE PROCEDURE iris_project_info.trigger_tmp_table();
		
-- TRIGGER ON INSERT pic_data
CREATE OR REPLACE FUNCTION iris_project_info.trigger_insert_pic_data()
	RETURNS TRIGGER 
	LANGUAGE plpgsql
	AS $$
	DECLARE
		unix_time_prev DOUBLE PRECISION;
		buchst_prev CHARACTER VARYING(2);
		lon_prev DOUBLE PRECISION;
		lat_prev DOUBLE PRECISION;
	BEGIN
		-- getting position
		NEW.position := st_setsrid(
			st_geomfromtext('POINT(' || NEW.lon || ' ' || NEW.lat || ')'),
			4326
		);
		
		-- getting time_previous
		SELECT unix_time FROM tmp_table ORDER BY id DESC LIMIT 1 INTO unix_time_prev;
		NEW.time_previous := coalesce(NEW.unix_time - unix_time_prev, 0);
		
		SELECT buchst FROM tmp_table ORDER BY id DESC LIMIT 1 INTO buchst_prev;
		
		-- getting distance_previous
		SELECT lon, lat FROM tmp_table ORDER BY id DESC LIMIT 1 INTO lon_prev, lat_prev;
		NEW.distance_previous := coalesce(
			st_distance(
				st_transform(
					st_setsrid(st_geomfromtext('POINT(' || NEW.lon || ' ' || NEW.lat || ')'), 4326),
					2180
				),
				st_transform(
					st_setsrid(st_geomfromtext('POINT(' || lon_prev || ' ' || lat_prev || ')'), 4326),
					2180
				)
			),
			0
		);
		
		-- calculating speed_previous
		IF NEW.time_previous > 0 THEN
			NEW.speed_previous := (NEW.distance_previous / NEW.time_previous) * 3.6; -- przelicznik na km/h
		ELSE
			NEW.speed_previous := 0;
		END IF;
		
		-- on different camera
		IF NEW.buchst != buchst_prev THEN
			NEW.time_previous := 0;
			NEW.distance_previous := 0;
		END IF;
		
		-- inserting some info to tmp table
		INSERT INTO tmp_table (buchst, lon, lat, unix_time) VALUES (NEW.buchst, NEW.lon, NEW.lat, NEW.unix_time);
		
		RETURN NEW;
	END;
	$$
;
CREATE TRIGGER trigger_insert_pic
	BEFORE INSERT
	ON iris_project_info.pic_data
	FOR EACH ROW
		EXECUTE PROCEDURE iris_project_info.trigger_insert_pic_data();
		
-- TRIGGER ON UPDATE index_data
CREATE OR REPLACE FUNCTION iris_project_info.trigger_update_index_data()
	RETURNS TRIGGER 
	LANGUAGE plpgsql
	AS $$
	DECLARE
		unix_begin DOUBLE PRECISION;
		unix_end DOUBLE PRECISION;
	BEGIN
		-- NEW.route
		SELECT st_makeline(position ORDER BY unix_time) FROM iris_project_info.pic_data
			WHERE index_data_id = NEW.index_data_id
			--GROUP BY index_data_id, buchst, station
			INTO NEW.route;
		
		-- NEW.begin_time
		SELECT unix_time FROM iris_project_info.pic_data
			WHERE index_data_id = NEW.index_data_id
			ORDER BY unix_time ASC LIMIT 1
			INTO unix_begin;
		
		SELECT TO_TIMESTAMP(unix_begin)
			INTO NEW.begin_time;
	
		-- NEW.end_time
		SELECT unix_time FROM iris_project_info.pic_data
			WHERE index_data_id = NEW.index_data_id
			ORDER BY unix_time DESC LIMIT 1
			INTO unix_end;
			
		SELECT TO_TIMESTAMP(unix_end)
			INTO NEW.end_time;
		
		-- NEW.elapsed_time
		SELECT NEW.end_time - NEW.begin_time
			INTO NEW.elapsed_time;
		
		-- NEW.avg_speed
		SELECT round(avg(speed_previous)::numeric, 1) FROM iris_project_info.pic_data
		WHERE index_data_id = NEW.index_data_id
		INTO NEW.avg_speed;
		
		-- NEW.pic_count
		SELECT count(*) FROM iris_project_info.pic_data
		WHERE index_data_id = NEW.index_data_id
		INTO NEW.pic_count;
		
		RETURN NEW;
	END;
	$$
;
CREATE TRIGGER trigger_update_index
	BEFORE UPDATE
	ON iris_project_info.index_data
	FOR EACH ROW
		EXECUTE PROCEDURE iris_project_info.trigger_update_index_data();