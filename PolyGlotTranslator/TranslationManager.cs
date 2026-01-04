using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PolyGlotTranslator
{
    public class TranslationManager
    {
        private HttpClient httpClient;
        private string apiKey = ""; // Kendi API key'inizi ekleyebilirsiniz

        public TranslationManager()
        {
            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<string> TranslateTextAsync(string text, string sourceLang, string targetLang)
        {
            try
            {
                // Eğer internet bağlantısı varsa Google Translate API'yi kullan
                if (IsInternetAvailable())
                {
                    return await TranslateWithGoogleTranslate(text, sourceLang, targetLang);
                }
                else
                {
                    // Offline çeviri (basit sözlük)
                    return TranslateOffline(text, sourceLang, targetLang);
                }
            }
            catch (Exception)
            {
                // Her iki yöntem de başarısız olursa offline çeviriye dön
                return TranslateOffline(text, sourceLang, targetLang);
            }
        }

        private async Task<string> TranslateWithGoogleTranslate(string text, string sourceLang, string targetLang)
        {
            try
            {
                // Google Translate API (ücretsiz versiyon)
                string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sourceLang}&tl={targetLang}&dt=t&q={HttpUtility.UrlEncode(text)}";

                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string result = await response.Content.ReadAsStringAsync();

                // JSON'dan çeviriyi parse et
                return ParseGoogleTranslateResponse(result);
            }
            catch
            {
                // API hatası durumunda offline çeviriye dön
                return TranslateOffline(text, sourceLang, targetLang);
            }
        }

        private string ParseGoogleTranslateResponse(string jsonResponse)
        {
            try
            {
                // Basit JSON parsing
                int startIndex = jsonResponse.IndexOf('"') + 1;
                if (startIndex <= 0) return "";

                int endIndex = jsonResponse.IndexOf('"', startIndex);
                if (endIndex <= startIndex) return "";

                return jsonResponse.Substring(startIndex, endIndex - startIndex);
            }
            catch
            {
                return "Çeviri parse edilemedi";
            }
        }

        private string TranslateOffline(string text, string sourceLang, string targetLang)
        {
            // Basit offline çeviri sözlüğü
            Dictionary<string, Dictionary<string, string>> dictionary = GetOfflineDictionary();

            if (dictionary.ContainsKey(sourceLang) && dictionary[sourceLang].ContainsKey(text.ToLower()))
            {
                return dictionary[sourceLang][text.ToLower()];
            }

            // Eşleşme yoksa metni ters çevir veya aynen döndür
            if (sourceLang == "en" && targetLang == "tr")
            {
                return TranslateEnglishToTurkish(text);
            }
            else if (sourceLang == "tr" && targetLang == "en")
            {
                return TranslateTurkishToEnglish(text);
            }

            return $"Çeviri: [{sourceLang} -> {targetLang}]\n{text}\n\n(Çeviri için internet bağlantısı gerekli)";
        }

        private Dictionary<string, Dictionary<string, string>> GetOfflineDictionary()
        {
            var dictionary = new Dictionary<string, Dictionary<string, string>>();

            // İngilizce -> Türkçe
            var enToTr = new Dictionary<string, string>
            {
                {"hello", "merhaba"},
                {"good morning", "günaydın"},
                {"good night", "iyi geceler"},
                {"thank you", "teşekkür ederim"},
                {"please", "lütfen"},
                {"yes", "evet"},
                {"no", "hayır"},
                {"good", "iyi"},
                {"bad", "kötü"},
                {"how are you", "nasılsın"},
                {"what is your name", "adın ne"},
                {"i love you", "seni seviyorum"},
                {"goodbye", "hoşça kal"},
                {"see you", "görüşürüz"},
                {"water", "su"},
                {"food", "yiyecek"},
                {"house", "ev"},
                {"car", "araba"},
                {"computer", "bilgisayar"},
                {"phone", "telefon"}
            };

            // Türkçe -> İngilizce
            var trToEn = new Dictionary<string, string>
            {
                {"merhaba", "hello"},
                {"günaydın", "good morning"},
                {"iyi geceler", "good night"},
                {"teşekkür ederim", "thank you"},
                {"lütfen", "please"},
                {"evet", "yes"},
                {"hayır", "no"},
                {"iyi", "good"},
                {"kötü", "bad"},
                {"nasılsın", "how are you"},
                {"adın ne", "what is your name"},
                {"seni seviyorum", "i love you"},
                {"hoşça kal", "goodbye"},
                {"görüşürüz", "see you"},
                {"su", "water"},
                {"yiyecek", "food"},
                {"ev", "house"},
                {"araba", "car"},
                {"bilgisayar", "computer"},
                {"telefon", "phone"}
            };

            dictionary["en"] = enToTr;
            dictionary["tr"] = trToEn;

            return dictionary;
        }

        private string TranslateEnglishToTurkish(string text)
        {
            var words = text.ToLower().Split(' ');
            var result = new List<string>();
            var dict = GetOfflineDictionary();

            foreach (var word in words)
            {
                if (dict.ContainsKey("en") && dict["en"].ContainsKey(word))
                {
                    result.Add(dict["en"][word]);
                }
                else
                {
                    result.Add($"[{word}]");
                }
            }

            return string.Join(" ", result);
        }

        private string TranslateTurkishToEnglish(string text)
        {
            var words = text.ToLower().Split(' ');
            var result = new List<string>();
            var dict = GetOfflineDictionary();

            foreach (var word in words)
            {
                if (dict.ContainsKey("tr") && dict["tr"].ContainsKey(word))
                {
                    result.Add(dict["tr"][word]);
                }
                else
                {
                    result.Add($"[{word}]");
                }
            }

            return string.Join(" ", result);
        }

        private bool IsInternetAvailable()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(3);
                    var response = client.GetAsync("http://www.google.com").Result;
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        public void SetApiKey(string key)
        {
            apiKey = key;
        }

        public async Task<List<string>> GetSupportedLanguagesAsync()
        {
            try
            {
                // Desteklenen dillerin listesi
                return new List<string>
                {
                    "Türkçe (tr)", "İngilizce (en)", "Almanca (de)", "Fransızca (fr)",
                    "İspanyolca (es)", "İtalyanca (it)", "Rusça (ru)", "Arapça (ar)",
                    "Çince (zh-CN)", "Japonca (ja)", "Korece (ko)", "Portekizce (pt)"
                };
            }
            catch
            {
                return new List<string> { "Türkçe (tr)", "İngilizce (en)" };
            }
        }
    }
}