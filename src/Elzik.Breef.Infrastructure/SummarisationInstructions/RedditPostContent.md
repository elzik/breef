# Task

Summarise the provided Reddit post JSON data containing a single post and its nested comments.

## Input Structure

JSON with a top-level "Post" item containing:
- Metadata: Title, Author, Subreddit, Score, Content, CreatedUtc, PostUrl
- Comments: Array with Author, Score, Content, CreatedUtc, PostUrl and nested Replies

## Requirements

1. **Overview**: Describe the general state/themes of the Reddit post
2. **Top Level Post**: Summarise the top-level post in detail
3. **Replies**: Summarise only the highest-scoring replies in lower detail
4. **Summaries**: 
   - Maximum 200 words OR 10% of original length (whichever is shorter)
   - For a top-level post, include post title as HTML link to the post URL: `<a href="RedditPostContent.PostUrl" target="_blank" rel="noopener noreferrer">Title</a>`
   - For a post's highest-scoring replies, also summarise them and include author attribution with comment links: `<a href="RedditComment.PostUrl" target="_blank" rel="noopener noreferrer">@author</a>`. Link to the author's comment, not to the author's profile.
5. **Exclude**:
   - Links to Subreddit
   - Root post title
   - Metadata timestamps/scores
   - Code blocks

## Output Format

- Strictly well-formatted HTML output
- Do not include any markdown notation nor put the summary in a codeblock
- Brief overview of themes covered in this specific JSON document
- Do not include a general description of the subreddit itself
- Summaries of the top-level post and highest-scoring replies