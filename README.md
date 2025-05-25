# breef


[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=elzik_breef&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=elzik_breef)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=elzik_breef&metric=bugs)](https://sonarcloud.io/summary/new_code?id=elzik_breef)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=elzik_breef&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=elzik_breef)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=elzik_breef&metric=coverage)](https://sonarcloud.io/summary/new_code?id=elzik_breef)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=elzik_breef&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=elzik_breef)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=elzik_breef&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=elzik_breef)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=elzik_breef&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=elzik_breef)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=elzik_breef&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=elzik_breef)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=elzik_breef&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=elzik_breef)

## Introduction

Use LLMs to generate summaries of web content for reading later.

## Configuration

Configuration can be via an appsettings.json file or environment variables. These two configuration schemes can be mixed. If a setting is configured in both appsettings.json and as an environment variable, the environment variable will take precedence.

In the following examples, the JSON comments show the environment variable name which corresponds to the setting in appsettings.json.

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

These config items relate to the AI service used for generating summaries.

- **Provider** - The AI service provider. [`AzureOpenAI`](https://ai.azure.com/), [`OpenAI`](https://platform.openai.com/) and [`Ollama`](https://ollama.com/) are supported. The Microsoft SematicKernel connector for Ollama is in preview and a warning will be logged to make this clear.
- **ModelId** - The model ID to be used for generating summaries. A chat-completion model should be used.
  - AzureOpenAI - This is given as the 'Name' in the 'Deployment info' in Azure.
  - OpenAI - This is given as the 'model' in OpenAI.
  - Ollama - This is given as the 'model' in Ollama and must also include the tag.
- **EndpointUrl** - The endpoint URL for the AI service.
  - AzureOpenAI - This is given as the 'Azure OpenAI endpoint' in Azure and typically takes the form `https://<tenant-specific>.openai.azure.com` It should not include any model-specific routing.
  - OpenAI - Typically `https://api.openai.com/v1` It should not include any model-specific routing.
  - Ollama - By default this is `http://<host>:11434`.
- **ApiKey** - The API key used to authenticate with the AI service. Ollama does not support an API key and should be left blank. If a key is provided for Ollama, a warning will be logged and the key will be ignored.

Example
```jsonc
{
  "BreefApi": {
    "ApiKey": "<wallabag-generaged-key>"                     // breef_BreefApi__ApiKey
  },

  "Wallabag": {
    "BaseUrl": "https://<wallabag-host>",                    // breef_Wallabag__BaseUrl
    "ClientId": "<wallabag-generaged-client-id>",            // breef_Wallabag__ClientId
    "ClientSecret": "<wallabag-generaged-client-secrect>",   // breef_Wallabag__ClientSecret
    "Username": "<wallabag-username>",                       // breef_Wallabag__Username
    "Password": "<wallabag-password>"                        // breef_Wallabag__Password
  },

  "AiService": {
    "Provider": "OpenAi",                                    // breef_AiService__Provider
    "ModelId": "https://api.openai.com/v1",                  // breef_AiService__ModelId
    "EndpointUrl": "gpt-4o-mini",                            // breef_AiService__EndpointUrl
    "ApiKey": "<open-ai-api-key>"                            // breef_AiService__ApiKey
  }
}
```

### Optional

#### AiService

- **TimeOut** - Sets the number of seconds before the AiService used will time out. The default used if not set is 100 seconds. This may need to be increased where Ollama is used with limiting hardware.

Example:

```jsonc
"AiService": {
    "Timeout": 100,                        // breef_AiService__Timeout
}
```

#### AI Content Summariser

The AI model will use these settings when generating summaries. Although the model may not adhere to these settings, they will influence the resulting summary.

- **TargetSummaryLengthPercentage** - Sets the size of the summary with respect to the size of the original article. The default used if not set is 10%.
- **TargetSummaryMaxWordCount** - Sets the maximum number of words for the summary generated. The default used if not set is 200 words.

Example:

```jsonc
"AiContentSummariser": {
    "TargetSummaryLengthPercentage": 10,   // breef_AiContentSummariser__TargetSummaryLengthPercentage
    "TargetSummaryMaxWordCount": 200       // breef_AiContentSummariser__TargetSummaryMaxWordCount
}
```

#### Web Page Downloader

These settings affect how pages are downloaded prior to being summarised.

  - **UserAgent** - The user agent used when downloading pages. By default this is set to `Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36` but can be overridden here.

Example:

```jsonc
"WebPageDownLoader" : {
    "UserAgent": "<custom-agent>"   // breef_WebPageDownLoader__UserAgent
}
```

#### Logging

Logging is handled by Serilog and configuration is documented [here](https://github.com/serilog/serilog-settings-configuration/blob/dev/README.md). This example shows how to change the default log level:

```jsonc
"Serilog": {
  "MinimumLevel": {
    "Default": "Debug"   // breef_Serilog__MinimumLevel__Default
  }
}
```