using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("/[controller]/[action]")]
    public class Controller : ControllerBase
    {
        private readonly IMessageHandler _messaging;

        public Controller(IMessageHandler messageHandler)
        {
            _messaging = messageHandler;
        }

        [HttpGet(Name = "Get")]

        public async Task<string> Get()
        {
            return await _messaging.SendAndReceive("get");
        }

        [HttpPost(Name = "Increment")]
        public void Increment()
        {
            _messaging.Send("increment");
        }

        [HttpPost(Name = "Decrement")]
        public void Decrement()
        {
            _messaging.Send("decrement");
        }
    }
}