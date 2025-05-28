using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client
{
    public interface IRedditPostClient
    {
        [Get("/comments/{postId}.json")]
        [Headers("User-Agent: breef/1.0.0 (https://github.com/elzik/breef)")]
        Task<RedditPost> GetPost(string postId);
    }
}
