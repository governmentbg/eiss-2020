using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace IOWebApplicationApi.Controllers
{
    /// <summary>
    /// Демо API
    /// </summary>
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        /// <summary>
        /// Списък от стойности
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, typeof(string[]), Description = "Списък от стойности")]
        public IActionResult Get()
        {
            return Ok(new string[] { "value1", "value2" });
        }

        /// <summary>
        /// Конкретна стойност
        /// </summary>
        /// <param name="id">Идентификатор на стойност</param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("{id}")]
        [SwaggerResponse(StatusCodes.Status200OK, typeof(string), Description = "Намерената стойност")]
        public IActionResult Get(int id)
        {
            var model = new { id };

            return Ok(model);
        }

        /// <summary>
        /// Празен метод, имитира създаване на стойност
        /// </summary>
        /// <param name="value">Стойност за добавяне</param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponse(StatusCodes.Status201Created, typeof(string), Description = "Добавения елемент")]
        public IActionResult Post([FromBody]string value)
        {
            int newId = 8;

            return Created(Url.Action(nameof(Get), new { id = newId }), value);
        }

        /// <summary>
        /// Този метод е игнориран в документацията
        /// Празен метод имитиращ редакция
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        [HttpPut("{id}")]
        [SwaggerIgnore]
        public IActionResult Put(int id, [FromBody]string value)
        {
            return Ok(value);
        }

        /// <summary>
        /// Изтрива елемент
        /// </summary>
        /// <param name="id">Идентификатор на елемента за изтриване</param>
        [HttpDelete("{id}")]
        [SwaggerResponse(StatusCodes.Status200OK, typeof(void))]
        public IActionResult Delete(int id)
        {
            return Ok();
        }
    }
}
