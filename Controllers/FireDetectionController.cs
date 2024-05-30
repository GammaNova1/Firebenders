using Firebenders.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Firebenders.Controllers
{
    public class FireDetectionController : Controller
    {
        private readonly HttpClient _httpClient;

        public FireDetectionController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public IActionResult Anasayfa()
        {
            return View("Anasayfa");
        }

        [HttpGet]
        public IActionResult Arsiv()
        {
            return View("Arsiv");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                string baseUrl = "https://firebenders.s3.eu-central-1.amazonaws.com/firebenders/";
                string[] categories = { "fire", "not_fire" };

                Random rnd = new Random();
                string category = categories[rnd.Next(categories.Length)];

                int number = rnd.Next(0, 50);
                string numberString = number.ToString("D3");

                string imageUrl = baseUrl + $"{category}{numberString}.png";

                var imageResponse = await _httpClient.GetAsync(imageUrl);
                imageResponse.EnsureSuccessStatusCode();

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
                    double minLatitude = 37.0; // Türkiye'nin güney sınırı
                    double maxLatitude = 41.0; // Türkiye'nin kuzey sınırı
                    double minLongitude = 28.0; // Türkiye'nin batı sınırı
                    double maxLongitude = 43.0; // Türkiye'nin doğu sınırı

                    // Türkiye'nin içindeki en geniş dikdörtgenin sınırlarını belirle
                    double latitude, longitude;

                    latitude = rand.NextDouble() * (maxLatitude - minLatitude) + minLatitude;
                    longitude = rand.NextDouble() * (maxLongitude - minLongitude) + minLongitude;


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

        //[HttpPost]
        //public async Task<IActionResult> Predict(IFormFile image)
        //{
        //    if (image == null || image.Length == 0)
        //    {
        //        ViewBag.Message = "Image is not selected";
        //        return View("Index");
        //    }

        //    try
        //    {
        //        // Kullanıcının yüklediği dosya yolunu belirleyin
        //        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "firebenders");
        //        if (!Directory.Exists(uploads))
        //        {
        //            Directory.CreateDirectory(uploads);
        //        }

        //        var filePath = Path.Combine(uploads, image.FileName);
        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await image.CopyToAsync(stream);
        //        }

        //        // Resmi Flask API'ye gönder
        //        var form = new MultipartFormDataContent();
        //        form.Add(new StreamContent(System.IO.File.OpenRead(filePath)), "img", image.FileName);
        //        ViewBag.ImagePath = "/firebenders/" + image.FileName; // Relative path for displaying the image in the view

        //        var response = await _httpClient.PostAsync("http://localhost:5000/predictpath", form);
        //        response.EnsureSuccessStatusCode();

        //        var jsonResponse = await response.Content.ReadAsStringAsync();
        //        var prediction = JObject.Parse(jsonResponse)["prediction"].ToString();

        //        ViewBag.Prediction = prediction;

        //        if (prediction.ToLower() == "fire")
        //        {
        //            Random rand = new Random();
        //            double latitude = rand.NextDouble() * (42.1071 - 36.0) + 35.0;
        //            double longitude = rand.NextDouble() * (44.7931 - 26.0) + 26.0;

        //            ViewBag.Latitude = latitude;
        //            ViewBag.Longitude = longitude;
        //        }

        //        return View("Index");
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Error = $"Error: {ex.Message}";
        //        return View("Hata");
        //    }
        //}
    }
}
