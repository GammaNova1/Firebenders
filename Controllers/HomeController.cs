using Firebenders.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Firebenders.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        public HomeController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        //[HttpGet]
        //public IActionResult Index()
        //{
        //    return View();
        //}

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
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

                // Resmi al
                var imageResponse = await _httpClient.GetAsync(imageUrl);
                imageResponse.EnsureSuccessStatusCode();

                // Resmi Flask API'ye gönder ve tahmin al
                var requestBody = new { url = imageUrl };
                var predictionResponse = await _httpClient.PostAsJsonAsync("http://localhost:5000/predicturl", requestBody);
                predictionResponse.EnsureSuccessStatusCode();

                var jsonResponse = await predictionResponse.Content.ReadAsStringAsync();
                var prediction = JObject.Parse(jsonResponse)["predicted_label"].ToString();

                ViewBag.Prediction2 = prediction;
                ViewBag.ImageUrl = imageUrl;
                if (prediction.ToLower() == "fire")
                {
                    Random rand = new Random();
                    double latitude = rand.NextDouble() * (42.1071 - 36.0) + 35.0;
                    double longitude = rand.NextDouble() * (44.7931 - 26.0) + 26.0;

                    ViewBag.Latitude2 = latitude;
                    ViewBag.Longitude2 = longitude;
                }

                return View("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
                return View("Hata");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Predict(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                ViewBag.Message = "Image is not selected";
                return View("Index");
            }

            try
            {
                var filePath = Path.GetTempFileName();
                using (var stream = System.IO.File.Create(filePath))
                {
                    await image.CopyToAsync(stream);
                }

                // Send the image to the Flask API
                var form = new MultipartFormDataContent();
                form.Add(new StreamContent(System.IO.File.OpenRead(filePath)), "img", image.FileName);

                var response = await _httpClient.PostAsync("http://localhost:5000/predictpath", form);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var prediction = JObject.Parse(jsonResponse)["prediction"].ToString();

                ViewBag.Prediction = prediction;

                if (prediction.ToLower() == "fire")
                {
                    Random rand = new Random();
                    double latitude = rand.NextDouble() * (42.1071 - 36.0) + 35.0;
                    double longitude = rand.NextDouble() * (44.7931 - 26.0) + 26.0;

                    ViewBag.Latitude = latitude;
                    ViewBag.Longitude = longitude;
                }

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
