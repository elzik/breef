# breef

![Coverage](https://gist.githubusercontent.com/elzik/527882e89a938dc78f61a08c300edec4/raw/c93a0d914e1219520529a650f0dac24d809bee53/breef-code-coverage-main.svg)

## Introduction

Use LLMs to generate summaries of web content for reading later.

## Configuration

Configuration can be via an appsettings.json file or environment variables. These two configuration schemes can be mixed. If a setting is configured in both appsettings.json and as an environment variable, the environment variable will take precedence.

### Required

Example
```json
{
  "BreefApi": {
    "ApiKey": ""
  },

  "Wallabag": {
    "BaseUrl": "",
    "ClientId": "",
    "ClientSecret": "",
    "Username": "",
    "Password": ""
  },

  "AiService": {
    "ModelId": "",
    "EndpointUrl": "",
    "ApiKey": ""
  }
}
```

### Optional

#### AI Content Summariser

The AI model will use these settings when generating summaries. Although the model may not adhere to these settings, they will influence the resulting summary.

- **TargetSummaryLengthPercentage** - Sets the size of the summary with respect to the size of the original article.
- **TargetSummaryMaxWordCount** - Sets the maximum number of words for the summary generated.

Example:

```json
"AiContentSummariser": {
    TargetSummaryLengthPercentage: 10,        // breef_AiContentSummariser__TargetSummaryLengthPercentage
    TargetSummaryMaxWordCount: 200            // breef_AiContentSummariser__TargetSummaryMaxWordCount
}
```
