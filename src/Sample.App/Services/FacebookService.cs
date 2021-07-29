using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sample.App.Services
{
    public class FacebookService
    {
        public FacebookService(IOptionsMonitor<FacebookServiceOptions> facebookOptionsMonitor)
        {
            facebookOptions = facebookOptionsMonitor.CurrentValue ?? throw new Exception();
        }

        public FacebookSig ParseSignedRequest(string signedRequest)
        {
            var tokens = signedRequest.Split('.');

            var encodedSig = tokens[0];
            var payload = tokens[1];

            var sig = Convert.FromBase64String(FixBase64String(encodedSig));
            var data = JsonSerializer.Deserialize<FacebookSig>(DecodeBase64Url(payload));

            var expectedSig = hmacSHA256(DecodeBase64Url(payload), facebookOptions.AppSecret);

            // Verify data
            if (sig.SequenceEqual(expectedSig))
            {
                throw new Exception("Bad signed JSON signature!");
            }

            return data;
        }

        private string DecodeBase64Url(string input)
        {
            var inputValue = FixBase64String(input.Trim());
            return DecodeBase64(inputValue);
        }

        private string DecodeBase64(string input)
        {
            var bytes = Convert.FromBase64String(input);
            return DecodeBase64(bytes);
        }

        private string DecodeBase64(byte[] input)
        {
            var result = Encoding.UTF8.GetString(input);
            return result;
        }

        private byte[] hmacSHA256(string data, string key)
        {
            byte[] result = null;
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                result = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            }

            return result;
        }

        private static string FixBase64String(string str)
        {
            while (str.Length % 4 != 0)
            {
                str = str.PadRight(str.Length + 1, '=');
            }
            return str.Replace("-", "+").Replace("_", "/");
        }


        private readonly FacebookServiceOptions facebookOptions;
    }

   

    public class FacebookSig
    {
        public string Algorithm { get; set; }
        public int Expires { get; set; }
        [JsonPropertyName("issued_at")]
        public int IssuedAt { get; set; }
        [JsonPropertyName("user_id")]
        public string UserId { get; set; }
    }
}
