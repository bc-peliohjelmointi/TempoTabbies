using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;


public struct ScoreEntry
{
    public string profileName;
    public string mapName;
    public int score;
    public float accuracy;
    public string grade;
    public int maxCombo;
}
public static class ScoreDatabase
{
    private static string dbPath =>
        "URI=file:" + Path.Combine(Application.persistentDataPath, "scores.db");

    // Call once on game start
    public static void Initialize()
    {
        using var connection = new SqliteConnection(dbPath);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
        @"
        CREATE TABLE IF NOT EXISTS scores (
            profileName TEXT NOT NULL,
            mapName TEXT NOT NULL,
            score INTEGER,
            accuracy REAL,
            grade TEXT,
            maxCombo INTEGER,
            PRIMARY KEY (profileName, mapName)
        );
        ";
        command.ExecuteNonQuery();
    }

    public static void SaveScore(
        string profileName,
        string mapName,
        int score,
        float accuracy,
        string grade,
        int maxCombo)
    {
        using var connection = new SqliteConnection(dbPath);
        connection.Open();

        // Check existing score
        using var checkCmd = connection.CreateCommand();
        checkCmd.CommandText =
        @"
        SELECT score FROM scores
        WHERE profileName = @profile AND mapName = @map
        ";
        checkCmd.Parameters.AddWithValue("@profile", profileName);
        checkCmd.Parameters.AddWithValue("@map", mapName);

        object result = checkCmd.ExecuteScalar();

        // If score exists and is higher, do nothing
        if (result != null && score <= System.Convert.ToInt32(result))
            return;

        using var command = connection.CreateCommand();
        command.CommandText =
        @"
        INSERT OR REPLACE INTO scores
        (profileName, mapName, score, accuracy, grade, maxCombo)
        VALUES
        (@profile, @map, @score, @accuracy, @grade, @combo)
        ";

        command.Parameters.AddWithValue("@profile", profileName);
        command.Parameters.AddWithValue("@map", mapName);
        command.Parameters.AddWithValue("@score", score);
        command.Parameters.AddWithValue("@accuracy", accuracy);
        command.Parameters.AddWithValue("@grade", grade);
        command.Parameters.AddWithValue("@combo", maxCombo);

        command.ExecuteNonQuery();
    }

    public static bool LoadScore(
        string profileName,
        string mapName,
        out int score,
        out float accuracy,
        out string grade,
        out int maxCombo)
    {
        score = 0;
        accuracy = 0;
        grade = "N/A";
        maxCombo = 0;

        using var connection = new SqliteConnection(dbPath);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
        @"
        SELECT score, accuracy, grade, maxCombo
        FROM scores
        WHERE profileName = @profile AND mapName = @map
        ";

        command.Parameters.AddWithValue("@profile", profileName);
        command.Parameters.AddWithValue("@map", mapName);

        using IDataReader reader = command.ExecuteReader();
        if (!reader.Read())
            return false;

        score = reader.GetInt32(0);
        accuracy = reader.GetFloat(1);
        grade = reader.GetString(2);
        maxCombo = reader.GetInt32(3);

        return true;
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
        SELECT profileName, mapName, score, accuracy, grade, maxCombo
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
                        score = reader.GetInt32(2),
                        accuracy = reader.GetFloat(3),
                        grade = reader.GetString(4),
                        maxCombo = reader.GetInt32(5)
                    };

                    results.Add(entry);
                }
            }
        }

        return results;
    }
}
