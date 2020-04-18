using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using myChatRoomZ_WebAPI.Data;
using myChatRoomZ_WebAPI.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myChatRoomZ_WebAPI.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class ChannelController : Controller
    {

        private readonly IChatRoomZRepository _repository;
        private readonly ILogger<ChannelController> _logger;
        public ChannelController(IChatRoomZRepository repository, ILogger<ChannelController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        [Route("api/channels")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public ActionResult<IEnumerable<Channel>> Get()
        {
            try
            {
                return Ok(_repository.GetAllChannels());
            }
            catch (Exception ex)
            {

                _logger.LogError($"Failed to get channels:{ex}");
                return BadRequest("Bad request");
            }
        }


        //ADDING MESSAGES TO REPOSITORY
        [HttpPost]
        [Route("api/PostMessage")]
        public async Task<IActionResult> PostMessage([FromBody]ChatMessage model)
        {

            try
            {
                if (ModelState.IsValid)
                {

                    model.SentAt = DateTime.Now;
                    _repository.AddMessage(model);

                    if (_repository.SaveAll())
                    {
                        return Created($"/api/channels/{model.ChannelId}", model); //"Created" matching 201 code
                    }
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to post New Message:{ex}");
            }

            return BadRequest("Failed to post New Message");


        }

        //REMOVING MESSAGES FROM REPOSITORY
        [HttpPost]
        [Route("api/DeleteMessage")]
        public async Task<IActionResult> DeleteMessage([FromBody]ChatMessage model)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    _repository.DeleteMessage(model);

                    if (_repository.SaveAll())
                    {
                        return Ok(model); //"OK" matching 200 code
                    }
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to Delete Message:{ex}");
            }

            return BadRequest("Failed to Delete Message");
        }

    }
}
