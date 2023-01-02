﻿using Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Mime;
using System.Text;

namespace WebFrontEnd.Pages.ShortUrl
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        public CreateModel(IHttpClientFactory httpClientFactory) => this.httpClientFactory = httpClientFactory;        

        [BindProperty]
        public Models.ShortUrl ShortUrl { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (ShortUrl?.Path?.Length > 10)
            {
                TempData["ErrorMessage"] = "Path cannot be longer than 10 characters.";

                return Redirect("~/");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid data.";

                return Redirect("~/");
            }
            
            var httpClient = httpClientFactory.CreateClient();            
            var response = await httpClient.PostAsync($"http://webapi/ShortUrl", new StringContent(JsonConvert.SerializeObject(ShortUrl), Encoding.UTF8, MediaTypeNames.Application.Json));

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = (await response.Content.ReadFromJsonAsync<ErrorDetails>())?.Message ?? "An unexpected error has occurred.";

                return Redirect("~/");
            }

            TempData["InfoMessage"] = $"URL created successfully: {await response.Content.ReadAsStringAsync()}.";

            return Redirect("~/");
        }
    }
}
