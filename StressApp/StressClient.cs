﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace StressApp;

internal class StressClient
{
    public StressClient(HttpClient client)
    {
        this.Client = client;
    }

    public HttpClient Client { get; }

    public async Task<bool> GetPage(string relativeAddress)
    {
        bool result;
        try
        {
            var response = await Client.GetAsync(relativeAddress);
            var content = await response.Content.ReadAsStringAsync();
            result = response.IsSuccessStatusCode;
        }
        catch (HttpRequestException err)
        {
            Debug.WriteLine($"Failed with status {err.StatusCode}");
            result = false;
        }
        catch (Exception)
        {
            result = false;
        }


        return result;
    }

    /// <summary>
    /// Unless the server always fails, this call will always succeed
    /// because Polly retries for several time when it fails.
    /// </summary>
    public async Task<bool> Post(string relativeAddress, string data)
    {
        bool result;
        try
        {
            //new StringContent($"\"{data}\"", Encoding.UTF8, "application/json"));
            using var content = JsonContent.Create(data);
            using var response = await Client.PostAsync(relativeAddress, content);
            result = response.IsSuccessStatusCode;
        }
        catch (HttpRequestException err)
        {
            Debug.WriteLine($"Failed with status {err.StatusCode}");
            result = false;
        }
        catch (Exception)
        {
            result = false;
        }

        return result;
    }


}

