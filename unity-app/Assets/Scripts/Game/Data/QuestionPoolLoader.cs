using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MouthOfTruth.Game.Data
{
    internal static class QuestionPoolLoader
    {
        internal static IReadOnlyList<QuestionDefinition> loadQuestionDefinitions(QuestionPoolFilePath questionPoolFilePath)
        {
            if (questionPoolFilePath.IsEmpty)
            {
                throw new ArgumentException("Question pool file path cannot be empty.", nameof(questionPoolFilePath));
            }

            if (File.Exists(questionPoolFilePath.Value) == false)
            {
                throw new FileNotFoundException("Question pool JSON file was not found.", questionPoolFilePath.Value);
            }

            string jsonText = File.ReadAllText(questionPoolFilePath.Value);
            QuestionPoolJsonDocument questionPoolJsonDocument = JsonUtility.FromJson<QuestionPoolJsonDocument>(jsonText);

            if (questionPoolJsonDocument == null || questionPoolJsonDocument.questions == null)
            {
                throw new InvalidDataException("Question pool JSON did not contain a questions array.");
            }

            List<QuestionDefinition> questionDefinitions = new List<QuestionDefinition>();

            foreach (QuestionDefinitionJsonRecord questionDefinitionJsonRecord in questionPoolJsonDocument.questions)
            {
                if (string.IsNullOrWhiteSpace(questionDefinitionJsonRecord.id))
                {
                    throw new InvalidDataException("Question definition id cannot be empty.");
                }

                if (string.IsNullOrWhiteSpace(questionDefinitionJsonRecord.text))
                {
                    throw new InvalidDataException($"Question definition text cannot be empty: {questionDefinitionJsonRecord.id}");
                }

                QuestionId questionId = new QuestionId(questionDefinitionJsonRecord.id);
                QuestionText questionText = new QuestionText(questionDefinitionJsonRecord.text);
                QuestionCategory questionCategory = new QuestionCategory(questionDefinitionJsonRecord.category);
                QuestionDifficulty questionDifficulty = new QuestionDifficulty(questionDefinitionJsonRecord.difficulty);
                EQuestionAvailability questionAvailability = questionDefinitionJsonRecord.enabled
                    ? EQuestionAvailability.Enabled
                    : EQuestionAvailability.Disabled;

                questionDefinitions.Add(new QuestionDefinition(questionId, questionText, questionCategory, questionDifficulty, questionAvailability));
            }

            return questionDefinitions;
        }

        [Serializable]
        private sealed class QuestionPoolJsonDocument
        {
            // JsonUtility maps fields by the external question-pool JSON keys.
            public List<QuestionDefinitionJsonRecord> questions = new List<QuestionDefinitionJsonRecord>();
        }

        [Serializable]
        private sealed class QuestionDefinitionJsonRecord
        {
            // JsonUtility maps fields by the external question-pool JSON keys.
            public string id = string.Empty;
            public string text = string.Empty;
            public string category = string.Empty;
            public int difficulty = 0;
            public bool enabled = true;
        }
    }
}
