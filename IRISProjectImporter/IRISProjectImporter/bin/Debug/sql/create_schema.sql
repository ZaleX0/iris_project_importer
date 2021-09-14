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

-- TRIGGER ON UPDATE index_data
CREATE OR REPLACE FUNCTION iris_project_info.trigger_update_data()
	RETURNS TRIGGER 
	LANGUAGE plpgsql
	AS $$
	BEGIN
		-- UPDATE pic_data
		UPDATE iris_project_info.pic_data SET
			--
			position = st_setsrid(st_geomfromtext('POINT(' || lon || ' ' || lat || ')'), 4326),
			--
			time_previous = coalesce(unix_time - tmp.unix_time_prev, 0),
			--
			distance_previous = coalesce(st_distance(
				st_transform(st_setsrid(st_geomfromtext('POINT(' || tmp.lon_prev || ' ' || tmp.lat_prev || ')'), 4326), 2180),
				st_transform(st_setsrid(st_geomfromtext('POINT(' || lon || ' ' || lat || ')'), 4326), 2180)
				), 0),
			--
			speed_previous = coalesce(
				(coalesce(
					st_distance(
						st_transform(st_setsrid(st_geomfromtext('POINT(' || tmp.lon_prev || ' ' || tmp.lat_prev || ')'), 4326), 2180),
						st_transform(st_setsrid(st_geomfromtext('POINT(' || lon || ' ' || lat || ')'), 4326), 2180)
					), 0) / nullif(coalesce(unix_time - tmp.unix_time_prev, 0), 0) * 3.6
				), 0)
			--
			FROM (select pic_data_id as pic,
				  lag(unix_time) over (order by unix_time) as unix_time_prev,
				  lag(lon) over (order by unix_time) as lon_prev,
				  lag(lat) over (order by unix_time) as lat_prev
				  from iris_project_info.pic_data) tmp
			WHERE index_data_id = NEW.index_data_id
			AND pic_data_id = tmp.pic;
		-- UPDATE index_data
		NEW.route = (
			select st_makeline(position order by unix_time)
			from iris_project_info.pic_data
			where index_data_id = NEW.index_data_id);
		--
		NEW.begin_time = (
			select to_timestamp(unix_time)
			from iris_project_info.pic_data
			where index_data_id = NEW.index_data_id
			order by unix_time asc limit 1);
		--
		NEW.end_time = (
			select to_timestamp(unix_time)
			from iris_project_info.pic_data
			where index_data_id = NEW.index_data_id
			order by unix_time desc limit 1);
		--
		NEW.elapsed_time = NEW.end_time - NEW.begin_time;
		--
		NEW.avg_speed = (
			select round(avg(speed_previous)::numeric, 1)
			from iris_project_info.pic_data
			where index_data_id = NEW.index_data_id);
		--
		NEW.pic_count = (
			select count(*)
			from iris_project_info.pic_data
			where index_data_id = NEW.index_data_id);
		--
		RETURN NEW;
	END;
	$$;

CREATE TRIGGER trigger_update_index
	BEFORE UPDATE
	ON iris_project_info.index_data
	FOR EACH ROW
		EXECUTE PROCEDURE iris_project_info.trigger_update_data();