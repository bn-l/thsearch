namespace thsearch;

using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Collections.Concurrent;
using System.Diagnostics;

class IndexSqlite : IIndex
{
    private readonly string dbPath;

    // private readonly ConcurrentBag<(int fileId, string stem, int occurrences)> stemsToInsert;



    public IndexSqlite(string path)
    {
        this.dbPath = path;

        if (!File.Exists(dbPath) || new FileInfo(dbPath).Length == 0) {  CreateDatabase();  }

        // stemsToInsert = new ConcurrentBag<(int fileId, string stem, int occurrences)>();
    
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
                );
                CREATE INDEX idx_files_path ON Files (path);
            ";
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


    // TODO: Split into update and insert
    // Add can be reused as update 
    // Insert can add to a concurrent bag after all the updates are made. It will get the highest id number, and then add to the bag, incrementing that number as the id.


    public void Add(string path, FileIndexEntry entry)
    {
        int fileId;

        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            // FILES
            SqliteCommand upsertFileCmd = connection.CreateCommand();

            upsertFileCmd.CommandText = @"
                INSERT INTO Files (path, lastmodified)
                VALUES ($path, $lastmodified)
                ON CONFLICT (path) DO UPDATE SET
                    lastmodified=excluded.lastmodified
                ;
                SELECT id FROM Files WHERE path=$path;
            ";
            upsertFileCmd.Parameters.AddWithValue("$path", path);
            upsertFileCmd.Parameters.AddWithValue("$lastmodified", entry.LastModified.ToString());

            //  'SQLite Error 19: 'FOREIGN KEY constraint failed'.'
            var fileId64 = upsertFileCmd.ExecuteScalar();
            fileId = Convert.ToInt32(fileId64);

            AddStems(fileId, entry, connection);
        }

    }

    private void AddStems(int fileId, FileIndexEntry entry, SqliteConnection connection)
    {

        SqliteCommand command = connection.CreateCommand();

        using (var transaction = connection.BeginTransaction())
        {
            command.Transaction = transaction;

            foreach (var stem in entry.StemSet)
            {
                command.Parameters.Clear(); // Clear parameters before adding new ones
                command.CommandText = $@"
                    INSERT OR REPLACE INTO Stems (stem, file_id, occurrences) VALUES ($stem, $file_id, $occurrences);
                ";
                command.Parameters.AddWithValue("$stem", stem);
                command.Parameters.AddWithValue("$file_id", fileId);
                command.Parameters.AddWithValue("$occurrences", entry.stemFrequency[stem]);

                command.ExecuteNonQuery();
            }

            transaction.Commit();
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

    public void Prune(List<string> foundFiles)
    {
        // TODO: Get list of deleted files only, run delete specifically on those files 

        // Get list of deleted files by comparing the list of files in the database to the list of files found in the directory


        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            Stopwatch stopwatch = new Stopwatch();





        
            SqliteCommand selectDeletedCmd = connection.CreateCommand();
            SqliteCommand deleteStemsCmd = connection.CreateCommand();
            SqliteCommand deleteFilesCmd = connection.CreateCommand();
            SqliteCommand vacuumCmd = connection.CreateCommand();


            selectDeletedCmd.CommandText = @"
                SELECT id FROM Files WHERE path NOT IN (" + string.Join(",", foundFiles.Select((s, i) => $"'{s}'")) + @")
            ";

            vacuumCmd.CommandText = "VACUUM";


            List<int> deletedFiles = new List<int>();
            SqliteDataReader reader = selectDeletedCmd.ExecuteReader();
            while (reader.Read())
            {
                deletedFiles.Add(reader.GetInt32(0));
            }

            string deleteTextArray = string.Join(",", deletedFiles.Select(x => $"'{x}'"));

            deleteStemsCmd.CommandText = @"
                DELETE FROM Stems WHERE file_id IN (" + deleteTextArray + @")
                ;
            ";

            deleteFilesCmd.CommandText = @"
                DELETE FROM Files WHERE id IN (" + deleteTextArray + @")
                ;
            ";


            if (deletedFiles.Count > 0)
            {
                stopwatch.Start();  // !START

                deleteStemsCmd.ExecuteNonQuery();

                stopwatch.Reset(); 
                Console.WriteLine($"It took {stopwatch.ElapsedMilliseconds} ms to run deleteStemsCmd");

                stopwatch.Start();  // !START

                deleteFilesCmd.ExecuteNonQuery();

                stopwatch.Reset(); 
                Console.WriteLine($"It took {stopwatch.ElapsedMilliseconds} ms to run deleteFilesCmd");

            }
            else if (deletedFiles.Count > 200)
            {
                stopwatch.Start();  // !START

                vacuumCmd.ExecuteNonQuery();

                stopwatch.Reset(); 
                Console.WriteLine($"It took {stopwatch.ElapsedMilliseconds} ms to run vacuumCmd");
                stopwatch.Reset(); 
            }
            
        }
    }

    public void Finished()
    {


    }


}
