// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using InteractiveKernel = Microsoft.DotNet.Interactive.Kernel;

// ReSharper disable InconsistentNaming

public static class Settings
{
    private const string DefaultConfigFile = "../config/settings.json";
    private const string TypeKey = "type";
    private const string ModelKey = "model";
    private const string EndpointKey = "endpoint";
    private const string SecretKey = "apikey";
    private const string OrgKey = "org";

    private const string AmadeusClientID = "amadeusClientID";
    private const string AmadeusSecret = "amadeusSecret";

    private const string TomorrowioSecretKey = "tomorrowio";
    private const bool StoreConfigOnFile = true;

    // Prompt user for Azure Endpoint URL
    public static async Task<string> AskAzureEndpoint(bool _useAzureOpenAI = true, string configFile = DefaultConfigFile)
    {
        var (useAzureOpenAI, model, azureEndpoint, apiKey, orgId, tomorrowioApiKey, amadeusClientID, amadeusSecret  ) = ReadSettings(_useAzureOpenAI, configFile);

        // If needed prompt user for Azure endpoint
        if (useAzureOpenAI && string.IsNullOrWhiteSpace(azureEndpoint))
        {
            azureEndpoint = await InteractiveKernel.GetInputAsync("Please enter your Azure OpenAI endpoint");
        }

        WriteSettings(configFile, useAzureOpenAI, model, azureEndpoint, apiKey, orgId,  tomorrowioApiKey, amadeusClientID, amadeusSecret );

        // Print report
        if (useAzureOpenAI)
        {
            Console.WriteLine("Settings: " + (string.IsNullOrWhiteSpace(azureEndpoint)
                ? "ERROR: Azure OpenAI endpoint is empty"
                : $"OK: Azure OpenAI endpoint configured [{configFile}]"));
        }

        return azureEndpoint;
    }

    // Prompt user for OpenAI model name / Azure OpenAI deployment name
    public static async Task<string> AskModel(bool _useAzureOpenAI = true, string configFile = DefaultConfigFile)
    {
        var (useAzureOpenAI, model, azureEndpoint, apiKey, orgId,  tomorrowioApiKey, amadeusClientID, amadeusSecret ) = ReadSettings(_useAzureOpenAI, configFile);

        // If needed prompt user for model name / deployment name
        if (string.IsNullOrWhiteSpace(model))
        {
            if (useAzureOpenAI)
            {
                model = await InteractiveKernel.GetInputAsync("Please enter your Azure OpenAI deployment name");
            }
            else
            {
                // Use the best model by default, and reduce the setup friction, particularly in VS Studio.
                model = "gpt-3.5-turbo";
            }
        }

        WriteSettings(configFile, useAzureOpenAI, model, azureEndpoint, apiKey, orgId,  tomorrowioApiKey, amadeusClientID, amadeusSecret );

        // Print report
        if (useAzureOpenAI)
        {
            Console.WriteLine("Settings: " + (string.IsNullOrWhiteSpace(model)
                ? "ERROR: deployment name is empty"
                : $"OK: deployment name configured [{configFile}]"));
        }
        else
        {
            Console.WriteLine("Settings: " + (string.IsNullOrWhiteSpace(model)
                ? "ERROR: model name is empty"
                : $"OK: AI model configured [{configFile}]"));
        }

        return model;
    }

    // Prompt user for API Key
    public static async Task<string> AskApiKey(bool _useAzureOpenAI = true, string configFile = DefaultConfigFile)
    {
        var (useAzureOpenAI, model, azureEndpoint, apiKey, orgId,  tomorrowioApiKey, amadeusClientID, amadeusSecret ) = ReadSettings(_useAzureOpenAI, configFile);

        // If needed prompt user for API key
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            if (useAzureOpenAI)
            {
                var pw = await InteractiveKernel.GetPasswordAsync("Please enter your Azure OpenAI API key");
                apiKey = pw.GetClearTextPassword();
                orgId = "";
            }
            else
            {
                var pw = await InteractiveKernel.GetPasswordAsync("Please enter your OpenAI API key");
                apiKey = pw.GetClearTextPassword();
            }
        }

