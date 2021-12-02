using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("/[controller]/[action]")]
    public class Controller : ControllerBase
    {
        private readonly MessageHandler _messaging;

        public Controller(ILogger<Controller> logger)
        {
            _messaging = new MessageHandler();
        }

        [HttpGet(Name = "Get")]
        //[Route("api/[controller]/[action]")]
        public string Get()
        {
            return _messaging.SendAndReceive("get").Result;
        }

        [HttpPost(Name = "Increment")]
        //[Route("api/[controller]/[action]")]
        public void Increment()
        {
            _messaging.Send("increment");
            //get success?
        }

        [HttpPost(Name = "Decrement")]
        //[Route("api/[controller]/[action]")]
        public void Decrement()
        {
            _messaging.Send("decrement");
        }
    }
}