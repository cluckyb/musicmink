using MusicMinkAppLayer.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace MusicMinkAppLayer.Helpers
{
    public enum LastfmStatusCode
    {
        Success,
        RetryableFailure,
        Failure,
    }

    /// <summary>
    /// Helper for interfacing with LastFM
    /// </summary>
    public class LastFMManager
    {
        private const string API_KEY = ApiKeys.LASTFM_API_KEY;
        private const string API_SECRET = ApiKeys.LASTFM_API_SECRET;
        private const string API_PATH = "https://ws.audioscrobbler.com/2.0/";

        private HttpClient LocalClient = new HttpClient();

        private bool Initalized = false;

        private static LastFMManager _current;
        public static LastFMManager Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new LastFMManager();
                }

                return _current;
            }
        }

        private LastFMManager() { }

        private async Task<JObject> getInfo(string method, Dictionary<string, string> parameters, bool isSigned = false)
        {
            if (!Initalized)
            {
                LocalClient.BaseAddress = new Uri(API_PATH);
                LocalClient.DefaultRequestHeaders.Accept.Clear();
                LocalClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Initalized = true;
            }

            parameters.Add("api_key", API_KEY);
            parameters.Add("method", method);

            if (isSigned)
            {
                string api_sig = GenerateSignature(parameters);

                parameters.Add("api_sig", api_sig);
            }

            parameters.Add("format", "JSON");

            HttpContent content = new FormUrlEncodedContent(parameters);

            HttpResponseMessage response = await LocalClient.PostAsync("", content);

            string responseAsString = await response.Content.ReadAsStringAsync();

            if (responseAsString == string.Empty) return new JObject();

            return (JObject) JsonConvert.DeserializeObject(responseAsString);
        }

        public async Task<String> GetAlbumArt(string artist, string album)
        {
            JObject result = await getInfo("album.getinfo",
                new Dictionary<string, string>() { { "artist", artist }, { "album", album } });

            if (result["album"] != null)
            {
                if (result["album"]["image"] != null)
                {
                    return result["album"]["image"].Last["#text"].ToString();
                }
            }

            return string.Empty;
        }

        public async Task<bool> UpdateScrobbleSession()
        {            
            string username = ApplicationSettings.GetSettingsValue<string>(ApplicationSettings.SETTING_LASTFM_USERNAME, string.Empty);
            string password = ApplicationSettings.GetSettingsValue<string>(ApplicationSettings.SETTING_LASTFM_PASSWORD, string.Empty);

            string sessionKey = string.Empty;

            JObject result = await getInfo("auth.getMobileSession",
                             new Dictionary<string, string>() { { "username", username }, { "password", password } },
                             true);

            if (result["session"] != null)
            {
                if (result["session"]["key"] != null)
                {
                    sessionKey = result["session"]["key"].ToString();
                }
            }

            Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Updated LastFM session key {0}", (sessionKey == null ? "NULL" : "NOT NULL"));

            ApplicationSettings.PutSettingsValue(ApplicationSettings.SETTING_LASTFM_SESSION_TOKEN, sessionKey);

            return sessionKey != null;
        }

        private DateTime UnixTimeZero = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        public async Task<LastfmStatusCode> ScrobbleTrack(string trackName, string artistName, DateTime time, bool retry = false)
        {
            bool isScrobblingOn = ApplicationSettings.GetSettingsValue<bool>(ApplicationSettings.SETTING_IS_LASTFM_SCROBBLING_ENABLED, false);

            if (!isScrobblingOn) return LastfmStatusCode.Success;

            string sessionKey = ApplicationSettings.GetSettingsValue<string>(ApplicationSettings.SETTING_LASTFM_SESSION_TOKEN, string.Empty);

            if (sessionKey == string.Empty)
            {
                await UpdateScrobbleSession();

                sessionKey = ApplicationSettings.GetSettingsValue<string>(ApplicationSettings.SETTING_LASTFM_SESSION_TOKEN, string.Empty);

                if (sessionKey == string.Empty) return LastfmStatusCode.Failure;
            }

            int totalSeconds = (int)(time - UnixTimeZero).TotalSeconds;


            JObject result = await getInfo("track.scrobble",
                new Dictionary<string, string>() { { "track", trackName }, { "artist", artistName }, { "timestamp", totalSeconds.ToString() }, { "sk", sessionKey } },
                    true);

            if (result["error"] != null)
            {
                int r = Int32.Parse(result["error"].ToString());

                // bad session key
                if (r == 9 && !retry)
                {
                    Logger.Current.Log(new CallerInfo(), LogLevel.Warning, "Got an invalid session response when scrobbling, retrying");

                    await UpdateScrobbleSession();

                    LastfmStatusCode secondSuccess = await ScrobbleTrack(trackName, artistName, time, true);

                    return secondSuccess;
                }
                else if (r == 8 || r == 11 || r == 16 || r == 29)
                {
                    Logger.Current.Log(new CallerInfo(), LogLevel.Warning, "Got an retryable response {0} when scrobbling, retrying later", r);

                    return LastfmStatusCode.RetryableFailure;
                }

                Logger.Current.Log(new CallerInfo(), LogLevel.Warning, "Got an failure response {0} when scrobbling, giving up", r);

                return LastfmStatusCode.Failure;
            }
            else if (result["scrobbles"] != null)
            {
                return LastfmStatusCode.Success;
            }
            else
            {
                return LastfmStatusCode.Failure;
            }
        }

        private string GenerateSignature(Dictionary<string, string> parameters)
        {
            List<string> signatureParts = new List<string>();

            List<string> keyList = new List<string>(parameters.Keys);

            keyList.Sort();

            StringBuilder b = new StringBuilder();

            foreach (string s in keyList)
            {
                b.Append(s);
                b.Append(parameters[s]);
            }

            b.Append(API_SECRET);


                var Md5 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
                var buffer = CryptographicBuffer.ConvertStringToBinary(b.ToString(), BinaryStringEncoding.Utf8);
                IBuffer buffHash = Md5.HashData(buffer);


                return CryptographicBuffer.EncodeToHexString(buffHash);
        }
    }
}
