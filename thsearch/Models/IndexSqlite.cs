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
                    id INTEGER PRIMARY KEY,
                    path TEXT UNIQUE NOT NULL,
                    lastmodified TEXT NOT NULL
                )";
            createFilesTableCmd.ExecuteNonQuery();

            var createStemsTableCmd = connection.CreateCommand();
            createStemsTableCmd.CommandText = @"
                CREATE TABLE Stems (
                    id INTEGER PRIMARY KEY,
                    stem TEXT,
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

            //using (var transaction = connection.BeginTransaction)

            upsertFileCmd.CommandText = @"
                UPDATE Files SET lastmodified = $lastmodified WHERE path = $path;
                INSERT OR IGNORE INTO Files (path, lastmodified) VALUES ($path, $lastmodified);
                SELECT last_insert_rowid();";
            upsertFileCmd.Parameters.AddWithValue("$path", path);
            upsertFileCmd.Parameters.AddWithValue("$lastmodified", entry.LastModified.ToString());

            //  'SQLite Error 19: 'FOREIGN KEY constraint failed'.'
            var fileId64 = upsertFileCmd.ExecuteScalar();
            fileId = Convert.ToInt32(fileId64);
        }

        AddStems(fileId, entry);
    }

    private void AddStems(int fileId, FileIndexEntry entry)
    {
        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();

            using (var transaction = connection.BeginTransaction())
            {
                command.Transaction = transaction;

                foreach (var stem in entry.StemSet)
                {
                    command.Parameters.Clear(); // Clear parameters before adding new ones
                    command.CommandText = $"INSERT OR REPLACE INTO Stems (stem, file_id, occurrences) VALUES ($stem, $file_id, $occurrences)";
                    command.Parameters.AddWithValue("$stem", stem);
                    command.Parameters.AddWithValue("$file_id", fileId);
                    command.Parameters.AddWithValue("$occurrences", entry.stemFrequency[stem]);

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

    }


    public void Prune(List<string> foundFiles)
    {
        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            // Delete stems not associated with files in foundFiles
            SqliteCommand deleteStemsCmd = connection.CreateCommand();
            string foundPlaceholders = string.Join(",", foundFiles.Select((s, i) => $"@path{i}"));
            deleteStemsCmd.CommandText = $@"
                DELETE FROM Stems
                WHERE file_id NOT IN (
                    SELECT id FROM Files WHERE path IN ({foundPlaceholders})
                );
                DELETE FROM Files WHERE path NOT IN ({foundPlaceholders})";
            for (var i = 0; i < foundFiles.Count; i++)
            {
                deleteStemsCmd.Parameters.AddWithValue($"@path{i}", foundFiles[i]);
            }
            deleteStemsCmd.ExecuteNonQuery();
        }
    }

    public bool RecordUpToDate(FileModel file)
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

            if (lastModifiedString == null ) { return false;  }

            DateTime dbFileDate = DateTime.Parse(lastModifiedString);

            return DateTime.Compare
            (
                TruncateMilliseconds(dbFileDate),
                TruncateMilliseconds(file.LastModified)
            ) == 0;

        }
    }
    private DateTime TruncateMilliseconds(DateTime dateTime)
    {
        return dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerSecond));
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
