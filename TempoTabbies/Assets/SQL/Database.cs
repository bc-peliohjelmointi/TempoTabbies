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
                PRIMARY KEY (profileName, mapName, difficulty)
            );
            ";
            command.ExecuteNonQuery();
        }

        Debug.Log("[ScoreDatabase] Initialized at: " + Application.persistentDataPath);
    }

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

            IDbCommand command = connection.CreateCommand();
            command.CommandText =
            @"
            INSERT OR REPLACE INTO scores
            (profileName, mapName, difficulty, score, accuracy, grade, maxCombo, clearType)
            VALUES
            (@profile, @map, @difficulty, @score, @accuracy, @grade, @combo, @clear)
            ";

            command.Parameters.Add(new SqliteParameter("@profile", profileName));
            command.Parameters.Add(new SqliteParameter("@map", mapName));
            command.Parameters.Add(new SqliteParameter("@difficulty", difficulty));
            command.Parameters.Add(new SqliteParameter("@score", score));
            command.Parameters.Add(new SqliteParameter("@accuracy", accuracy));
            command.Parameters.Add(new SqliteParameter("@grade", grade));
            command.Parameters.Add(new SqliteParameter("@combo", maxCombo));
            command.Parameters.Add(new SqliteParameter("@clear", clearType));

            command.ExecuteNonQuery();
        }

        Debug.Log($"[DB SAVE] {profileName} | {mapName} | {difficulty} | {score}");
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
            SELECT profileName, mapName, difficulty, score, accuracy, grade, maxCombo, clearType
            FROM scores
            ORDER BY score DESC
            ";

            using (IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    ScoreEntry entry = new ScoreEntry
                    {
                        profileName = reader.GetString(0),
                        mapName = reader.GetString(1),
                        difficulty = reader.GetString(2),
                        score = reader.GetInt32(3),
                        accuracy = reader.GetFloat(4),
                        grade = reader.GetString(5),
                        maxCombo = reader.GetInt32(6),
                        clearType = reader.IsDBNull(7) ? "Unknown" : reader.GetString(7)
                    };
                    results.Add(entry);
                }
            }
        }

        return results;
    }
}
