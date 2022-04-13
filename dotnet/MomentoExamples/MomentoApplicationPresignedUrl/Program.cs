﻿using System;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MomentoSdk;

namespace MomentoApplicationPresignedUrl
{
    class Program
    {        
        private static readonly string SIGNING_KEY = Environment.GetEnvironmentVariable("SIGNING_KEY");
        private static readonly string ENDPOINT = Environment.GetEnvironmentVariable("ENDPOINT");

        private static readonly uint URL_TTL_MINUTES = 10;
        private static readonly string CACHE_NAME = "cache";
        private static readonly string OBJECT_KEY = "MyKey";
        private static readonly string OBJECT_VALUE = "MyData";
        private static readonly uint OBJECT_TTL_SECONDS = 60 * 60 * 24 * 14; // 14 days

        private static readonly HttpClient client = new HttpClient();

        private static string Base64Encode(string plainText) {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private static async Task RunPresignedUrlExample(Uri setUri, Uri getUri)
        {
            // Currently Envoy does not support using raw bytes as request body with HTTP/2
            // TODO: Remove Base64 encoding once we support REST endpoint with HTTP/1.1 only
            var json = JsonConvert.SerializeObject(Base64Encode(OBJECT_VALUE));
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            Console.WriteLine($"Posting value with signed URL for {CacheOperation.SET}: {OBJECT_VALUE}");
            var setResponse = await client.PostAsync(setUri, data);
            setResponse.EnsureSuccessStatusCode();
            
            var getResponse = await client.GetStringAsync(getUri);
            Console.WriteLine($"Retrieved value with signed URL for {CacheOperation.GET}: {getResponse}");
        }

        static async Task Main(string[] args)
        {
            // prepare requests
            uint expiryEpochSeconds = (uint) DateTimeOffset.UtcNow.AddMinutes(URL_TTL_MINUTES).ToUnixTimeSeconds();
            var setReq = new SigningRequest(CACHE_NAME, OBJECT_KEY, CacheOperation.SET, expiryEpochSeconds) { TtlSeconds = OBJECT_TTL_SECONDS };
            var getReq = new SigningRequest(CACHE_NAME, OBJECT_KEY, CacheOperation.GET, expiryEpochSeconds);
            Console.WriteLine($"Request claims: exp = {expiryEpochSeconds}, cache = {CACHE_NAME}, key = {OBJECT_KEY}, ttl (for set) = {OBJECT_TTL_SECONDS}");

            // create presigned urls
            MomentoSigner signer = new MomentoSigner(SIGNING_KEY);
            var setUrl = signer.CreatePresignedUrl(ENDPOINT, setReq);
            var getUrl = signer.CreatePresignedUrl(ENDPOINT, getReq);

            Uri setUri, getUri;
            Uri.TryCreate(setUrl, UriKind.Absolute, out setUri);
            Console.WriteLine($"Signed URL for {CacheOperation.SET}:\n{setUri}");
            Uri.TryCreate(getUrl, UriKind.Absolute, out getUri);
            Console.WriteLine($"Signed URL for {CacheOperation.GET}:\n{getUri}");

            // set and get using presigned urls
            await RunPresignedUrlExample(setUri, getUri);
        }
    }
}
