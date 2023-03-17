namespace thsearch;

using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

class IndexSqlite : IIndex
{
    private readonly string dbPath;

    public IndexSqlite(string path)
    {
        this.dbPath = path;

        if (!File.Exists(dbPath))
        {
            CreateDatabase();
        }
    }

    private void CreateDatabase()
    {
        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            var createFilesTableCmd = connection.CreateCommand();
            createFilesTableCmd.CommandText = @"
                CREATE TABLE Files (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    path TEXT UNIQUE NOT NULL,
                    lastmodified TEXT NOT NULL
                )";
            createFilesTableCmd.ExecuteNonQuery();

            var createStemsTableCmd = connection.CreateCommand();
            createStemsTableCmd.CommandText = @"
                CREATE TABLE Stems (
                    stem TEXT PRIMARY KEY,
                    file_id INTEGER NOT NULL,
                    occurrences INTEGER NOT NULL,
                    FOREIGN KEY(file_id) REFERENCES Files(id)
                )";
            createStemsTableCmd.ExecuteNonQuery();
        }
    }

    public void Add(string path, FileIndexEntry entry)
    {
        int fileId;

        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            // FILES
            SqliteCommand upsertFileCmd = connection.CreateCommand();
            upsertFileCmd.CommandText = @"
                INSERT OR REPLACE INTO Files (path, lastmodified) 
                VALUES ($path, $lastmodified);
                SELECT last_insert_rowid();";
            upsertFileCmd.Parameters.AddWithValue("$path", path);
            upsertFileCmd.Parameters.AddWithValue("$lastmodified", entry.LastModified.ToString());

            var fileId64 = upsertFileCmd.ExecuteScalar();
            fileId = Convert.ToInt32(fileId64);
        }
        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            // STEMS
            // TODO: A query is executed for every single stem. Is there a way to batch this?
            foreach (string stem in entry.StemSet)
            {
                SqliteCommand upsertStemCmd = connection.CreateCommand();
                upsertStemCmd.CommandText = @"
                    INSERT OR REPLACE INTO Stems (stem, file_id, occurrences)
                    VALUES ($stem, $file_id, $occurrences)";
                upsertStemCmd.Parameters.AddWithValue("$stem", stem);
                upsertStemCmd.Parameters.AddWithValue("$file_id", fileId);
                upsertStemCmd.Parameters.AddWithValue("$occurrences", entry.stemFrequency[stem]);
                upsertStemCmd.ExecuteNonQuery();
            }
        }
    }


    public void Prune(List<string> foundFiles)
    {
        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();
            // Delete all files not found in the current directory
            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = @"
                DELETE FROM Files WHERE path NOT IN ($paths)";
            deleteCmd.Parameters.AddWithValue("$paths", String.Join(",", foundFiles));
            deleteCmd.ExecuteNonQuery();
        }
    }

    public bool FileUpToDate(FileModel file)
    {
        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            // Check if the file is in the Files table
            SqliteCommand selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"
                SELECT lastmodified FROM Files WHERE path = $path";
            selectCmd.Parameters.AddWithValue("$path", file.Path);
            
            string lastModifiedString = (string)selectCmd.ExecuteScalar();

            if (lastModifiedString == null)
            {
                return false;
            }
            else
            {
                return DateTime.Parse(lastModifiedString) >= file.LastModified;
            }
        }
    }

    public int GetFileCount()
    {
        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            // Count the number of files in the Files table
            SqliteCommand countCmd = connection.CreateCommand();
            countCmd.CommandText = "SELECT COUNT(*) FROM Files";

            return Convert.ToInt32(countCmd.ExecuteScalar());
        }
    }

    public string GetPath(int fileId)
    {
        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            // Get the path of the file with the given id
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"
                SELECT path FROM Files WHERE id = $file_id";
            selectCmd.Parameters.AddWithValue("$file_id", fileId);

            return (string)selectCmd.ExecuteScalar();
        }
    }

    public bool TryLookUpStem(string stem, out List<(int, int)> occurrences)
    {
        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            // Get the occurrences of the stem in the Stems table
            SqliteCommand selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"
                SELECT file_id, occurrences FROM Stems WHERE stem = $stem";
            selectCmd.Parameters.AddWithValue("$stem", stem);
            SqliteDataReader reader = selectCmd.ExecuteReader();

            occurrences = new List<(int, int)>();
            while (reader.Read())
            {
                int fileId = reader.GetInt32(0);
                int frequency = reader.GetInt32(1);
                occurrences.Add((fileId, frequency));
            }

            return occurrences.Count > 0;
        }
    }

    public void Finished()
    {
        // Nothing to do here
    }


}
