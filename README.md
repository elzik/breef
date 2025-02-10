# breef

![Coverage](https://gist.githubusercontent.com/elzik/527882e89a938dc78f61a08c300edec4/raw/c93a0d914e1219520529a650f0dac24d809bee53/breef-code-coverage-main.svg)


## Introduction

Use LLMs to generate summaries of web content for reading later.

## Configuration

Configuration can be via an appsettings.json file or via environment variables. These two configuration schemes can be mixex. If a setting is configured in both appsettings.json and as an environment variable, the environment variable will take precedence.

### Required

Exmaple
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

These settings will be used by the AI model. Although there is no gurantee the model will adhere to these settings, they are likely to influence the resulting summary.

- **TargetSummaryLengthPercentage** - Sets the size of the summary with respect to the sze of the original article.
- **TargetSummaryMaxWordCount** - Sets the maximum number of words for the summary generated.

Exmaple:

```json
"AiContentSummariser" : {
    TargetSummaryLengthPercentage: 10,        // breef_AiContentSummariser__TargetSummaryLengthPercentage
    TargetSummaryMaxWordCount: 200            // breef_AiContentSummariser__TargetSummaryMaxWordCount
}
```