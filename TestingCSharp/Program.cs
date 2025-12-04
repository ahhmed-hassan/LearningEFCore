// See https://aka.ms/new-console-template for more information

using System;

using System.Collections.Generic;

using System.Linq;

//var coreData = new DocumentData("1", "Doc", "Alice", "Hello world");
//var draft = new Draft(coreData);
//// This won't compile - Draft doesn't have a Publish method!
////draft.Publish                     COMPILE Error! 
//var underReview = draft.Submit("Bob", "Carol");
//underReview.AddComment("Bob", "Looks good!");
//var published = underReview.Publish();

//// Shared immutable data (owned by each document type)

namespace DocumentWorkflow;
public record DocumentData(string Id, string Title, string Author, string Content);

// ---------------- Draft ----------------
public record Draft
{
    public DocumentData Data { get; }
    public int Version { get; }

    public Draft(DocumentData data, int version = 1)
        => (Data, Version) = (data, version);

    public Draft WithEdit(string newContent) =>
        new(Data with { Content = newContent }, Version + 1);

    public UnderReview Submit(params string[] reviewers) =>
        new(Data, reviewers.ToList());    // Only legal way to get UnderReview
}
public record ReviewerComment(string reviewer, string comment);

// ---------------- Under Review ----------------
public record UnderReview
{
    internal UnderReview(DocumentData data, List<string> reviewers)
        => (Data, Reviewers) = (data, reviewers);

    public DocumentData Data { get; }
    public List<string> Reviewers { get; }
    public List<ReviewerComment> ReviewerComments { get; } = new();

    public UnderReview AddComment(string r, string c)
    {
        ReviewerComments.Add(new (r, c)); return this;
    }

    public Draft SendBack() => new(Data);

    public Published Publish() =>
        new(Data, DateTime.UtcNow, Guid.NewGuid().ToString());
}

// ---------------- Published ----------------
public record Published
{
    internal Published(DocumentData data, DateTime date, string doi)
        => (Data, Date, DOI) = (data, date, doi);

    public DocumentData Data { get; }
    public DateTime Date { get; }
    public string DOI { get; }
    public int ViewCount { get; private set; }
    public void View() => ViewCount++;
}

// ---------------- Example ----------------
