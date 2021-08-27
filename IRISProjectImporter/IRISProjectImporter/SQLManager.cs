using System;
using System.Collections.Generic;
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

        public void InsertIndexFileWithPICs(IndexFileInfo indexFile, PICFileInfo[] picFiles, string connectionString)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    string sql = "INSERT INTO index_data " +
                        "(index_data_id, vnk, nnk, von_stat, bis_stat, richtung, cam, datum, version, bemerkung, volume, picpath, " +
                        "v_seither_km, n_seither_km, abs, str_bez, laenge, kierunek, nrodc, km_lokp, km_lokk, km_globp, km_globk, phoml) " +
                        "VALUES (gen_random_uuid(), @a, @b, @c, @d, @e, @f, @g, @h, @i, @j, @k, @l, @m, @n, @o, @p, @r, @s, @t, @u, @w, @x, @y) " +
                        "RETURNING index_data_id;";
                    var indexCommand = new NpgsqlCommand(sql, connection);
                    indexCommand.Parameters.AddWithValue("a", indexFile.vnk);
                    indexCommand.Parameters.AddWithValue("b", indexFile.nnk);
                    indexCommand.Parameters.AddWithValue("c", indexFile.von_stat);
                    indexCommand.Parameters.AddWithValue("d", indexFile.bis_stat);
                    indexCommand.Parameters.AddWithValue("e", indexFile.richtung);
                    indexCommand.Parameters.AddWithValue("f", indexFile.cam);
                    indexCommand.Parameters.AddWithValue("g", indexFile.datum);
                    indexCommand.Parameters.AddWithValue("h", indexFile.version);
                    indexCommand.Parameters.AddWithValue("i", indexFile.bemerkung);
                    indexCommand.Parameters.AddWithValue("j", indexFile.volume);
                    indexCommand.Parameters.AddWithValue("k", indexFile.picpath);
                    indexCommand.Parameters.AddWithValue("l", indexFile.v_seither_km);
                    indexCommand.Parameters.AddWithValue("m", indexFile.n_seither_km);
                    indexCommand.Parameters.AddWithValue("n", indexFile.abs);
                    indexCommand.Parameters.AddWithValue("o", indexFile.str_bez);
                    indexCommand.Parameters.AddWithValue("p", indexFile.laenge);
                    indexCommand.Parameters.AddWithValue("r", indexFile.kierunek);
                    indexCommand.Parameters.AddWithValue("s", indexFile.nrodc);
                    indexCommand.Parameters.AddWithValue("t", indexFile.km_lokp);
                    indexCommand.Parameters.AddWithValue("u", indexFile.km_lokk);
                    indexCommand.Parameters.AddWithValue("w", indexFile.km_globp);
                    indexCommand.Parameters.AddWithValue("x", indexFile.km_globk);
                    indexCommand.Parameters.AddWithValue("y", indexFile.phoml);

                    using (var reader = indexCommand.ExecuteReader())
                    {
                        indexFile.index_data_id = reader.GetString(0);
                        Console.WriteLine(indexFile.index_data_id);
                    }

                    for (int i = 0; i < picFiles.Length; i++)
                    {
                        var picCommand = new NpgsqlCommand(sql, connection);
                        picCommand.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
        }


        public string[] InsertIndexFileInfos(IndexFileInfo[] indexFiles, string connectionString)
        {
            //TODO: do multiple index files in transaction and return index_data_id array
            throw new NotImplementedException();
        }





        public void test(IndexFileInfo indexFile, string connectionString)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    string sql = "INSERT INTO iris_project_info.index_data " +
                        "(index_data_id, vnk, nnk, von_stat, bis_stat, richtung, cam, datum, version, bemerkung, volume, picpath, " +
                        "v_seither_km, n_seither_km, abs, str_bez, laenge, kierunek, nrodc, km_lokp, km_lokk, km_globp, km_globk, phoml) " +
                        "VALUES (gen_random_uuid(), @a, @b, @c, @d, @e, @f, now(), @h, @i, @j, @k, @l, @m, @n, @o, @p, @r, @s, @t, @u, @w, @x, @y) " +
                        "RETURNING index_data_id;";
                    var indexCommand = new NpgsqlCommand(sql, connection);
                    indexCommand.Parameters.AddWithValue("a", indexFile.vnk);
                    indexCommand.Parameters.AddWithValue("b", indexFile.nnk);
                    indexCommand.Parameters.AddWithValue("c", indexFile.von_stat);
                    indexCommand.Parameters.AddWithValue("d", indexFile.bis_stat);
                    indexCommand.Parameters.AddWithValue("e", indexFile.richtung);
                    indexCommand.Parameters.AddWithValue("f", indexFile.cam);
                    //indexCommand.Parameters.AddWithValue("g", indexFile.datum);
                    indexCommand.Parameters.AddWithValue("h", indexFile.version);
                    indexCommand.Parameters.AddWithValue("i", indexFile.bemerkung);
                    indexCommand.Parameters.AddWithValue("j", indexFile.volume);
                    indexCommand.Parameters.AddWithValue("k", indexFile.picpath);
                    indexCommand.Parameters.AddWithValue("l", indexFile.v_seither_km);
                    indexCommand.Parameters.AddWithValue("m", indexFile.n_seither_km);
                    indexCommand.Parameters.AddWithValue("n", indexFile.abs);
                    indexCommand.Parameters.AddWithValue("o", indexFile.str_bez);
                    indexCommand.Parameters.AddWithValue("p", indexFile.laenge);
                    indexCommand.Parameters.AddWithValue("r", indexFile.kierunek);
                    indexCommand.Parameters.AddWithValue("s", indexFile.nrodc);
                    indexCommand.Parameters.AddWithValue("t", indexFile.km_lokp);
                    indexCommand.Parameters.AddWithValue("u", indexFile.km_lokk);
                    indexCommand.Parameters.AddWithValue("w", indexFile.km_globp);
                    indexCommand.Parameters.AddWithValue("x", indexFile.km_globk);
                    indexCommand.Parameters.AddWithValue("y", indexFile.phoml);

                    using (var reader = indexCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            indexFile.index_data_id = reader.GetString(0);
                            Console.WriteLine(indexFile.index_data_id);
                        }
                    }
                    transaction.Commit();
                }
            }
        }



    }
}
