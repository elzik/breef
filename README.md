# breef

![Coverage](https://gist.githubusercontent.com/elzik/527882e89a938dc78f61a08c300edec4/raw/c93a0d914e1219520529a650f0dac24d809bee53/breef-code-coverage-main.svg)

## Introduction

Use LLMs to generate summaries of web content for reading later.

## Configuration

Configuration can be via an appsettings.json file or environment variables. These two configuration schemes can be mixed. If a setting is configured in both appsettings.json and as an environment variable, the environment variable will take precedence.

### Required

#### BreefApi

These config items relate to the BreefApi service itself.

- **ApiKey** - The API key used to authenticate with the breef API service. This is required for all requests to the API.

#### Wallabag

These config items relate to the Wallabag service. Refer to the [Wallabag documentation](https://doc.wallabag.org/developer/api/oauth/#creating-a-new-api-client) for more information on setting up breef as an API client.

- **BaseUrl** - The base URL for the Wallabag instance.
- **ClientId** - The Wallabag client ID for breef.
- **ClientSecret** - The Wallabag client secret for breef.
- **Username** - The Wallabag username to be used in conjunction with the breef client.
- **Password** - The Wallabag password to be used in conjunction with the breef client.

#### AiService

These config items relate to the AI service used for generating summaries. Currently, only Azure OpenAI is supported.

- **ModelId** - The model ID to be used for generating summaries.
- **EndpointUrl** - The endpoint URL for the AI service.
- **ApiKey** - The API key used to authenticate with the AI service.

Example
```jsonc
{
  "BreefApi": {
    "ApiKey": ""               // breef_BreefApi__ApiKey
  },

  "Wallabag": {
    "BaseUrl": "",             // breef_Wallabag__BaseUrl
    "ClientId": "",            // breef_Wallabag__ClientId
    "ClientSecret": "",        // breef_Wallabag__ClientSecret
    "Username": "",            // breef_Wallabag__Username
    "Password": ""             // breef_Wallabag__Password
  },

  "AiService": {
    "ModelId": "",             // breef_AiService__ModelId
    "EndpointUrl": "",         // breef_AiService__EndpointUrl
    "ApiKey": ""               // breef_AiService__ApiKey
  }
}
```

### Optional

#### AI Content Summariser

The AI model will use these settings when generating summaries. Although the model may not adhere to these settings, they will influence the resulting summary.

- **TargetSummaryLengthPercentage** - Sets the size of the summary with respect to the size of the original article.
- **TargetSummaryMaxWordCount** - Sets the maximum number of words for the summary generated.

Example:

```jsonc
"AiContentSummariser": {
    "TargetSummaryLengthPercentage": 10,        // breef_AiContentSummariser__TargetSummaryLengthPercentage
    "TargetSummaryMaxWordCount": 200            // breef_AiContentSummariser__TargetSummaryMaxWordCount
}
```

#### Web Page Downloader

These settings affect how pages are downloaded prior to being summarised.

  - **UserAgent** - The user agent used when downloading pages. By default this is set to `Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36` but can be overridden here.

Example:

```jsonc
"WebPageDownLoader" : {
    "UserAgent": "custom agent"        // breef_WebPageDownLoader__UserAgent
}
```
