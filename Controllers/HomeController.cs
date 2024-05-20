﻿using Firebenders.Models;
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

        [HttpPost]
        public async Task<IActionResult> Predict(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                ViewBag.Message = "Image is not selected";
                return View("Index");
            }

            // Save the image to a temporary location
            var filePath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(filePath))
            {
                await image.CopyToAsync(stream);
            }

            // Send the image to the Flask API
            var form = new MultipartFormDataContent();
            form.Add(new StreamContent(System.IO.File.OpenRead(filePath)), "img", image.FileName);

            var response = await _httpClient.PostAsync("http://localhost:5000/prediction", form);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var prediction = JObject.Parse(jsonResponse)["prediction"].ToString();

            ViewBag.Prediction = prediction;
            return View("Index");
        }
    }
}