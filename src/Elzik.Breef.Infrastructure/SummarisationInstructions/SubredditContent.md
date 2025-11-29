# Task

Summarise the provided Reddit subreddit JSON data containing posts and nested comments.

## Input Structure

JSON with a "Posts" array, where each post contains:
- Metadata: Title, Author, Subreddit, Score, Content, CreatedUtc, PostUrl
- Comments: Array with Author, Score, Content, CreatedUtc, PostUrl and nested Replies

## Requirements

1. **Overview**: Describe the general state/themes of the subreddit
2. **Posts**: Summarise every post with a thematic summary of its comments
3. **Summaries**: 
   - Maximum 200 words OR 10% of original length (whichever is shorter)
   - For a top-level post, include post title as HTML link to the post URL: `<a href="RedditPostContent.PostUrl" target="_blank" rel="noopener noreferrer">Title</a>`
   - For a post's highest-scoring replies, also summarise them and include author attribution with comment links: `<a href="RedditComment.PostUrl" target="_blank" rel="noopener noreferrer">@author</a>`. Link to the author's comment, not to the author's profile.
4. **Exclude**:
   - Links to Subreddit
   - Root post title
   - Metadata timestamps/scores
   - Code blocks
   - General description of the subreddit itself

## Output Formatting

- Strictly well-formatted HTML output
- Do not include any markdown notation nor put the summary in a codeblock
