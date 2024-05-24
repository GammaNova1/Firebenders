using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Firebenders.Controllers
{
    public class FireDetectionController : Controller
    {
        private readonly HttpClient _httpClient;

        public FireDetectionController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FirePredict()
        {
            string baseUrl = "https://firebenders.s3.eu-central-1.amazonaws.com/firebenders/";
            string[] categories = { "fire", "not_fire" };

            // Rastgele bir kategori (fire veya not_fire) seç
            Random rnd = new Random();
            string category = categories[rnd.Next(categories.Length)];

            // Rastgele bir sayı seç (0 ile 49 arasında)
            int number = rnd.Next(0, 50);
            string numberString = number.ToString("D3");

            // Resim URL'sini oluştur
            string imageUrl = baseUrl + $"{category}{numberString}.png";

            try
            {
                
                // Resmi al
                var imageResponse = await _httpClient.GetAsync(imageUrl);
                imageResponse.EnsureSuccessStatusCode();

                // Resmi Flask API'ye gönder ve tahmin al
                var requestBody = new { url = imageUrl };
                var predictionResponse = await _httpClient.PostAsJsonAsync("http://localhost:5000/predict", requestBody);
                predictionResponse.EnsureSuccessStatusCode();

                var jsonResponse = await predictionResponse.Content.ReadAsStringAsync();
                var prediction = JObject.Parse(jsonResponse)["predicted_label"].ToString();

                ViewBag.Prediction = prediction;
                ViewBag.ImageUrl = imageUrl;

                return View("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
                return View("Hata");
            }
        }
    }
}
