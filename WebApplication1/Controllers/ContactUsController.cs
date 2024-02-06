using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ContactUsController : Controller
    {
        public class CaptchaResponseViewModel
        {
            public bool Success { get; set; }

            [JsonProperty(PropertyName = "error-codes")]
            public IEnumerable<string> ErrorCodes { get; set; }

            [JsonProperty(PropertyName = "challenge_ts")]
            public DateTime ChallengeTime { get; set; }

            public string HostName { get; set; }
            public double Score { get; set; }
            public string Action { get; set; }
        }


        // GET: ContactUs
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(ContactUsViewModel model)
        {
            model.Name = Request.Form["FirstName"] + " " + Request.Form["LastName"];
            model.Email = Request.Form["Email"];
            model.Message = Request.Form["Message"];
            model.GoogleCaptchaToken = Request.Form["GoogleCaptchaToken"];

            if (ModelState.IsValid)
            {
                var isCaptchaValid = await IsCaptchaValid(model.GoogleCaptchaToken);
                if (isCaptchaValid)
                {
                    // send email
                    return RedirectToAction("Success");
                }
                else
                {
                    ModelState.AddModelError("GoogleCaptcha", "The captcha is not valid");
                }

            }

            return View(model);
        }

        private async Task<bool> IsCaptchaValid(string response)
        {
            try
            {
                var secret = "Your Secret Key";
                using (var client = new HttpClient())
                {
                    var values = new Dictionary<string, string>
                    {
                        {"secret", secret},
                        {"response", response},
                        {"remoteip", Request.UserHostAddress}
                    };

                    var content = new FormUrlEncodedContent(values);
                    var verify = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                    var captchaResponseJson = await verify.Content.ReadAsStringAsync();
                    var captchaResult = JsonConvert.DeserializeObject<CaptchaResponseViewModel>(captchaResponseJson);
                    return captchaResult.Success
                           && captchaResult.Action == "ContactUs"
                           && captchaResult.Score > 0.5;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public ActionResult Success()
        {
            return View();
        }
    }
}