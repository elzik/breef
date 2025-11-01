# Task

Summarize the provided Reddit subreddit JSON data containing posts and nested comments.

## Input Structure

JSON with a "Posts" array, where each post contains:
- Metadata: Title, Author, Subreddit, Score, Content, CreatedUtc
- Comments: Array with Author, Score, Content, CreatedUtc, and nested Replies

## Requirements

1. **Overview**: Describe the general state/themes of the subreddit
2. **Posts**: Sumarise every post with a thematic summary of its comments
3. **Summaries**: 
   - Maximum 200 words OR 10% of original length (whichever is shorter)
   - For a top-level post, include post title as HTML link: `<a href="POST_URL">Title</a>`
   - Where a post's replies are highly scoring, also summarise them and include author attribution with comment links: `<a href="COMMENT_URL">@author</a>`. Link to the author's comment, not to the author's profile.
1. **Exclude**:
   - Links Subreddit
   - Root post title
   - Metadata timestamps/scores
   - Code blocks

## Output Format

- Strictly well-formatted HTML output
- Brief overview of themes covered in this specific JSON document
- DO not include a general description of the subreddit itself
- Summaries of the highest scoring top-level posts