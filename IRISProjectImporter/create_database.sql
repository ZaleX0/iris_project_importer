
-- HINT :) do generowania unikalnego ID typu CHAR(36) można użyć funkcji gen_random_uuid() np.
-- INSERT INTO test values(gen_random_uuid(), 'imie', 16, 'nazwisko') ... itd. 

-- to trzeba wykonać jako pierwsz, a następnie przełączyć się na utworzoną bazę danych
CREATE DATABASE iris_project_importer
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'Polish_Poland.1250'
    LC_CTYPE = 'Polish_Poland.1250'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;

-- wszystko pozostałe wykonujemy po przełączeniu się na bazę danych
CREATE SCHEMA iris_project_info;

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
	elapsed_time TIMESTAMP WITHOUT TIME ZONE,
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

