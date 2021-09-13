using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace IRISProjectImporter
{
    class SQLManager
    {
        public enum InsertPICResult
        {
            Insert,
            Skip
        }

        readonly Form _ownerWindow;

        public SQLManager() { }
        public SQLManager(Form owner)
        {
            _ownerWindow = owner;
        }


        public string GetConnectionString(string host, string port, string login, string password, string dbName)
        {
            return string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};Timeout={5};",
                host,
                port,
                login,
                password,
                dbName,
                5);
        }
        public string[] GetDatabaseNames(string connectionString)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                List<string> dbList = new List<string>();
                var command = new NpgsqlCommand("SELECT datname FROM pg_database", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dbList.Add(reader.GetString(0));
                    }
                }
                return dbList.ToArray();
            }
        }
        public bool CheckIfSchemaExists(string connectionString)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var reader = new NpgsqlCommand("SELECT schema_name FROM information_schema.schemata WHERE schema_name = 'iris_project_info';", connection).ExecuteReader())
                {
                    string schema = string.Empty;
                    if (reader.Read())
                        schema = reader.GetString(0);

                    if (schema != string.Empty)
                        return true;
                    else
                        return false;
                }
            }
        }
        public void CreateSchema(string connectionString)
        {
            string create_schema_sql = File.ReadAllText(@"sql\create_schema.sql");

            using (var connection = new NpgsqlConnection(connectionString))
            using (var command = new NpgsqlCommand(create_schema_sql, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }


        public async Task<InsertPICResult> InsertPIC(string picPath, NpgsqlConnection connection)
        {
            PICFileInfo[] pics = null;
            IndexFileInfo index = null;

            bool reading = true;
            while (reading)
            {
                #region Read Xml (get PICFileInfo[] and IndexFileInfo)
                try
                {
                    XmlFileReader xmlReader = new XmlFileReader();
                    pics = xmlReader.PicFileInfoArray_buchst_XorP(picPath);
                    index = xmlReader.GetIndexByPicPath(picPath);

                    reading = false;
                }
                catch (Exception)
                {
                    DialogResult dialogResult = (DialogResult) _ownerWindow.Invoke(new Func<DialogResult>(() => {
                        return MessageBox.Show(
                            _ownerWindow,
                            "Wystąpił błąd podczas wczytywania pliku xml.",
                            "Błąd",
                            MessageBoxButtons.AbortRetryIgnore,
                            MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button2
                        );
                    }));
                    switch (dialogResult)
                    {
                        case DialogResult.Abort: // Stop reading xml 
                            throw new Exception("Operation aborted");

                        case DialogResult.Retry: // Retry reading xml
                            break;

                        case DialogResult.Ignore: // Ignore this pic => exit this method
                            return InsertPICResult.Skip;

                        default:
                            throw;
                    }
                }
                #endregion
            }

            bool inserting = true;
            while (inserting)
            {
                try
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        int picDataCount = CountPicDataByPicPath(index.picpath, connection);
                        if (picDataCount >= pics.Length) // skip this pic xml
                            return InsertPICResult.Skip;

                        else if (picDataCount != 0 && picDataCount % pics.Length != 0) // this is never executed unless someone adds something manually to db
                            DeletePicDataByPicPath(index.picpath, connection);

                        // INSERT index_data UUID
                        string indexUUID = InsertNewIndexUUID(connection);

                        // INSERT pic_data
                        if (IsDirectionOpposite(index))
                            for (int i = pics.Length - 1; i >= 0; i--) // opposite direction -> read xml from the bottom
                                InsertPicData(pics[i], indexUUID, connection);

                        else
                            for (int i = 0; i < pics.Length; i++)
                                InsertPicData(pics[i], indexUUID, connection);

                        // UPDATE index_data
                        UpdateIndexData(index, indexUUID, connection);

                        // COMMIT
                        await transaction.CommitAsync();
                    }
                    inserting = false;
                }
                catch (Exception)
                {
                    DialogResult dialogResult = (DialogResult) _ownerWindow.Invoke(new Func<DialogResult>(() => {
                        return MessageBox.Show(
                            _ownerWindow,
                            "Wystąpił błąd połączenia z bazą danych.",
                            "Błąd",
                            MessageBoxButtons.AbortRetryIgnore,
                            MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button2
                        );
                    }));
                    switch (dialogResult)
                    {
                        case DialogResult.Abort: // Stop inserting pics
                            throw new Exception("Operation aborted");

                        case DialogResult.Retry: // Retry inserting pics
                            inserting = true;
                            break;

                        case DialogResult.Ignore: // Ignore this pic => exit this method
                            return InsertPICResult.Skip;

                        default:
                            throw;
                    }
                }
            }
            return InsertPICResult.Insert;
        }


        private int CountPicDataByPicPath(string picpathFromXml, NpgsqlConnection connection)
        {
            string query = $"SELECT count(*) FROM iris_project_info.pic_data WHERE picpath = '{picpathFromXml}';";

            using (var command = new NpgsqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                int count = -1;

                if (reader.Read())
                    count = reader.GetInt32(0);

                return count;
            }
        }
        private void DeletePicDataByPicPath(string picpathFromXml, NpgsqlConnection connection)
        {
            string query = $"DELETE FROM iris_project_info.pic_data WHERE picpath = '{picpathFromXml}';";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        private string InsertNewIndexUUID(NpgsqlConnection connection)
        {
            string query = "INSERT INTO iris_project_info.index_data (index_data_id) VALUES (gen_random_uuid()) " +
                "RETURNING index_data_id;";

            using (var command = new NpgsqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                string uuid = "";

                if (reader.Read())
                    uuid = reader.GetString(0);

                return uuid;
            }
        }
        private void InsertPicData(PICFileInfo pic, string indexUUID, NpgsqlConnection connection)
        {
            string query = "INSERT INTO iris_project_info.pic_data " +
                "(pic_data_id, index_data_id, id_drogi, vnk, nnk, abs, version, buchst, station, " +
                "seiher_km, filename, format, datum, lat, latns, lon, lonew, alt, heading, picpath, " +
                "acc_lat, acc_lon, acc_alt, acc_heading, acc_roll, acc_pitch, roll, pitch, unix_time, pic_id) " +
                "VALUES (gen_random_uuid(), @v01, @v02, @v03, @v04, @v05, @v06, @v07, @v08, @v09, @v10, @v11, @v12, " +
                "@v13, @v14, @v15, @v16, @v17, @v18, @v19, @v20, @v21, @v22, @v23, @v24, @v25, @v26, @v27, @v28, @v29);";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("v01", indexUUID);
                #region picCommand.Parameters
                command.Parameters.AddWithValue("v02", pic.id_drogi);
                command.Parameters.AddWithValue("v03", pic.vnk);
                command.Parameters.AddWithValue("v04", pic.nnk);
                command.Parameters.AddWithValue("v05", pic.abs);
                command.Parameters.AddWithValue("v06", pic.version);
                command.Parameters.AddWithValue("v07", pic.buchst);
                command.Parameters.AddWithValue("v08", pic.station);
                command.Parameters.AddWithValue("v09", pic.seiher_km);
                command.Parameters.AddWithValue("v10", pic.filename);
                command.Parameters.AddWithValue("v11", pic.format);
                command.Parameters.AddWithValue("v12", DateTime.Parse(pic.datum));
                command.Parameters.AddWithValue("v13", pic.lat);
                command.Parameters.AddWithValue("v14", pic.latns);
                command.Parameters.AddWithValue("v15", pic.lon);
                command.Parameters.AddWithValue("v16", pic.lonew);
                command.Parameters.AddWithValue("v17", pic.alt);
                command.Parameters.AddWithValue("v18", pic.heading);
                command.Parameters.AddWithValue("v19", pic.picpath);
                command.Parameters.AddWithValue("v20", pic.acc_lat);
                command.Parameters.AddWithValue("v21", pic.acc_lon);
                command.Parameters.AddWithValue("v22", pic.acc_alt);
                command.Parameters.AddWithValue("v23", pic.acc_heading);
                command.Parameters.AddWithValue("v24", pic.acc_roll);
                command.Parameters.AddWithValue("v25", pic.acc_pitch);
                command.Parameters.AddWithValue("v26", pic.roll);
                command.Parameters.AddWithValue("v27", pic.pitch);
                command.Parameters.AddWithValue("v28", pic.unix_time);
                command.Parameters.AddWithValue("v29", pic.pic_id);
                #endregion
                command.ExecuteNonQuery();
            }
        }
        private void UpdateIndexData(IndexFileInfo index, string indexUUID, NpgsqlConnection connection)
        {
            #region SQL - UPDATE index
            string sql_update_index = "UPDATE iris_project_info.index_data " +
                "SET vnk = @p01," +
                "nnk = @p02," +
                "von_stat = @p03," +
                "bis_stat = @p04," +
                "richtung = @p05," +
                "cam = @p06," +
                "datum = @p07," +
                "version = @p08," +
                "bemerkung = @p09," +
                "volume = @p10," +
                "picpath = @p11," +
                "v_seither_km = @p12," +
                "n_seither_km = @p13," +
                "abs = @p14," +
                "str_bez = @p15," +
                "laenge = @p16," +
                "kierunek = @p17," +
                "nrodc = @p18," +
                "km_lokp = @p19," +
                "km_lokk = @p20," +
                "km_globp = @p21," +
                "km_globk = @p22," +
                "phoml = @p23 " +
                $"WHERE index_data_id = '{indexUUID}'; ";
            #endregion
            using (var indexCommand = new NpgsqlCommand(sql_update_index, connection))
            {
                #region indexCommand.Parameters
                indexCommand.Parameters.AddWithValue("p01", index.vnk);
                indexCommand.Parameters.AddWithValue("p02", index.nnk);
                indexCommand.Parameters.AddWithValue("p03", index.von_stat);
                indexCommand.Parameters.AddWithValue("p04", index.bis_stat);
                indexCommand.Parameters.AddWithValue("p05", index.richtung);
                indexCommand.Parameters.AddWithValue("p06", index.cam);
                indexCommand.Parameters.AddWithValue("p07", DateTime.Parse(index.datum));
                indexCommand.Parameters.AddWithValue("p08", index.version);
                indexCommand.Parameters.AddWithValue("p09", index.bemerkung);
                indexCommand.Parameters.AddWithValue("p10", index.volume);
                indexCommand.Parameters.AddWithValue("p11", index.picpath);
                indexCommand.Parameters.AddWithValue("p12", index.v_seither_km);
                indexCommand.Parameters.AddWithValue("p13", index.n_seither_km);
                indexCommand.Parameters.AddWithValue("p14", index.abs);
                indexCommand.Parameters.AddWithValue("p15", index.str_bez);
                indexCommand.Parameters.AddWithValue("p16", index.laenge);
                indexCommand.Parameters.AddWithValue("p17", index.kierunek);
                indexCommand.Parameters.AddWithValue("p18", index.nrodc);
                indexCommand.Parameters.AddWithValue("p19", index.km_lokp);
                indexCommand.Parameters.AddWithValue("p20", index.km_lokk);
                indexCommand.Parameters.AddWithValue("p21", index.km_globp);
                indexCommand.Parameters.AddWithValue("p22", index.km_globk);
                indexCommand.Parameters.AddWithValue("p23", index.phoml);
                #endregion
                indexCommand.ExecuteNonQuery();
            }
        }
        private bool IsDirectionOpposite(IndexFileInfo index)
        {
            string direction = new DirectoryInfo(index.picpath).Parent.Name.Split('_')[2];
            return direction.Equals("M");
        }



        public InsertPICResult InsertPIC_OLD(string picPath, string connectionString)
        {
            PICFileInfo[] pics = null;
            IndexFileInfo index = null;

            bool reading = true;
            while (reading)
            {
                #region Read Xml (get PICFileInfo[] and IndexFileInfo)
                try
                {
                    XmlFileReader xmlReader = new XmlFileReader();
                    pics = xmlReader.PicFileInfoArray_buchst_XorP(picPath);
                    index = xmlReader.GetIndexByPicPath(picPath);

                    reading = false;
                }
                catch (Exception)
                {
                    DialogResult dialogResult = (DialogResult) _ownerWindow.Invoke(new Func<DialogResult>(() => {
                        return MessageBox.Show(
                            _ownerWindow,
                            "Wystąpił błąd podczas wczytywania pliku xml.",
                            "Błąd",
                            MessageBoxButtons.AbortRetryIgnore,
                            MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button2
                        );
                    }));
                    switch (dialogResult)
                    {
                        case DialogResult.Abort: // Stop reading xml 
                            throw new Exception("Operation aborted");

                        case DialogResult.Retry: // Retry reading xml
                            break;

                        case DialogResult.Ignore: // Ignore this pic => exit this method
                            return InsertPICResult.Skip;

                        default:
                            throw;
                    }
                }
                #endregion
            }

            bool inserting = true;
            while (inserting)
            {
                try
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        using (var transaction = connection.BeginTransaction())
                        {
                            int picDataCount = CountPicDataByPicPath(index.picpath, connection);
                            if (picDataCount >= pics.Length) // skip this pic xml
                                return InsertPICResult.Skip;

                            else if (picDataCount != 0 && picDataCount % pics.Length != 0) // this is never executed unless someone adds something manually to db
                                DeletePicDataByPicPath(index.picpath, connection);

                            // INSERT index_data UUID
                            string indexUUID = InsertNewIndexUUID(connection);

                            // INSERT pic_data
                            if (IsDirectionOpposite(index))
                                for (int i = pics.Length - 1; i >= 0; i--) // opposite direction -> read xml from the bottom
                                    InsertPicData(pics[i], indexUUID, connection);

                            else
                                for (int i = 0; i < pics.Length; i++)
                                    InsertPicData(pics[i], indexUUID, connection);

                            // UPDATE index_data
                            UpdateIndexData(index, indexUUID, connection);

                            // COMMIT
                            transaction.Commit();
                        }
                    }
                    inserting = false;
                }
                catch (Exception)
                {
                    DialogResult dialogResult = (DialogResult) _ownerWindow.Invoke(new Func<DialogResult>(() => {
                        return MessageBox.Show(
                            _ownerWindow,
                            "Wystąpił błąd połączenia z bazą danych.",
                            "Błąd",
                            MessageBoxButtons.AbortRetryIgnore,
                            MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button2
                        );
                    }));
                    switch (dialogResult)
                    {
                        case DialogResult.Abort: // Stop inserting pics
                            throw new Exception("Operation aborted");

                        case DialogResult.Retry: // Retry inserting pics
                            inserting = true;
                            break;

                        case DialogResult.Ignore: // Ignore this pic => exit this method
                            return InsertPICResult.Skip;

                        default:
                            throw;
                    }
                }
            }
            return InsertPICResult.Insert;
        }
    }
}
