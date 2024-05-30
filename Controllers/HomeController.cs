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

        [HttpGet]
        public IActionResult Index()
        {
            return View();
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
        public IActionResult Makale()
        {
            return View("Makale");
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
                // Kullanıcının yüklediği dosya yolunu belirleyin
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "firebenders");
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                var filePath = Path.Combine(uploads, image.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Resmi Flask API'ye gönder
                var form = new MultipartFormDataContent();
                form.Add(new StreamContent(System.IO.File.OpenRead(filePath)), "img", image.FileName);
                ViewBag.ImagePath = "/firebenders/" + image.FileName; // Relative path for displaying the image in the view

                var response = await _httpClient.PostAsync("http://localhost:5000/predictpath", form);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var prediction = JObject.Parse(jsonResponse)["prediction"].ToString();

                ViewBag.Prediction = prediction;

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
    }
}