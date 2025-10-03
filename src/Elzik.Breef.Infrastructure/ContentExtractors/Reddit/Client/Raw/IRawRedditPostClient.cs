using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elzik.Breef.Infrastructure.ContentExtractors.Reddit.Client.Raw
{
    public interface IRawRedditPostClient
    {
        [Get("/comments/{postId}.json")]
        [Headers("User-Agent: breef/1.0.0 (https://github.com/elzik/breef)")]
        Task<RawRedditPost> GetPost(string postId);
    }
}
