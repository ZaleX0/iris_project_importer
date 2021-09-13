-- TRIGGER ON UPDATE index_data
CREATE OR REPLACE FUNCTION iris_project_info.trigger_update_data()
	RETURNS TRIGGER 
	LANGUAGE plpgsql
	AS $$
	DECLARE
		unix_begin DOUBLE PRECISION;
		unix_end DOUBLE PRECISION;
	BEGIN
		-- UPDATE pic_data
		
		
		
		RETURN NEW;
	END;
	$$;

CREATE TRIGGER trigger_update
	BEFORE UPDATE
	ON iris_project_info.index_data
	FOR EACH ROW
	EXECUTE PROCEDURE iris_project_info.trigger_update_data();