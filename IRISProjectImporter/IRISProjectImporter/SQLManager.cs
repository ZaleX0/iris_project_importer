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



        public void InsertIndexWithPICs(string indexPath, string[] picPaths, string connectionString)
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

        public void test(string indexPath, string connectionString)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    XmlFileReader xmlReader = new XmlFileReader();

                    IndexFileInfo[] indexes = xmlReader.ReadAllIndexFileInfo(indexPath).ToArray();
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

                        try // TODO: insert pozostałych danych
                        {
                            string picPath = $"{new FileInfo(indexPath).DirectoryName}\\{indexes[i].picpath}";
                            picPath = new DirectoryInfo(picPath).GetFiles("PIC_*.xml")[0].FullName;
                            foreach (PICFileInfo pic in xmlReader.ReadAllPicFileInfo(picPath))
                            {
                                #region SQL - INSERT INTO iris_project_info.pic_data...
                                string sql_pic = "INSERT INTO iris_project_info.pic_data " +
                                    "(pic_data_id, index_data_id, id_drogi, vnk, nnk, abs, version, buchst, station, " +
                                    "seiher_km, filename, format, datum, lat, latns, lon, lonew, alt, heading, picpath, " +
                                    "acc_lat, acc_lon, acc_alt, acc_heading, acc_roll, acc_pitch, roll, pitch, unix_time, pic_id) " +
                                    "VALUES (gen_random_uuid(), @v01, @v02, @v03, @v04, @v05, @v06, @v07, @v08, @v09, @v10, @v11, @v12, " +
                                    "@v13, @v14, @v15, @v16, @v17, @v18, @v19, @v20, @v21, @v22, @v23, @v24, @v25, @v26, @v27, @v28, @v29);";
                                #endregion
                                using (var picCommand = new NpgsqlCommand(sql_pic, connection))
                                {
                                    picCommand.Parameters.AddWithValue("v01", indexUUIDs[i]);
                                    #region picCommand.Parameters
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
                                }
                            }
                        }
                        catch (Exception)
                        {

                            throw;
                        }
                        
                    }
                    
                    transaction.Commit();
                }
            }
        }



        public Task LoadDataGridViewAsync(DataGridView dataGridView, string connectionString)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            //using (var adapter = new NpgsqlDataAdapter("SELECT * FROM iris_project_info.index_data", connection))
            using (var adapter = new NpgsqlDataAdapter("SELECT * FROM iris_project_info.pic_data LIMIT 100000", connection))
            {
                connection.Open();
                DataTable dataTable = new DataTable("IndexTable");
                adapter.Fill(dataTable);

                Stopwatch s = Stopwatch.StartNew();

                dataGridView.DataSource = dataTable;

                s.Stop();
                Console.WriteLine(s.ElapsedMilliseconds);
                return Task.CompletedTask;
            }
        }

    }
}
