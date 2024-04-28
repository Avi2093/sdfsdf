using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Twilio_POC.Models;

namespace Twilio_POC.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TwilioController : ControllerBase
    {
     
        private readonly IConfiguration _configuration;
        private const string _fromNumber = "+1251211"; 

        public TwilioController(IConfiguration configuration)
        {
            _configuration = configuration;
            TwilioClient.Init(
                _configuration["TwilioSettings:AccountSid"],
                _configuration["TwilioSettings:AuthToken"]
            );
        }

        public class CallRequest
        {
            public string ToNumber { get; set; }
        }

        [HttpPost("outbound")]
        public IActionResult MakeCall([FromBody] CallRequest request)
        {
            var call = CallResource.Create(
                to: new PhoneNumber(request.ToNumber),
                from: new PhoneNumber(_fromNumber),
                url: new Uri("http://demo.twilio.com/docs/voice.xml")
            );

            var callDetails = new
            {
                Message = "Call initiated successfully",
                CallSid = call.Sid,
                DateCreated = call.DateCreated,
                DateUpdated = call.DateUpdated,
                ParentCallSid = call.ParentCallSid,
                AccountSid = call.AccountSid,
                To = call.To.ToString(),
                From = call.From.ToString(),
                PhoneNumberSid = call.PhoneNumberSid,
                Status = call.Status.ToString(),
                StartTime = call.StartTime,
                EndTime = call.EndTime,
                Duration = call.Duration,
                Price = call.Price,
                PriceUnit = call.PriceUnit,
                Direction = call.Direction.ToString(),
                AnsweredBy = call.AnsweredBy,
                ApiVersion = call.ApiVersion,
                ForwardedFrom = call.ForwardedFrom,
                CallerName = call.CallerName,
                Uri = call.Uri.ToString()
            };

            return Ok(callDetails);
        }



        //[HttpPost("outbound")]
        //public IActionResult MakeCall()
        //{
        //    // Hard-code the numbers from the curl example
        //    string toNumber = "+919408210";
        //    string fromNumber = "+1251220";

        //    // Construct TwiML for manual message
        //    var twiml = new Twilio.Types.Twiml("<Response><Say>Hello! Anil How are you?.</Say></Response>");

        //    var call = CallResource.Create(
        //        to: new PhoneNumber(toNumber),
        //        from: new PhoneNumber(fromNumber),
        //        url: new Uri("http://demo.twilio.com/docs/voice.xml"),
        //        twiml: twiml
        //    );
        //    return Ok(new { Message = "Call initiated successfully", CallSid = call.Sid });
        //}



        //[HttpPost("inbound")]
        //public IActionResult ReceiveCall([FromForm] string from, [FromForm] string to, [FromForm] string CallSid, [FromForm] string AccountSid)
        //{
        //    // Log or use the data as needed
        //    // Console.WriteLine($"Inbound call from {from} to {to}, call SID: {CallSid}");

        //    var response = new Twilio.TwiML.VoiceResponse();
        //    response.Say("Thank you for calling!");
        //    return Content(response.ToString(), "text/xml");
        //}

        [HttpPost("inbound")]
        public IActionResult ReceiveCall([FromForm] InboundCallRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Send bad request response if validation fails
            }

            // Log or use the data as needed
            // Console.WriteLine($"Inbound call from {request.From} to {request.To}, call SID: {request.CallSid}");

            var response = new Twilio.TwiML.VoiceResponse();
            response.Say("Thank you for calling!");
            return Content(response.ToString(), "text/xml");
        }

        [HttpGet]
        public async Task<IActionResult> GetCalls()
        {
            var calls = await CallResource.ReadAsync();

            var callLogs = calls.Select(c => new
            {
                c.Sid,
                c.DateCreated,
                c.DateUpdated,
                c.AccountSid,
                c.To,
                c.From,
                c.PhoneNumberSid,
                c.Status,
                c.StartTime,
                c.EndTime,
                c.Duration,
                c.Price,
                c.PriceUnit,
                c.Direction,
                c.AnsweredBy,
                c.ApiVersion,
                c.ForwardedFrom,
                c.CallerName,
                c.Uri,
                c.SubresourceUris
            }).ToList();

            return Ok(callLogs);
        }

    }
}





















using System.ComponentModel.DataAnnotations;
namespace Twilio_POC.Models
{
   

    public class InboundCallRequest
    {
        [Required]
        [StringLength(34, MinimumLength = 34)]
        [RegularExpression("^AC[0-9a-fA-F]{32}$")]
        public string AccountSid { get; set; }

        [Required]
        [StringLength(34, MinimumLength = 34)]
        [RegularExpression("^CA[0-9a-fA-F]{32}$")]
        public string CallSid { get; set; }

        public string ApiVersion { get; set; }

        [Required]
        public string CallStatus { get; set; } // Consider using Enum if possible for known statuses

        [Required]
        [Phone]
        public string From { get; set; }

        [Required]
        [Phone]
        public string To { get; set; }

        public string Direction { get; set; }

        public string ForwardedFrom { get; set; }

        public string CallerName { get; set; }

        public string? ParentCallSid { get; set; }

        // Make geographic parameters truly optional
        public string? FromCity { get; set; }
        public string? FromState { get; set; }
        public string? FromZip { get; set; }
        public string? FromCountry { get; set; }
        public string? ToCity { get; set; }
        public string? ToState { get; set; }
        public string? ToZip { get; set; }
        public string? ToCountry { get; set; }

    }
}
