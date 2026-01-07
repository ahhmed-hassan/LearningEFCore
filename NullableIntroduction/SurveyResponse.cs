namespace NullableIntroduction;

internal class SurveyResponse(int id)
{
    private Dictionary<int, string>? surveyResponses;
    public int Id { get; } = id;

    private static readonly Random randomGenerator = new Random();
    public static SurveyResponse GetRandomId() => new SurveyResponse(randomGenerator.Next());

    public bool AnsweredSurvey => surveyResponses != null;
    public string Answer(int index) => surveyResponses?.GetValueOrDefault(index) ?? "No answer";

    public bool AnswerSurvey(IEnumerable<SurveyQuestion> questions)
    {
        if (ConsentToSurvey())
        {
            surveyResponses = new Dictionary<int, string>();
            int index = 0;
            foreach (var question in questions)
            {
                var answer = GenerateAnswer(question);
                if (answer != null)
                {
                    surveyResponses.Add(index, answer);
                }
                index++;
            }
        }
        return true;
    }

    private string? GenerateAnswer(SurveyQuestion question)
    {
        switch (question.TypeOfQuestion)
        {
            case QuestionType.YesNo:
                int n = randomGenerator.Next(-1, 2);
                return (n == -1) ? default : (n == 0) ? "No" : "Yes";

            case QuestionType.Number:
                n = randomGenerator.Next(-30, 101);
                return (n < 0) ? default : n.ToString();

            case QuestionType.Text:
            default:
                switch (randomGenerator.Next(0, 5))
                {
                    case 0:
                        return default;
                    case 1:
                        return "Red";
                    case 2:
                        return "Green";
                    case 3:
                        return "Blue";
                }
                return "Red. No, Green. Wait.. Blue... AAARGGGGGHHH!";
        }
    }

    private bool ConsentToSurvey() => randomGenerator.Next(0, 2) == 1;
}