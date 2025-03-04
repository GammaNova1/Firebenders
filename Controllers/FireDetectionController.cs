﻿using Firebenders.Data;
using Firebenders.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;

public class FireDetectionController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public FireDetectionController(IHttpClientFactory httpClientFactory, ApplicationDbContext context, IMemoryCache cache)
    {
        _httpClientFactory = httpClientFactory;
        _context = context;
        _cache = cache;
    }

    [HttpGet]
    public IActionResult Anasayfa()
    {
        return View("Anasayfa");
    }

    [HttpGet]
    public IActionResult Arsiv()
    {
        if (!_cache.TryGetValue("Records", out List<Records> records))
        {
            records = _context.Records.ToList();
            _cache.Set("Records", records, TimeSpan.FromMinutes(5)); // Cache veriyi 5 dakika saklar
        }
        return View(records);
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            string baseUrl = "https://firebenders.s3.eu-central-1.amazonaws.com/firebenders/";
            string[] categories = { "fire", "not_fire" };

            Random rnd = new Random();
            string category = categories[rnd.Next(categories.Length)];

            int number = rnd.Next(0, 50);
            string numberString = number.ToString("D3");
            string imgName = $"{category}{numberString}.png";
            ViewBag.imgName = imgName;
            string imageUrl = baseUrl + imgName;

            var imageResponse = await httpClient.GetAsync(imageUrl);
            imageResponse.EnsureSuccessStatusCode();

            var requestBody = new { url = imageUrl };
            var predictionResponse = await httpClient.PostAsJsonAsync("http://localhost:5000/predicturl", requestBody);
            predictionResponse.EnsureSuccessStatusCode();

            var jsonResponse = await predictionResponse.Content.ReadAsStringAsync();
            var prediction = JObject.Parse(jsonResponse)["predicted_label"].ToString();

            ViewBag.Prediction2 = prediction;
            ViewBag.ImageUrl = imageUrl;

            double latitude = 0, longitude = 0;

            if (prediction.ToLower() == "fire")
            {
                Random rand = new Random();
                double minLatitude = 37.0; // Türkiye'nin güney sınırı
                double maxLatitude = 41.0; // Türkiye'nin kuzey sınırı
                double minLongitude = 28.0; // Türkiye'nin batı sınırı
                double maxLongitude = 43.0; // Türkiye'nin doğu sınırı

                latitude = rand.NextDouble() * (maxLatitude - minLatitude) + minLatitude;
                longitude = rand.NextDouble() * (maxLongitude - minLongitude) + minLongitude;

                ViewBag.Latitude = latitude;
                ViewBag.Longitude = longitude;
                var fireRecord = new Records
                {
                    RecordName = imgName,
                    RecordImage = imageUrl,
                    RecordLatitude = latitude.ToString(),
                    RecordLongtitude = longitude.ToString(),
                    RecordStatus = true,
                    RecordDate = DateTime.Now,
                    RecordStyle = true
                };
                _context.Records.Add(fireRecord);
                await _context.SaveChangesAsync();
            }
            else
            {
                var fireRecord = new Records
                {
                    RecordName = imgName,
                    RecordImage = imageUrl,
                    RecordLatitude = latitude.ToString(),
                    RecordLongtitude = longitude.ToString(),
                    RecordStatus = false,
                    RecordDate = DateTime.Now,
                    RecordStyle = true
                };
                _context.Records.Add(fireRecord);
                await _context.SaveChangesAsync();
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
