using Alura.ListaLeitura.Modelos;
using Alura.ListaLeitura.Persistencia;
using Alura.WebAPI.Api.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;

namespace Alura.ListaLeitura.Api.Controllers
{
    [ApiController]
    [Authorize]
    [ApiVersion("2.0")]
    [ApiExplorerSettings(GroupName = "v2")]
    [Route("api/v{version:apiVersion}/livros")]
    public class Livros2Controller : ControllerBase
    {
        private readonly IRepository<Livro> _repo;

        public Livros2Controller(IRepository<Livro> repository)
        {
            _repo = repository;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Recupera uma coleção paginada de livros.",
            Tags = new[] { "Livros" }
        )]
        [HttpGet]
        [ProducesResponseType(statusCode: 200, Type = typeof(LivroPaginado))]
        [ProducesResponseType(statusCode: 500, Type = typeof(ErrorResponse))]
        [ProducesResponseType(statusCode: 404)]
        public IActionResult ListaDeLivros(
            [FromQuery] LivroFiltro filtro,
            [FromQuery] LivroOrdem ordem,
            [FromQuery] LivroPaginacao paginacao)
        {
            var livroPaginado = _repo.All
                .AplicaFiltro(filtro)
                .AplicaOrdem(ordem)
                .Select(l => l.ToApi())                
                .ToLivroPaginado(paginacao);
            return Ok(livroPaginado);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Recupera o livro identificado por seu {id}.",
            Tags = new[] { "Livros" },
            Produces = new[] { "application/json", "application/xml" }
        )]
        [ProducesResponseType(statusCode: 200, Type = typeof(LivroApi))]
        [ProducesResponseType(statusCode: 500, Type = typeof(ErrorResponse))]
        [ProducesResponseType(statusCode: 404)]
        public IActionResult Recuperar(int id)
        {
            var model = _repo.Find(id);
            if (model == null)
                return NotFound();

            return Ok(model);
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Registra novo livro na base.",
            Tags = new[] { "Livros" }
            )]
        [ProducesResponseType(statusCode: 201, Type = typeof(LivroApi))]
        [ProducesResponseType(statusCode: 400, Type = typeof(ErrorResponse))]
        [ProducesResponseType(statusCode: 500, Type = typeof(ErrorResponse))]
        public IActionResult Incluir([FromForm] LivroUpload model)
        {
            if (ModelState.IsValid)
            {
                var livro = model.ToLivro();
                _repo.Incluir(livro);

                var url = Url.Action("Recuperar", new { id = livro.Id });
                return Created(url, livro);
            }
            return BadRequest(ErrorResponse.FromModelState(ModelState));
        }

        [HttpPut]
        [SwaggerOperation(
            Summary = "Modifica o livro na base.",
            Tags = new[] { "Livros" })]
        [ProducesResponseType(statusCode: 200)]
        [ProducesResponseType(400, Type = typeof(ErrorResponse))]
        [ProducesResponseType(500, Type = typeof(ErrorResponse))]

        public IActionResult Alterar([FromForm] LivroUpload model)
        {
            if (ModelState.IsValid)
            {
                var livro = model.ToLivro();
                if (model.Capa == null)
                {
                    livro.ImagemCapa = _repo.All
                        .Where(l => l.Id == livro.Id)
                        .Select(l => l.ImagemCapa)
                        .FirstOrDefault();
                }
                _repo.Alterar(livro);
                return Ok();
            }
            return BadRequest();
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Exclui o livro da base.",
            Tags = new[] { "Livros" }
        )]
        [ProducesResponseType(404)]
        [ProducesResponseType(204)]
        [ProducesResponseType(500, Type = typeof(ErrorResponse))]
        public IActionResult Remover(int id)
        {
            var model = _repo.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            _repo.Excluir(model);
            return NoContent();
        }

        [HttpGet("{id}/capa")]
        [SwaggerOperation(
            Summary = "Recupera a capa do livro identificado por seu {id}.",
            Tags = new[] { "Livros" },
            Produces = new[] { "image/png" }
        )]
        public IActionResult ImagemCapa(int id)
        {
            byte[] img = _repo.All
                           .Where(l => l.Id == id)
                           .Select(l => l.ImagemCapa)
                           .FirstOrDefault();
            if (img != null)
            {
                return File(img, "image/png");
            }
            return File("~/images/capas/capa-vazia.png", "image/png");
        }
    }
}