        WriteSettings(configFile, useAzureOpenAI, model, azureEndpoint, apiKey, orgId,  tomorrowioApiKey, amadeusClientID, amadeusSecret );

        // Print report
        Console.WriteLine("Settings: " + (string.IsNullOrWhiteSpace(apiKey)
            ? "ERROR: API key is empty"
            : $"OK: API key configured [{configFile}]"));

        return apiKey;
    }

    public static async Task<string> AskTomorrowioApiKey(bool _useAzureOpenAI = true, string configFile = DefaultConfigFile)
    {
        var (useAzureOpenAI, model, azureEndpoint, apiKey, orgId,  tomorrowioApiKey, amadeusClientID, amadeusSecret ) = ReadSettings(_useAzureOpenAI, configFile);

        // If needed prompt user for API key
        if (string.IsNullOrWhiteSpace(tomorrowioApiKey))
        {
            if (useAzureOpenAI)
            {
                var pw = await InteractiveKernel.GetPasswordAsync("Please enter your tomorrow.io API key");
                tomorrowioApiKey = pw.GetClearTextPassword();
                orgId = "";
            }
            else
            {
                var pw = await InteractiveKernel.GetPasswordAsync("Please enter your tomorrow.io API key");
                tomorrowioApiKey = pw.GetClearTextPassword();
            }
        }

        WriteSettings(configFile, useAzureOpenAI, model, azureEndpoint, apiKey, orgId,  tomorrowioApiKey, amadeusClientID, amadeusSecret );

        // Print report
        Console.WriteLine("Settings: " + (string.IsNullOrWhiteSpace(apiKey)
            ? "ERROR: tomorrow.io API key is empty"
            : $"OK: tomorrow.io API key configured [{configFile}]"));

        return apiKey;
    }

    // Prompt user for OpenAI Organization Id
    public static async Task<string> AskOrg(bool _useAzureOpenAI = true, string configFile = DefaultConfigFile)
    {
        var (useAzureOpenAI, model, azureEndpoint, apiKey, orgId,  tomorrowioApiKey, amadeusClientID, amadeusSecret ) = ReadSettings(_useAzureOpenAI, configFile);

        // If needed prompt user for OpenAI Org Id
        if (!useAzureOpenAI && string.IsNullOrWhiteSpace(orgId))
        {
            orgId = await InteractiveKernel.GetInputAsync("Please enter your OpenAI Organization Id (enter 'NONE' to skip)");
        }

        WriteSettings(configFile, useAzureOpenAI, model, azureEndpoint, apiKey, orgId,  tomorrowioApiKey, amadeusClientID, amadeusSecret );

        return orgId;
    }

    // Load settings from file
    public static (bool useAzureOpenAI, string model, string azureEndpoint, string apiKey, string orgId, string tomorrowioApiKey, string amadeusClientID, string amadeusSecret )
        LoadFromFile(string configFile = DefaultConfigFile)
    {
        if (!File.Exists(configFile))
        {
            Console.WriteLine("Configuration not found: " + configFile);
            Console.WriteLine("\nPlease run the Setup Notebook (0-AI-settings.ipynb) to configure your AI backend first.\n");
            throw new Exception("Configuration not found, please setup the notebooks first using notebook 0-AI-settings.pynb");
        }

        try
        {
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(configFile));
            bool useAzureOpenAI = config[TypeKey] == "azure";
            string model = config[ModelKey];
            string azureEndpoint = config[EndpointKey];
            string apiKey = config[SecretKey];
            string orgId = config[OrgKey];
            string tomorrowioApiKey = config[TomorrowioSecretKey];
            string amadeusClientID = config[AmadeusClientID];
            string amadeusSecret = config[AmadeusSecret];

            if (orgId == "none") { orgId = ""; }

            return (useAzureOpenAI, model, azureEndpoint, apiKey, orgId, tomorrowioApiKey, amadeusClientID, amadeusSecret );
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong: " + e.Message);
            return (true, "", "", "", "", "","", "");
        }
    }

    // Delete settings file
    public static void Reset(string configFile = DefaultConfigFile)
    {
        if (!File.Exists(configFile)) { return; }

        try
        {
            File.Delete(configFile);
            Console.WriteLine("Settings deleted. Run the notebook again to configure your AI backend.");
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong: " + e.Message);
        }
    }

    // Read and return settings from file
    private static (bool useAzureOpenAI, string model, string azureEndpoint, string apiKey, string orgId, string tomorrowioApiKey, string amadeusClientID, string amadeusSecret )
        ReadSettings(bool _useAzureOpenAI, string configFile)
    {
        // Save the preference set in the notebook
        bool useAzureOpenAI = _useAzureOpenAI;
        string model = "";
        string azureEndpoint = "";
        string apiKey = "";
        string orgId = "";
        string tomorrowioApiKey = "";
        string amadeusClientID = "";
        string amadeusSecret = "";

        try
        {
            if (File.Exists(configFile))
            {
                (useAzureOpenAI, model, azureEndpoint, apiKey, orgId, tomorrowioApiKey, amadeusClientID, amadeusSecret ) = LoadFromFile(configFile);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong: " + e.Message);
        }

        // If the preference in the notebook is different from the value on file, then reset
        if (useAzureOpenAI != _useAzureOpenAI)
        {
            Reset(configFile);
            useAzureOpenAI = _useAzureOpenAI;
            model = "";
            azureEndpoint = "";
            apiKey = "";
            orgId = "";
            tomorrowioApiKey = "";
            amadeusClientID = "";
            amadeusSecret = "";
            
        }

        return (useAzureOpenAI, model, azureEndpoint, apiKey, orgId, tomorrowioApiKey, amadeusClientID, amadeusSecret );
    }

    // Write settings to file
    private static void WriteSettings(
        string configFile, bool useAzureOpenAI, string model, string azureEndpoint, string apiKey, string orgId, string tomorrowioApiKey, string amadeusClientID, string amadeusSecret)
    {
        try
        {
            if (StoreConfigOnFile)
            {
                var data = new Dictionary<string, string>
                {
                    { TypeKey, useAzureOpenAI ? "azure" : "openai" },
                    { ModelKey, model },
                    { EndpointKey, azureEndpoint },
                    { SecretKey, apiKey },
                    { OrgKey, orgId },
                    { TomorrowioSecretKey, tomorrowioApiKey},
                    { AmadeusClientID, amadeusClientID},
                    { AmadeusSecret, amadeusSecret}
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(configFile, JsonSerializer.Serialize(data, options));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong: " + e.Message);
        }

        // If asked then delete the credentials stored on disk
        if (!StoreConfigOnFile && File.Exists(configFile))
        {
            try
            {
                File.Delete(configFile);
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong: " + e.Message);
            }
        }
    }

    public static async Task<string> ConnectOAuth(string client, string secret)
    {
        // create an HTTP client with base address "https://test.api.amadeus.com"
        var http = new HttpClient { BaseAddress = new Uri("https://test.api.amadeus.com") };

        var message = new HttpRequestMessage(HttpMethod.Post, "/v1/security/oauth2/token");
        message.Content = new StringContent(
            $"grant_type=client_credentials&client_id={client}&client_secret={secret}",
            Encoding.UTF8, "application/x-www-form-urlencoded"
        );

        var results = await http.SendAsync(message);
        await using var stream = await results.Content.ReadAsStreamAsync();
        var oauthResults = await JsonSerializer.DeserializeAsync<OAuthResults>(stream);

        return oauthResults.access_token;
    }

    private class OAuthResults
    {
        public string access_token { get; set; }
    }
}
