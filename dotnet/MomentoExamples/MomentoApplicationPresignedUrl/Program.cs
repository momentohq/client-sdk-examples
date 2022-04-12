using System;
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
        private static readonly string KEY = "MyKey";
        private static readonly string VALUE = "MyData";
        private static readonly uint OBJECT_TTL_SECONDS = 60 * 60 * 24 * 14; // 14 days

        private static readonly HttpClient client = new HttpClient();

        private static string Base64Encode(string plainText) {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private static async Task RunPresignedUrlExample(string setUrl, string getUrl)
        {
            Console.WriteLine($"Posting value with signed URL for {CacheOperation.SET}: {VALUE}");
            var json = JsonConvert.SerializeObject(Base64Encode(VALUE));
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var setResponse = await client.PostAsync(setUrl, data);
            setResponse.EnsureSuccessStatusCode();
            
            var getResponse = await client.GetStringAsync(getUrl);
            Console.WriteLine($"Lookedup value with signed URL for {CacheOperation.GET}: {getResponse}");
        }

        static async Task Main(string[] args)
        {
            // prepare requests
            uint expiryEpochSeconds = (uint) DateTimeOffset.UtcNow.AddMinutes(URL_TTL_MINUTES).ToUnixTimeSeconds();
            var setReq = new SigningRequest(CACHE_NAME, KEY, CacheOperation.SET, expiryEpochSeconds)
            {
                TtlSeconds = OBJECT_TTL_SECONDS
            };
            var getReq = new SigningRequest(CACHE_NAME, KEY, CacheOperation.GET, expiryEpochSeconds);
            Console.WriteLine($"Request claims: cache = {CACHE_NAME}, key = {KEY}, exp = {expiryEpochSeconds}, ttl (for set) = {OBJECT_TTL_SECONDS}");

            // create presigned urls
            MomentoSigner signer = new MomentoSigner(SIGNING_KEY);
            var setUrl = signer.CreatePresignedUrl(ENDPOINT, setReq);
            var getUrl = signer.CreatePresignedUrl(ENDPOINT, getReq);

            Uri uriResult;
            bool result = Uri.TryCreate(setUrl, UriKind.Absolute, out uriResult);
            if (result) {
                Console.WriteLine($"Pre-signed URL for {CacheOperation.SET}:\n{setUrl}");
            }
            result = Uri.TryCreate(getUrl, UriKind.Absolute, out uriResult);
            if (result) {
                Console.WriteLine($"Pre-signed URL for {CacheOperation.GET}:\n{getUrl}");
            }

            // set and get using presigned urls
            await RunPresignedUrlExample(setUrl, getUrl);
        }
    }
}
