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
        readonly bool _log;
        readonly Logger _logger;
        readonly ProgressBarManager _progressBarManager;
        public SQLManager()
        {
            _log = false;
        }
        public SQLManager(Logger logger, ProgressBarManager pbm)
        {
            _log = true;
            _logger = logger;
            _progressBarManager = pbm;
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


        public void InsertIndexWithPics(string indexPath, string connectionString)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    #region Logger Code
                    if (_log) _logger.Log("Connection established.");
                    int index_i = 0;
                    int pic_i = 0;
                    #endregion

                    XmlFileReader xmlReader = new XmlFileReader();
                    IndexFileInfo[] indexes = xmlReader.IndexFileInfoArray(indexPath);
                    string[] indexUUIDs = new string[indexes.Length];
                    for (int i = 0; i < indexes.Length; i++)
                    {
                        #region SQL - INSERT INTO iris_project_info.index_data...
                        string sql_index = "INSERT INTO iris_project_info.index_data " +
                            "(index_data_id) " +
                            "VALUES (gen_random_uuid()) " +
                            "RETURNING index_data_id;";
                        #endregion
                        using (var command = new NpgsqlCommand(sql_index, connection))
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                                indexUUIDs[i] = reader.GetString(0);
                        }

                        try
                        {
                            #region SQL - INSERT INTO iris_project_info.pic_data...
                            string sql_pic = "INSERT INTO iris_project_info.pic_data " +
                                "(pic_data_id, index_data_id, id_drogi, vnk, nnk, abs, version, buchst, station, " +
                                "seiher_km, filename, format, datum, lat, latns, lon, lonew, alt, heading, picpath, " +
                                "acc_lat, acc_lon, acc_alt, acc_heading, acc_roll, acc_pitch, roll, pitch, unix_time, pic_id) " +
                                "VALUES (gen_random_uuid(), @v01, @v02, @v03, @v04, @v05, @v06, @v07, @v08, @v09, @v10, @v11, @v12, " +
                                "@v13, @v14, @v15, @v16, @v17, @v18, @v19, @v20, @v21, @v22, @v23, @v24, @v25, @v26, @v27, @v28, @v29);";
                            #endregion

                            string picPath = $"{new FileInfo(indexPath).DirectoryName}\\{indexes[i].picpath}";
                            picPath = new DirectoryInfo(picPath).GetFiles("PIC_*.xml")[0].FullName;
                            PICFileInfo[] pics = xmlReader.PicFileInfoArray(picPath);

                            #region Logger Code
                            if (_log)
                            {
                                _logger.Log($"Inserting ({i + 1}/{indexes.Length}): {indexes[i].picpath}");
                                _progressBarManager.StepProgressBar();
                            }
                            #endregion

                            // jesli kierunek jest przeciwny to czytaj xmla od konca
                            string kierunek = new DirectoryInfo(indexes[i].picpath).Parent.Name.Split('_')[2];
                            if (kierunek.Equals("M"))
                            {
                                for (int j = pics.Length - 1; j >= 0; j--)
                                {
                                    using (var picCommand = new NpgsqlCommand(sql_pic, connection))
                                    {
                                        picCommand.Parameters.AddWithValue("v01", indexUUIDs[i]);
                                        #region picCommand.Parameters
                                        picCommand.Parameters.AddWithValue("v02", pics[j].id_drogi);
                                        picCommand.Parameters.AddWithValue("v03", pics[j].vnk);
                                        picCommand.Parameters.AddWithValue("v04", pics[j].nnk);
                                        picCommand.Parameters.AddWithValue("v05", pics[j].abs);
                                        picCommand.Parameters.AddWithValue("v06", pics[j].version);
                                        picCommand.Parameters.AddWithValue("v07", pics[j].buchst);
                                        picCommand.Parameters.AddWithValue("v08", pics[j].station);
                                        picCommand.Parameters.AddWithValue("v09", pics[j].seiher_km);
                                        picCommand.Parameters.AddWithValue("v10", pics[j].filename);
                                        picCommand.Parameters.AddWithValue("v11", pics[j].format);
                                        picCommand.Parameters.AddWithValue("v12", DateTime.Parse(pics[j].datum));
                                        picCommand.Parameters.AddWithValue("v13", pics[j].lat);
                                        picCommand.Parameters.AddWithValue("v14", pics[j].latns);
                                        picCommand.Parameters.AddWithValue("v15", pics[j].lon);
                                        picCommand.Parameters.AddWithValue("v16", pics[j].lonew);
                                        picCommand.Parameters.AddWithValue("v17", pics[j].alt);
                                        picCommand.Parameters.AddWithValue("v18", pics[j].heading);
                                        picCommand.Parameters.AddWithValue("v19", pics[j].picpath);
                                        picCommand.Parameters.AddWithValue("v20", pics[j].acc_lat);
                                        picCommand.Parameters.AddWithValue("v21", pics[j].acc_lon);
                                        picCommand.Parameters.AddWithValue("v22", pics[j].acc_alt);
                                        picCommand.Parameters.AddWithValue("v23", pics[j].acc_heading);
                                        picCommand.Parameters.AddWithValue("v24", pics[j].acc_roll);
                                        picCommand.Parameters.AddWithValue("v25", pics[j].acc_pitch);
                                        picCommand.Parameters.AddWithValue("v26", pics[j].roll);
                                        picCommand.Parameters.AddWithValue("v27", pics[j].pitch);
                                        picCommand.Parameters.AddWithValue("v28", pics[j].unix_time);
                                        picCommand.Parameters.AddWithValue("v29", pics[j].pic_id);
                                        #endregion
                                        picCommand.ExecuteNonQuery();
                                    }
                                    pic_i++;
                                }
                            }
                            else
                            {
                                for (int j = 0; j < pics.Length; j++)
                                {
                                    using (var picCommand = new NpgsqlCommand(sql_pic, connection))
                                    {
                                        picCommand.Parameters.AddWithValue("v01", indexUUIDs[i]);
                                        #region picCommand.Parameters
                                        picCommand.Parameters.AddWithValue("v02", pics[j].id_drogi);
                                        picCommand.Parameters.AddWithValue("v03", pics[j].vnk);
                                        picCommand.Parameters.AddWithValue("v04", pics[j].nnk);
                                        picCommand.Parameters.AddWithValue("v05", pics[j].abs);
                                        picCommand.Parameters.AddWithValue("v06", pics[j].version);
                                        picCommand.Parameters.AddWithValue("v07", pics[j].buchst);
                                        picCommand.Parameters.AddWithValue("v08", pics[j].station);
                                        picCommand.Parameters.AddWithValue("v09", pics[j].seiher_km);
                                        picCommand.Parameters.AddWithValue("v10", pics[j].filename);
                                        picCommand.Parameters.AddWithValue("v11", pics[j].format);
                                        picCommand.Parameters.AddWithValue("v12", DateTime.Parse(pics[j].datum));
                                        picCommand.Parameters.AddWithValue("v13", pics[j].lat);
                                        picCommand.Parameters.AddWithValue("v14", pics[j].latns);
                                        picCommand.Parameters.AddWithValue("v15", pics[j].lon);
                                        picCommand.Parameters.AddWithValue("v16", pics[j].lonew);
                                        picCommand.Parameters.AddWithValue("v17", pics[j].alt);
                                        picCommand.Parameters.AddWithValue("v18", pics[j].heading);
                                        picCommand.Parameters.AddWithValue("v19", pics[j].picpath);
                                        picCommand.Parameters.AddWithValue("v20", pics[j].acc_lat);
                                        picCommand.Parameters.AddWithValue("v21", pics[j].acc_lon);
                                        picCommand.Parameters.AddWithValue("v22", pics[j].acc_alt);
                                        picCommand.Parameters.AddWithValue("v23", pics[j].acc_heading);
                                        picCommand.Parameters.AddWithValue("v24", pics[j].acc_roll);
                                        picCommand.Parameters.AddWithValue("v25", pics[j].acc_pitch);
                                        picCommand.Parameters.AddWithValue("v26", pics[j].roll);
                                        picCommand.Parameters.AddWithValue("v27", pics[j].pitch);
                                        picCommand.Parameters.AddWithValue("v28", pics[j].unix_time);
                                        picCommand.Parameters.AddWithValue("v29", pics[j].pic_id);
                                        #endregion
                                        picCommand.ExecuteNonQuery();
                                    }
                                    pic_i++;
                                }
                            }
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new Exception($"WARNING: {indexes[i].picpath} file missing.");
                        }

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
                            $"WHERE index_data_id = '{indexUUIDs[i]}'; ";
                        #endregion
                        using (var indexCommand = new NpgsqlCommand(sql_update_index, connection))
                        {
                            #region indexCommand.Parameters
                            indexCommand.Parameters.AddWithValue("p01", indexes[i].vnk);
                            indexCommand.Parameters.AddWithValue("p02", indexes[i].nnk);
                            indexCommand.Parameters.AddWithValue("p03", indexes[i].von_stat);
                            indexCommand.Parameters.AddWithValue("p04", indexes[i].bis_stat);
                            indexCommand.Parameters.AddWithValue("p05", indexes[i].richtung);
                            indexCommand.Parameters.AddWithValue("p06", indexes[i].cam);
                            indexCommand.Parameters.AddWithValue("p07", DateTime.Parse(indexes[i].datum));
                            indexCommand.Parameters.AddWithValue("p08", indexes[i].version);
                            indexCommand.Parameters.AddWithValue("p09", indexes[i].bemerkung);
                            indexCommand.Parameters.AddWithValue("p10", indexes[i].volume);
                            indexCommand.Parameters.AddWithValue("p11", indexes[i].picpath);
                            indexCommand.Parameters.AddWithValue("p12", indexes[i].v_seither_km);
                            indexCommand.Parameters.AddWithValue("p13", indexes[i].n_seither_km);
                            indexCommand.Parameters.AddWithValue("p14", indexes[i].abs);
                            indexCommand.Parameters.AddWithValue("p15", indexes[i].str_bez);
                            indexCommand.Parameters.AddWithValue("p16", indexes[i].laenge);
                            indexCommand.Parameters.AddWithValue("p17", indexes[i].kierunek);
                            indexCommand.Parameters.AddWithValue("p18", indexes[i].nrodc);
                            indexCommand.Parameters.AddWithValue("p19", indexes[i].km_lokp);
                            indexCommand.Parameters.AddWithValue("p20", indexes[i].km_lokk);
                            indexCommand.Parameters.AddWithValue("p21", indexes[i].km_globp);
                            indexCommand.Parameters.AddWithValue("p22", indexes[i].km_globk);
                            indexCommand.Parameters.AddWithValue("p23", indexes[i].phoml);
                            #endregion
                            indexCommand.ExecuteNonQuery();
                        }

                        index_i++;
                    }

                    #region Logger Code
                    if (_log) _logger.Log($"Commited {index_i} Index records and {pic_i} PIC records.");
                    #endregion
                    transaction.Commit();
                }
            }
        }

        private void InsertIndexWithPICs_OLD(string indexPath, string[] picPaths, string connectionString)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    XmlFileReader xmlReader = new XmlFileReader();

                    #region Logger Code
                    if (_log)
                    {
                        _logger.Log("Connection with database established.");
                        FileInfo indexFI = new FileInfo(indexPath);
                        _logger.Log($"Inserting: {indexFI.Directory.Name}\\{indexFI.Name}");
                    }
                    int logIndexRecords = 0;
                    int logPICRecords = 0;
                    #endregion

                    #region SQL querys
                    string sql_index = "INSERT INTO iris_project_info.index_data " +
                        "(index_data_id, vnk, nnk, von_stat, bis_stat, richtung, cam, datum, version, bemerkung, volume, picpath, " +
                        "v_seither_km, n_seither_km, abs, str_bez, laenge, kierunek, nrodc, km_lokp, km_lokk, km_globp, km_globk, phoml) " +
                        "VALUES (gen_random_uuid(), @a, @b, @c, @d, @e, @f, @g, @h, @i, @j, @k, @l, @m, @n, @o, @p, @r, @s, @t, @u, @w, @x, @y) " +
                        "RETURNING index_data_id;";

                    string sql_pic = "INSERT INTO iris_project_info.pic_data " +
                        "(pic_data_id, index_data_id, id_drogi, vnk, nnk, abs, version, buchst, station, " +
                        "seiher_km, filename, format, datum, lat, latns, lon, lonew, alt, heading, picpath, " +
                        "acc_lat, acc_lon, acc_alt, acc_heading, acc_roll, acc_pitch, roll, pitch, unix_time, pic_id) " +
                        "VALUES (gen_random_uuid(), @v01, @v02, @v03, @v04, @v05, @v06, @v07, @v08, @v09, @v10, @v11, @v12, @v13, @v14, @v15, @v16, @v17, @v18, @v19, @v20, @v21, @v22, @v23, @v24, @v25, @v26, @v27, @v28, @v29); ";
                    #endregion

                    // for every index find and insert current index and all PICs into database
                    foreach (IndexFileInfo index in xmlReader.ReadAllIndexFileInfo(indexPath))
                    {
                        var indexCommand = new NpgsqlCommand(sql_index, connection);
                        #region indexCommand.Parameters
                        indexCommand.Parameters.AddWithValue("a", index.vnk);
                        indexCommand.Parameters.AddWithValue("b", index.nnk);
                        indexCommand.Parameters.AddWithValue("c", index.von_stat);
                        indexCommand.Parameters.AddWithValue("d", index.bis_stat);
                        indexCommand.Parameters.AddWithValue("e", index.richtung);
                        indexCommand.Parameters.AddWithValue("f", index.cam);
                        indexCommand.Parameters.AddWithValue("g", DateTime.Parse(index.datum));
                        indexCommand.Parameters.AddWithValue("h", index.version);
                        indexCommand.Parameters.AddWithValue("i", index.bemerkung);
                        indexCommand.Parameters.AddWithValue("j", index.volume);
                        indexCommand.Parameters.AddWithValue("k", index.picpath);
                        indexCommand.Parameters.AddWithValue("l", index.v_seither_km);
                        indexCommand.Parameters.AddWithValue("m", index.n_seither_km);
                        indexCommand.Parameters.AddWithValue("n", index.abs);
                        indexCommand.Parameters.AddWithValue("o", index.str_bez);
                        indexCommand.Parameters.AddWithValue("p", index.laenge);
                        indexCommand.Parameters.AddWithValue("r", index.kierunek);
                        indexCommand.Parameters.AddWithValue("s", index.nrodc);
                        indexCommand.Parameters.AddWithValue("t", index.km_lokp);
                        indexCommand.Parameters.AddWithValue("u", index.km_lokk);
                        indexCommand.Parameters.AddWithValue("w", index.km_globp);
                        indexCommand.Parameters.AddWithValue("x", index.km_globk);
                        indexCommand.Parameters.AddWithValue("y", index.phoml);
                        #endregion

                        string indexUUID = "";
                        using (var reader = indexCommand.ExecuteReader())
                        { // here it executes with returning UUID value
                            if (reader.Read())
                                indexUUID = reader.GetString(0);
                        }

                        // find matching pic path with index path
                        for (int i = 0; i < picPaths.Length; i++)
                        {
                            DirectoryInfo currentDir = new DirectoryInfo(index.picpath);
                            DirectoryInfo picDir = new FileInfo(picPaths[i]).Directory;
                            string currentPath = $"{currentDir.Parent.Name}\\{currentDir.Name}";
                            string picDirPath = $"{picDir.Parent.Name}\\{picDir.Name}";

                            // if paths are the same it means that pic path was found to its according index path
                            if (currentPath.Equals(picDirPath))
                            {
                                #region Logger and ProgressBarManager Code
                                if (_log)
                                {
                                    FileInfo picFI = new FileInfo(picPaths[i]);
                                    DirectoryInfo picDI = picFI.Directory;
                                    _logger.Log($"Inserting: {picDI.Parent.Parent.Parent.Name}\\{picDI.Parent.Parent.Name}\\{picDI.Parent.Name}\\{picDI.Name}\\{picFI.Name}");
                                    _progressBarManager.StepProgressBar();
                                }
                                #endregion

                                foreach (PICFileInfo pic in xmlReader.ReadAllPicFileInfo(picPaths[i]))
                                {
                                    var picCommand = new NpgsqlCommand(sql_pic, connection);
                                    #region picCommand.Parameters
                                    picCommand.Parameters.AddWithValue("v01", indexUUID);
                                    picCommand.Parameters.AddWithValue("v02", pic.id_drogi);
                                    picCommand.Parameters.AddWithValue("v03", pic.vnk);
                                    picCommand.Parameters.AddWithValue("v04", pic.nnk);
                                    picCommand.Parameters.AddWithValue("v05", pic.abs);
                                    picCommand.Parameters.AddWithValue("v06", pic.version);
                                    picCommand.Parameters.AddWithValue("v07", pic.buchst);
                                    picCommand.Parameters.AddWithValue("v08", pic.station);
                                    picCommand.Parameters.AddWithValue("v09", pic.seiher_km);
                                    picCommand.Parameters.AddWithValue("v10", pic.filename);
                                    picCommand.Parameters.AddWithValue("v11", pic.format);
                                    picCommand.Parameters.AddWithValue("v12", DateTime.Parse(pic.datum));
                                    picCommand.Parameters.AddWithValue("v13", pic.lat);
                                    picCommand.Parameters.AddWithValue("v14", pic.latns);
                                    picCommand.Parameters.AddWithValue("v15", pic.lon);
                                    picCommand.Parameters.AddWithValue("v16", pic.lonew);
                                    picCommand.Parameters.AddWithValue("v17", pic.alt);
                                    picCommand.Parameters.AddWithValue("v18", pic.heading);
                                    picCommand.Parameters.AddWithValue("v19", pic.picpath);
                                    picCommand.Parameters.AddWithValue("v20", pic.acc_lat);
                                    picCommand.Parameters.AddWithValue("v21", pic.acc_lon);
                                    picCommand.Parameters.AddWithValue("v22", pic.acc_alt);
                                    picCommand.Parameters.AddWithValue("v23", pic.acc_heading);
                                    picCommand.Parameters.AddWithValue("v24", pic.acc_roll);
                                    picCommand.Parameters.AddWithValue("v25", pic.acc_pitch);
                                    picCommand.Parameters.AddWithValue("v26", pic.roll);
                                    picCommand.Parameters.AddWithValue("v27", pic.pitch);
                                    picCommand.Parameters.AddWithValue("v28", pic.unix_time);
                                    picCommand.Parameters.AddWithValue("v29", pic.pic_id);
                                    #endregion
                                    picCommand.ExecuteNonQuery();

                                    logPICRecords++;
                                }
                                break;
                            }
                        }
                        logIndexRecords++;
                    }

                    transaction.Commit();
                    #region Logger Code
                    if (_log) _logger.Log($"Commited {logIndexRecords} Index records and {logPICRecords} PIC records");
                    #endregion
                }
            }
        }


        public void InsertPIC(string picPath, string connectionString)
        {
            XmlFileReader xmlReader = new XmlFileReader();

            PICFileInfo[] pics = xmlReader.PicFileInfoArray(picPath);

            DirectoryInfo picDir = new FileInfo(picPath).Directory;
            DirectoryInfo indexDir = new FileInfo(picPath).Directory.Parent.Parent.Parent;
            string indexPath = indexDir.GetFiles("Index.xml")[0].FullName;
            IndexFileInfo index = null;
            foreach (IndexFileInfo file in xmlReader.ReadAllIndexFileInfo(indexPath))
            {
                string indexPicPath = $@"{picDir.Parent.Parent.Name}\{picDir.Parent.Name}\{picDir.Name}\";
                if (file.picpath.Equals(indexPicPath))
                    index = file;
            }


            
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    // INSERT index
                    #region SQL - INSERT INTO iris_project_info.index_data...
                    string sql_index = "INSERT INTO iris_project_info.index_data " +
                        "(index_data_id) " +
                        "VALUES (gen_random_uuid()) " +
                        "RETURNING index_data_id;";
                    #endregion
                    // INSERT pic
                    #region SQL - INSERT INTO iris_project_info.pic_data...
                    string sql_pic = "INSERT INTO iris_project_info.pic_data " +
                        "(pic_data_id, index_data_id, id_drogi, vnk, nnk, abs, version, buchst, station, " +
                        "seiher_km, filename, format, datum, lat, latns, lon, lonew, alt, heading, picpath, " +
                        "acc_lat, acc_lon, acc_alt, acc_heading, acc_roll, acc_pitch, roll, pitch, unix_time, pic_id) " +
                        "VALUES (gen_random_uuid(), @v01, @v02, @v03, @v04, @v05, @v06, @v07, @v08, @v09, @v10, @v11, @v12, " +
                        "@v13, @v14, @v15, @v16, @v17, @v18, @v19, @v20, @v21, @v22, @v23, @v24, @v25, @v26, @v27, @v28, @v29);";
                    #endregion
                    
                    string indexUUID = "";

                    using (var command = new NpgsqlCommand(sql_index, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            indexUUID = reader.GetString(0);
                    }

                    // INSERT pic
                    string kierunek = new DirectoryInfo(index.picpath).Parent.Name.Split('_')[2];
                    if (kierunek.Equals("M"))
                    {
                        for (int i = pics.Length - 1; i >= 0; i--)
                        {
                            using (var picCommand = new NpgsqlCommand(sql_pic, connection))
                            {
                                picCommand.Parameters.AddWithValue("v01", indexUUID);
                                #region picCommand.Parameters
                                picCommand.Parameters.AddWithValue("v02", pics[i].id_drogi);
                                picCommand.Parameters.AddWithValue("v03", pics[i].vnk);
                                picCommand.Parameters.AddWithValue("v04", pics[i].nnk);
                                picCommand.Parameters.AddWithValue("v05", pics[i].abs);
                                picCommand.Parameters.AddWithValue("v06", pics[i].version);
                                picCommand.Parameters.AddWithValue("v07", pics[i].buchst);
                                picCommand.Parameters.AddWithValue("v08", pics[i].station);
                                picCommand.Parameters.AddWithValue("v09", pics[i].seiher_km);
                                picCommand.Parameters.AddWithValue("v10", pics[i].filename);
                                picCommand.Parameters.AddWithValue("v11", pics[i].format);
                                picCommand.Parameters.AddWithValue("v12", DateTime.Parse(pics[i].datum));
                                picCommand.Parameters.AddWithValue("v13", pics[i].lat);
                                picCommand.Parameters.AddWithValue("v14", pics[i].latns);
                                picCommand.Parameters.AddWithValue("v15", pics[i].lon);
                                picCommand.Parameters.AddWithValue("v16", pics[i].lonew);
                                picCommand.Parameters.AddWithValue("v17", pics[i].alt);
                                picCommand.Parameters.AddWithValue("v18", pics[i].heading);
                                picCommand.Parameters.AddWithValue("v19", pics[i].picpath);
                                picCommand.Parameters.AddWithValue("v20", pics[i].acc_lat);
                                picCommand.Parameters.AddWithValue("v21", pics[i].acc_lon);
                                picCommand.Parameters.AddWithValue("v22", pics[i].acc_alt);
                                picCommand.Parameters.AddWithValue("v23", pics[i].acc_heading);
                                picCommand.Parameters.AddWithValue("v24", pics[i].acc_roll);
                                picCommand.Parameters.AddWithValue("v25", pics[i].acc_pitch);
                                picCommand.Parameters.AddWithValue("v26", pics[i].roll);
                                picCommand.Parameters.AddWithValue("v27", pics[i].pitch);
                                picCommand.Parameters.AddWithValue("v28", pics[i].unix_time);
                                picCommand.Parameters.AddWithValue("v29", pics[i].pic_id);
                                #endregion
                                picCommand.ExecuteNonQuery();
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < pics.Length; i++)
                        {
                            using (var picCommand = new NpgsqlCommand(sql_pic, connection))
                            {
                                picCommand.Parameters.AddWithValue("v01", indexUUID);
                                #region picCommand.Parameters
                                picCommand.Parameters.AddWithValue("v02", pics[i].id_drogi);
                                picCommand.Parameters.AddWithValue("v03", pics[i].vnk);
                                picCommand.Parameters.AddWithValue("v04", pics[i].nnk);
                                picCommand.Parameters.AddWithValue("v05", pics[i].abs);
                                picCommand.Parameters.AddWithValue("v06", pics[i].version);
                                picCommand.Parameters.AddWithValue("v07", pics[i].buchst);
                                picCommand.Parameters.AddWithValue("v08", pics[i].station);
                                picCommand.Parameters.AddWithValue("v09", pics[i].seiher_km);
                                picCommand.Parameters.AddWithValue("v10", pics[i].filename);
                                picCommand.Parameters.AddWithValue("v11", pics[i].format);
                                picCommand.Parameters.AddWithValue("v12", DateTime.Parse(pics[i].datum));
                                picCommand.Parameters.AddWithValue("v13", pics[i].lat);
                                picCommand.Parameters.AddWithValue("v14", pics[i].latns);
                                picCommand.Parameters.AddWithValue("v15", pics[i].lon);
                                picCommand.Parameters.AddWithValue("v16", pics[i].lonew);
                                picCommand.Parameters.AddWithValue("v17", pics[i].alt);
                                picCommand.Parameters.AddWithValue("v18", pics[i].heading);
                                picCommand.Parameters.AddWithValue("v19", pics[i].picpath);
                                picCommand.Parameters.AddWithValue("v20", pics[i].acc_lat);
                                picCommand.Parameters.AddWithValue("v21", pics[i].acc_lon);
                                picCommand.Parameters.AddWithValue("v22", pics[i].acc_alt);
                                picCommand.Parameters.AddWithValue("v23", pics[i].acc_heading);
                                picCommand.Parameters.AddWithValue("v24", pics[i].acc_roll);
                                picCommand.Parameters.AddWithValue("v25", pics[i].acc_pitch);
                                picCommand.Parameters.AddWithValue("v26", pics[i].roll);
                                picCommand.Parameters.AddWithValue("v27", pics[i].pitch);
                                picCommand.Parameters.AddWithValue("v28", pics[i].unix_time);
                                picCommand.Parameters.AddWithValue("v29", pics[i].pic_id);
                                #endregion
                                picCommand.ExecuteNonQuery();
                            }
                        }
                    }

                    // UPDATE index
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

                    transaction.Commit();
                }
            }
        }

    }
}
