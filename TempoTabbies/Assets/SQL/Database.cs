using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;

public struct ScoreEntry
{
    public string profileName;
    public string mapName;
    public string difficulty;
    public int score;
    public float accuracy;
    public string grade;
    public int maxCombo;
    public string clearType;
    public int playCount;
}

public static class ScoreDatabase
{
    private static string dbPath =>
        "URI=file:" + Path.Combine(Application.persistentDataPath, "scores.db");

    public static void Initialize()
    {
        using (IDbConnection connection = new SqliteConnection(dbPath))
        {
            connection.Open();

            IDbCommand command = connection.CreateCommand();
            command.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS scores (
                profileName TEXT NOT NULL,
                mapName TEXT NOT NULL,
                difficulty TEXT NOT NULL,
                score INTEGER,
                accuracy REAL,
                grade TEXT,
                maxCombo INTEGER,
                clearType TEXT,
                playcount INTEGER,
                PRIMARY KEY (profileName, mapName, difficulty)
            );
            ";
            command.ExecuteNonQuery();
        }

        Debug.Log("[ScoreDatabase] Initialized at: " + Application.persistentDataPath);
    }

    // Note: playcount is now managed by the DB (incremented on each save) - clearType is last parameter.
    public static void SaveScore(
        string profileName,
        string mapName,
        string difficulty,
        int score,
        float accuracy,
        string grade,
        int maxCombo,
        string clearType)
    {
        using (IDbConnection connection = new SqliteConnection(dbPath))
        {
            connection.Open();

            // Read existing playcount (if any) so we can increment
            int existingPlayCount = 0;
            using (IDbCommand selectCmd = connection.CreateCommand())
            {
                selectCmd.CommandText = @"
                    SELECT playcount
                    FROM scores
                    WHERE profileName = @profile AND mapName = @map AND difficulty = @difficulty
                ";
                selectCmd.Parameters.Add(new SqliteParameter("@profile", profileName));
                selectCmd.Parameters.Add(new SqliteParameter("@map", mapName));
                selectCmd.Parameters.Add(new SqliteParameter("@difficulty", difficulty));

                using (IDataReader reader = selectCmd.ExecuteReader())
                {
                    if (reader.Read() && !reader.IsDBNull(0))
                    {
                        existingPlayCount = reader.GetInt32(0);
                    }
                }
            }

            int newPlayCount = existingPlayCount + 1;

            // Upsert the row, writing the new playcount
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText =
                @"
                INSERT OR REPLACE INTO scores
                (profileName, mapName, difficulty, score, accuracy, grade, maxCombo, clearType, playcount)
                VALUES
                (@profile, @map, @difficulty, @score, @accuracy, @grade, @combo, @clear, @playcount)
                ";

                command.Parameters.Add(new SqliteParameter("@profile", profileName));
                command.Parameters.Add(new SqliteParameter("@map", mapName));
                command.Parameters.Add(new SqliteParameter("@difficulty", difficulty));
                command.Parameters.Add(new SqliteParameter("@score", score));
                command.Parameters.Add(new SqliteParameter("@accuracy", accuracy));
                command.Parameters.Add(new SqliteParameter("@grade", grade));
                command.Parameters.Add(new SqliteParameter("@combo", maxCombo));
                command.Parameters.Add(new SqliteParameter("@clear", clearType ?? "Unknown"));
                command.Parameters.Add(new SqliteParameter("@playcount", newPlayCount));

                command.ExecuteNonQuery();
            }

            Debug.Log($"[DB SAVE] {profileName} | {mapName} | {difficulty} | {score} | playcount={newPlayCount}");
        }
    }

    public static List<ScoreEntry> GetAllScores()
    {
        List<ScoreEntry> results = new();

        using (IDbConnection connection = new SqliteConnection(dbPath))
        {
            connection.Open();

            IDbCommand command = connection.CreateCommand();
            command.CommandText =
            @"
            SELECT profileName, mapName, difficulty, score, accuracy, grade, maxCombo, clearType, playcount
            FROM scores
            ORDER BY score DESC
            ";

            using (IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    ScoreEntry entry = new ScoreEntry
                    {
                        profileName = reader.IsDBNull(0) ? "Unknown" : reader.GetString(0),
                        mapName = reader.IsDBNull(1) ? "Unknown" : reader.GetString(1),
                        difficulty = reader.IsDBNull(2) ? "Unknown" : reader.GetString(2),
                        score = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                        accuracy = reader.IsDBNull(4) ? 0f : reader.GetFloat(4),
                        grade = reader.IsDBNull(5) ? "Unknown" : reader.GetString(5),
                        maxCombo = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                        clearType = reader.IsDBNull(7) ? "Unknown" : reader.GetString(7),
                        playCount = reader.IsDBNull(8) ? 0 : reader.GetInt32(8)
                    };
                    results.Add(entry);
                }
            }
        }

        return results;
    }

    // Get scores for a specific map and difficulty
    public static List<ScoreEntry> GetScores(string mapName, string difficulty)
    {
        List<ScoreEntry> results = new();

        using (IDbConnection connection = new SqliteConnection(dbPath))
        {
            connection.Open();

            IDbCommand command = connection.CreateCommand();
            command.CommandText = @"
            SELECT profileName, mapName, difficulty, score, accuracy, grade, maxCombo, clearType, playcount
            FROM scores
            WHERE mapName = @map AND difficulty = @difficulty
            ORDER BY score DESC
            ";

            command.Parameters.Add(new SqliteParameter("@map", mapName));
            command.Parameters.Add(new SqliteParameter("@difficulty", difficulty));

            using (IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    ScoreEntry entry = new ScoreEntry
                    {
                        profileName = reader.IsDBNull(0) ? "Unknown" : reader.GetString(0),
                        mapName = reader.IsDBNull(1) ? "Unknown" : reader.GetString(1),
                        difficulty = reader.IsDBNull(2) ? "Unknown" : reader.GetString(2),
                        score = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                        accuracy = reader.IsDBNull(4) ? 0f : reader.GetFloat(4),
                        grade = reader.IsDBNull(5) ? "Unknown" : reader.GetString(5),
                        maxCombo = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                        clearType = reader.IsDBNull(7) ? "Unknown" : reader.GetString(7),
                        playCount = reader.IsDBNull(8) ? 0 : reader.GetInt32(8)
                    };
                    results.Add(entry);
                }
            }
        }

        return results;
    }
}
