using Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client;
using Refit;
using Shouldly;

namespace Elzik.Breef.Infrastructure.Tests.Integration.ContentExtractors.Reddit.Client;

public class RedditPostClientTests
{
    private static bool IsRunningInGitHubWorkflow => Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";

    [SkippableFact]
    public async Task GetPost_ValidPostId_ReturnsRedditPost()
    {
        // Arrange
        Skip.If(IsRunningInGitHubWorkflow, "Skipped because requests to reddit.com from GitHub workflows are " +
            "always blocked meaning this test case always fails. This must be run locally instead.");
        var client = RestService.For<IRedditPostClient>("https://www.reddit.com/");
        var postId = "1kqiwzc"; // https://www.reddit.com/r/learnprogramming/comments/1kqiwzc

        // Act
        var redditPost = await client.GetPost(postId);

        // Assert
        redditPost.ShouldNotBeNull();
        redditPost.Count.ShouldBe(2, "a reddit post is made up of two listings: one for the main post and one for the replies");
        redditPost[0].Data.ShouldNotBeNull();
        redditPost[0].Data.Children.ShouldNotBeNull();
        redditPost[0].Data.Children.Count.ShouldBe(1, "there is only a single main post");
        redditPost[0].Data.Children[0].Kind.ShouldBe("t3", "t3 represents the type of main post");
        redditPost[0].Data.Children[0].Data.ShouldNotBeNull();

        var mainPost = redditPost[0].Data.Children[0].Data;
        mainPost.Id.ShouldBe("1kqiwzc");
        mainPost.Author.ShouldBe("melvman1");
        mainPost.CreatedUtc.ShouldBe(DateTime.Parse("2025-05-19T18:18:05"));
        mainPost.SelfText.ShouldBe("I am just about to enter the programming world, and want to become a software " +
            "engineer. This work ready college in Sweden has a 2 year long .net developer program with internships " +
            "at real companies. They also have a similar program but with javascript.\n\nI am wondering if this " +
            "would be a good path if my dream is to become a freelancer and I want to build easy apps / websites for " +
            "small startups in Sweden/worldwide.\n\nThis is the program:\n\nProgramming C# – 12 weeks\n\nDevelopment " +
            "against database and database administration – 9 weeks\n\nWeb development with .NET – 12 weeks\n\nAgile " +
            "development – 6 weeks\n\nCustomer understanding, consulting and reporting – 3 weeks\n\nApprenticeship " +
            "at companies – 12 weeks\n\nClean code – 6 weeks\n\nApprenticeship at companies – 16 weeks\n\nExam " +
            "thesis – 4 weeks");
        mainPost.Content.ShouldBe(mainPost.SelfText);

        var replies = redditPost[1].Data.Children;

        replies.Count.ShouldBe(5);

        // First reply
        replies[0].Kind.ShouldBe("t1");
        replies[0].Data.Id.ShouldBe("mt7aaf6");
        replies[0].Data.Author.ShouldBe("CodeRadDesign");
        replies[0].Data.Body.ShouldBe(
            "not really.\n\nas someone who's been freelance on and off for 30 years, you're looking for a more " +
            "rounded skill set.  \n\nyou're not going to compete with 'people from third world countries' like the " +
            "other poster mentioned; you just can't. so you have to ask yourself, what do people in my area actually " +
            "need. \n\nif the answer is (and it probably is) websites for their local businesses, then you want a mix " +
            "of graphic art, html/css/js, a frontend tech like react or vue, and a backend tech. that could be C#.net" +
            ", that could by python, lots of options.\n\nC# is definitely in demand, but not so much in freelance. " +
            "for the most part a C#.net core specialist is going to be part of a team, at a company, and you'll defo " +
            "want that college paper for that.  if you're only planning on freelance, you can realistically just self " +
            "learn. if you don't think you can handle the unstructuredness of self-learning..... you're going to hate " +
            "freelancing. \n\notherwise looks like a fine program, i would likely favor taking something like that " +
            "and planning on getting a Real Job though haha.\n\n*regarding your last point on your other comment \"" +
            "c# looks easy to learn\" is not really a valid criteria. your first language is going to be the hardest" +
            ", your second language will be ten times easier. c# is a good foundational language tho, i'd recommend " +
            "it over python because it teaches a lot of good habits early."
        );

        // Second reply
        replies[1].Kind.ShouldBe("t1");
        replies[1].Data.Id.ShouldBe("mt7lqgx");
        replies[1].Data.Author.ShouldBe("No_Researcher_7875");
        replies[1].Data.Body.ShouldBe(
            "As mentioned before it will be hard to compete with the experts but i think you are not thinking this " +
            "correctly.\n\n If you want to build sites, is not that important in wich language you code them but how " +
            "good and fast can you build them. \n\nThis program is a good start, and if you choose the js one would " +
            "be a little better mostly for the front end part.\n\nAnyways chose whatever program you like the most " +
            "and code, code a lot and you will be able to do what you want."
        );

        // Third reply (has nested reply)
        replies[2].Kind.ShouldBe("t1");
        replies[2].Data.Id.ShouldBe("mt606l6");
        replies[2].Data.Author.ShouldBe("[deleted]");
        replies[2].Data.Body.ShouldBe("[deleted]");

        // Fourth reply
        replies[3].Kind.ShouldBe("t1");
        replies[3].Data.Id.ShouldBe("mt83c0a");
        replies[3].Data.Author.ShouldBe("goqsane");
        replies[3].Data.Body.ShouldBe("No it’s not.");

        // Fifth reply
        replies[4].Kind.ShouldBe("t1");
        replies[4].Data.Id.ShouldBe("mt9gc9x");
        replies[4].Data.Author.ShouldBe("ToThePillory");
        replies[4].Data.Body.ShouldBe(
            "I got most of my freelancing work in C#, that and Java.\n\nThe problem is that you're a beginner, and " +
            "freelancing doesn't really suit beginners, or even decent juniors.\n\nFreelancing means every single " +
            "problem you encounter is 100% your responsibility to fix. There is no team to bounce ideas off, there " +
            "is no manager to talk a client out of an idea, there is nobody other than you to solve \\*all\\* " +
            "problems.\n\nI would aim to get a regular programming job first, freelancing is not easy, and generally " +
            "pays less than a normal job."
        );

        // Nested reply to third reply
        var nestedReplies = replies[2].Data.Replies.Data.Children;
        nestedReplies.Count.ShouldBe(1);
        nestedReplies[0].Data.Id.ShouldBe("mt60jnv");
        nestedReplies[0].Data.Author.ShouldBe("melvman1");
        nestedReplies[0].Data.Body.ShouldBe(
            "I am willing to work at the company i do my apprenticeship at for a couple years to learn, but is this " +
            "program a good start for my career if that is my ”long term” goal? :)"
        );
    }
}