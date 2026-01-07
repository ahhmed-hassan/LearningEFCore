using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NullableIntroduction;

internal enum QuestionType
{
    YesNo,
    Text,
    Number
}
internal class SurveyQuestion
{
    public string QuestionText { get; }
    public QuestionType TypeOfQuestion { get; }

    public SurveyQuestion(QuestionType typeOfQuestion, string text) =>
       (TypeOfQuestion, QuestionText) = (typeOfQuestion, text);
}

internal class SurveyRun
{
    private List<SurveyQuestion> _surveyQuestions = new();

    private List<SurveyResponse>? respondents;

    public IEnumerable<SurveyResponse> AllParticipants => (respondents ?? Enumerable.Empty<SurveyResponse>());
    public ICollection<SurveyQuestion> Questions => _surveyQuestions;
    public SurveyQuestion GetQuestion(int index) => _surveyQuestions[index];

    public void AddQuestion(QuestionType questionType, string question) =>
        AddQuestion(new SurveyQuestion(questionType, question));

    public void AddQuestion(SurveyQuestion surveyQuestion) =>
     _surveyQuestions.Add(surveyQuestion);

    public void PerformSurvey(int numberOfRespondents)
    {
        respondents =  Enumerable
            .Repeat(0, int.MaxValue)
            .Select(_ => SurveyResponse.GetRandomId())
            .Where(r => r.AnswerSurvey(_surveyQuestions))
            .Take(numberOfRespondents)
            .ToList();
    }
}
