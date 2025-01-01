﻿using AspNetCore.Authentication.ApiKey;

namespace Elzik.Breef.Api;

public class EnvironmentApiKeyProvider : IApiKeyProvider
{
    private const string ApiKeyEnvirnomentVariableName = "BREEF_API_KEY";
    private readonly string ApiKey = Environment.GetEnvironmentVariable(ApiKeyEnvirnomentVariableName) 
        ?? throw new InvalidOperationException(
            $"An environment variable named {ApiKeyEnvirnomentVariableName} must be provided.");

    public Task<IApiKey> ProvideAsync(string key)
    {
        if (key == ApiKey)
        {
            return Task.FromResult<IApiKey>(new ApiKey(key));
        }

        return Task.FromResult<IApiKey>(null);
    }
}