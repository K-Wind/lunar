using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    public class Controller : ControllerBase
    {
        private readonly IMessageHandler _messaging;

        public Controller(IMessageHandler messageHandler)
        {
            _messaging = messageHandler;
        }

        [HttpGet]
        [Route("count")]
        public async Task<string> Get()
        {
            return await _messaging.SendAndReceive("get");
        }

        [HttpPost]
        [Route("count/increment")]
        public void Increment()
        {
            _messaging.Send("increment");
        }

        [HttpPost]
        [Route("count/decrement")]
        public void Decrement()
        {
            _messaging.Send("decrement");
        }
    }
}