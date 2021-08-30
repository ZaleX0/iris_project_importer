using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace IRISProjectImporter
{
    class SQLManager
    {
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


        public string[] InsertIndexFileInfos(IndexFileInfo[] indexFiles, string connectionString)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                string[] indexUUIDs = new string[indexFiles.Length];
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    string sql = "INSERT INTO iris_project_info.index_data " +
                        "(index_data_id, vnk, nnk, von_stat, bis_stat, richtung, cam, datum, version, bemerkung, volume, picpath, " +
                        "v_seither_km, n_seither_km, abs, str_bez, laenge, kierunek, nrodc, km_lokp, km_lokk, km_globp, km_globk, phoml) " +
                        "VALUES (gen_random_uuid(), @a, @b, @c, @d, @e, @f, @g, @h, @i, @j, @k, @l, @m, @n, @o, @p, @r, @s, @t, @u, @w, @x, @y) " +
                        "RETURNING index_data_id;";
                    for (int i = 0; i < indexFiles.Length; i++)
                    {
                        var command = new NpgsqlCommand(sql, connection);
                        command.Parameters.AddWithValue("a", indexFiles[i].vnk);
                        command.Parameters.AddWithValue("b", indexFiles[i].nnk);
                        command.Parameters.AddWithValue("c", indexFiles[i].von_stat);
                        command.Parameters.AddWithValue("d", indexFiles[i].bis_stat);
                        command.Parameters.AddWithValue("e", indexFiles[i].richtung);
                        command.Parameters.AddWithValue("f", indexFiles[i].cam);
                        command.Parameters.AddWithValue("g", DateTime.Parse(indexFiles[i].datum));
                        command.Parameters.AddWithValue("h", indexFiles[i].version);
                        command.Parameters.AddWithValue("i", indexFiles[i].bemerkung);
                        command.Parameters.AddWithValue("j", indexFiles[i].volume);
                        command.Parameters.AddWithValue("k", indexFiles[i].picpath);
                        command.Parameters.AddWithValue("l", indexFiles[i].v_seither_km);
                        command.Parameters.AddWithValue("m", indexFiles[i].n_seither_km);
                        command.Parameters.AddWithValue("n", indexFiles[i].abs);
                        command.Parameters.AddWithValue("o", indexFiles[i].str_bez);
                        command.Parameters.AddWithValue("p", indexFiles[i].laenge);
                        command.Parameters.AddWithValue("r", indexFiles[i].kierunek);
                        command.Parameters.AddWithValue("s", indexFiles[i].nrodc);
                        command.Parameters.AddWithValue("t", indexFiles[i].km_lokp);
                        command.Parameters.AddWithValue("u", indexFiles[i].km_lokk);
                        command.Parameters.AddWithValue("w", indexFiles[i].km_globp);
                        command.Parameters.AddWithValue("x", indexFiles[i].km_globk);
                        command.Parameters.AddWithValue("y", indexFiles[i].phoml);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                indexUUIDs[i] = reader.GetString(0);
                            }
                        }
                    }
                    transaction.Commit();
                    return indexUUIDs;
                }
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

                    string sql_index = "INSERT INTO iris_project_info.index_data " +
                        "(index_data_id, vnk, nnk, von_stat, bis_stat, richtung, cam, datum, version, bemerkung, volume, picpath, " +
                        "v_seither_km, n_seither_km, abs, str_bez, laenge, kierunek, nrodc, km_lokp, km_lokk, km_globp, km_globk, phoml) " +
                        "VALUES (gen_random_uuid(), @a, @b, @c, @d, @e, @f, @g, @h, @i, @j, @k, @l, @m, @n, @o, @p, @r, @s, @t, @u, @w, @x, @y) " +
                        "RETURNING index_data_id;";

                    string sql_pic = "INSERT INTO iris_project_info.pic_data " +
                        "(pic_data_id, index_data_id, id_drogi, vnk, nnk, abs, version, buchst, station, " +
                        "seiher_km, filename, format, datum, lat, latns, lon, lonns, alt, heading, picpath, " +
                        "acc_lat, acc_lon, acc_alt, acc_heading, acc_roll, acc_pitch, roll, pitch, unix_time, pic_id) " +
                        "VALUES (gen_random_uuid(), @v01, @v02, @v03, @v04, @v05, @v06, @v07, @v08, @v09, @v10, @v11, @v12, @v13, @v14, @v15, @v16, @v17, @v18, @v19, @v20, @v21, @v22, @v23, @v24, @v25, @v26, @v27, @v28, @v29); ";

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
                        using (var reader = indexCommand.ExecuteReader()) // here it executes with returning UUID value
                        {
                            if (reader.Read())
                                indexUUID = reader.GetString(0);
                        }

                        // find matching pic path with index path
                        for (int i = 0; i < picPaths.Length; i++)
                        {
                            DirectoryInfo currentDir = new DirectoryInfo(index.picpath);
                            DirectoryInfo picDir = new FileInfo(picPaths[i]).Directory;
                            string currentPath = currentDir.Parent.Name + "\\" + currentDir.Name;
                            string picDirPath = picDir.Parent.Name + "\\" + picDir.Name;

                            // if paths are the same it means that pic path was found to its according index path
                            if (currentPath.Equals(picDirPath))
                            {
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
                                    picCommand.Parameters.AddWithValue("v16", pic.lonns);
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
                                break;
                            }
                        }
                    }

                    transaction.Commit();
                    Console.WriteLine("# COMMIT #");
                }
            }
        }

    }
}
