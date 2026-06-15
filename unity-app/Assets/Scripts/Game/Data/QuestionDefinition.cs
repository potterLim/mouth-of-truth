using System;

namespace MouthOfTruth.Game.Data
{
    [Serializable]
    public class QuestionDefinition
    {
        public QuestionDefinition(QuestionId id, QuestionText text, QuestionCategory category, QuestionDifficulty difficulty, EQuestionAvailability availability)
        {
            Id = id;
            Text = text;
            Category = category;
            Difficulty = difficulty;
            Availability = availability;
        }

        public QuestionId Id
        {
            get;
        }

        public QuestionText Text
        {
            get;
        }

        public QuestionCategory Category
        {
            get;
        }

        public QuestionDifficulty Difficulty
        {
            get;
        }

        public EQuestionAvailability Availability
        {
            get;
        }

        public bool IsEnabled => Availability == EQuestionAvailability.Enabled;
    }
}
