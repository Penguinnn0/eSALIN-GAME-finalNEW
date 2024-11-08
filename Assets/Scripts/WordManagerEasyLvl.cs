using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Data;
using Mono.Data.Sqlite;
using System;

public class GameManager : MonoBehaviour
{
    private string dbPath;

    private struct WordData
    {
        public int wordID;
        public string Tagalog;
        public string English;
        public string Definition;
    }

    private List<WordData> enemyWordList = new List<WordData>();
    public TextMeshProUGUI DefinitionText;
    public TextMeshProUGUI Choice1Text;
    public TextMeshProUGUI Choice2Text;
    public TextMeshProUGUI Choice3Text;

    static string correctAnswer;
    static List<int> enemies = new List<int>(); // Track enemies
    static List<string> english = new List<string>(); // Track english correct answer
    static List<string> definition = new List<string>(); // Track definitions
    static List<string> choices = new List<string>();
    private bool isFirstEnemy = true;

    private void Start()
    {
        dbPath = "URI=file:" + Application.dataPath + "/Plugins/eSALINdatabaseFinal.db";
        LoadUniqueEnemyWords("advancedWrds_tbl");

        Choice1Text.GetComponentInParent<Button>().onClick.AddListener(() => OnChoiceSelected(Choice1Text.text));
        Choice2Text.GetComponentInParent<Button>().onClick.AddListener(() => OnChoiceSelected(Choice2Text.text));
        Choice3Text.GetComponentInParent<Button>().onClick.AddListener(() => OnChoiceSelected(Choice3Text.text)); 
    }

    private void LoadUniqueEnemyWords(string tableName)
    {
        enemies.Clear();
        english.Clear();
        definition.Clear();
        
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            string query = $"SELECT adv_ID, tagalog, english, definition FROM {tableName} ORDER BY RANDOM()";

            using (var command = new SqliteCommand(query, connection))
            {
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        WordData wordData = new WordData
                        {
                            wordID = reader.GetInt32(0),
                            Tagalog = reader.GetString(1),
                            English = reader.GetString(2),
                            Definition = reader.GetString(3)
                        };

                        if (!enemyWordList.Exists(w => w.Tagalog == wordData.Tagalog && w.English == wordData.English))
                        {
                            enemyWordList.Add(wordData);
                            Debug.Log($"Loaded word: Tagalog='{wordData.Tagalog}', English='{wordData.English}', Definition='{wordData.Definition}', ID={wordData.wordID}");
                        }
                    }
                }
            }
            connection.Close();
        }
    }

    public void SetupEnemy(Enemy enemy)
    {
        if (enemyWordList.Count == 0)
        {
            Debug.LogWarning("No more words available.");
            return;
        }

        WordData wordData = enemyWordList[0];
        enemyWordList.RemoveAt(0);
        enemy.SetTagalogText(wordData.Tagalog);
        enemy.SetID(wordData.wordID);
        enemies.Add(wordData.wordID); 
        english.Add(wordData.English);
        definition.Add(wordData.Definition);
        
        Debug.Log($"Enemy setup: Tagalog='{wordData.Tagalog}', English='{wordData.English}', Definition='{wordData.Definition}', ID={wordData.wordID}");

        // Set the definition and choices only for the first enemy
        if (isFirstEnemy)
        {
            SetupChoicesAndDefinition();
            isFirstEnemy = false;
        }

        Debug.Log($"-----Current number of enemies in List: {enemies.Count}, ID: {enemies[0]}");
    }

    private void SetupChoicesAndDefinition()
    {
        if (definition.Count > 0 && english.Count > 0)
        {
            DefinitionText.text = definition[0];
            correctAnswer = english[0];
            Debug.Log($"Setting definition and correct answer: Definition='{definition[0]}', Correct Answer='{correctAnswer}'");
            SetupChoices();
        }
        else
        {
            Debug.LogWarning("No definition or correct answer available.");
        }
    }

    private void SetupChoices()
    {
        choices.Clear();
        choices.Add(correctAnswer);
        Debug.Log("Adding correct answer to choices list.");

        // Add two unique distractors
        while (choices.Count < 3)
        {
            string distractor = GetRandomEnglishWord("advancedWrds_tbl");
            if (!choices.Contains(distractor))
            {
                choices.Add(distractor);
            }
        }

        choices = ShuffleList(choices);
        Debug.Log($"Final choices: {choices[0]}, {choices[1]}, {choices[2]}");

        Choice1Text.text = choices[0];
        Choice2Text.text = choices[1];
        Choice3Text.text = choices[2];
    }

    public void OnChoiceSelected(string selectedChoice)
    {
        Debug.Log("Enemy on the list:" + enemies.Count);
        Debug.Log("Enemy ID:" + enemies[0]);
        Debug.Log($"Selected choice: '{selectedChoice}', Correct answer: '{correctAnswer}'");

        if (enemies.Count > 0)
            // Log what is in the first index of the enemies list
            Debug.Log($"First enemy ID in list: {enemies[0]}");
        {
            if (selectedChoice == correctAnswer)
            {
                Debug.Log("Correct answer selected.");
                ProceedToNextWord();
                DestroyCurrentEnemy();
            }
            else
            {
                Debug.Log("Incorrect answer.");
                HandleIncorrectAnswer();
                DestroyCurrentEnemy();
            }
        }
    }

    private void DestroyCurrentEnemy()
    {
        if (enemies.Count > 0)
        {
            int enemyID = enemies[0];
            Enemy enemyToDestroy = null;
            Debug.Log($"Attempting to destroy enemy with ID={enemyID}");


            foreach (Enemy enemy in FindObjectsOfType<Enemy>())
            {
                if (enemy.ID == enemyID)
                {
                    enemyToDestroy = enemy;
                    break;
                }
            }

            if (enemyToDestroy != null)
            {
                Debug.Log($"Destroying enemy with ID={enemyID}");
                Destroy(enemyToDestroy.gameObject);
                Debug.Log($"Destroyed enemy with ID={enemyID}");
                enemies.RemoveAt(0);
                english.RemoveAt(0);
                definition.RemoveAt(0);
            }
            else
            {
                Debug.LogWarning("Enemy with ID " + enemyID + " not found.");
            }
        }
    }

    private void ProceedToNextWord()
    {
        if (enemies.Count > 0 && english.Count > 0 && definition.Count > 0)
        {
            Debug.Log("Proceeding to next word.");
            SetupChoicesAndDefinition();
        }
        else
        {
            Debug.LogWarning("No more words to display.");
        }
    }

    private void HandleIncorrectAnswer()
    {
        Debug.Log("Handling incorrect answer.");
        ProceedToNextWord();

    }
    
    private string GetRandomEnglishWord(string tableName)
    {
        string randomWord = null;
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            string query = $"SELECT english FROM {tableName} ORDER BY RANDOM() LIMIT 1";
            using (var command = new SqliteCommand(query, connection))
            {
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        randomWord = reader.GetString(0);
                        Debug.Log($"Fetched random distractor: '{randomWord}'");
                    }
                }
            }
            connection.Close();
        }
        return randomWord;
    }

    private List<string> ShuffleList(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            string temp = list[i];
            int randomIndex = UnityEngine.Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
        return list;
    }
}


//DI KO NA KAYA
