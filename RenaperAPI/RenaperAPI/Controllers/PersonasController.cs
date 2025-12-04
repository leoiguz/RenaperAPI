using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using RenaperAPI;

namespace RenaperAPI.Controllers
{
    public class PersonasController : ApiController
    {
        private RenaperDBEntities db = new RenaperDBEntities();

        // GET: api/Personas
        public IQueryable<Personas> GetPersonas()
        {
            return db.Personas;
        }

        // GET: api/Personas/5
        [ResponseType(typeof(Personas))]
        public async Task<IHttpActionResult> GetPersonas(int id)
        {
            Personas personas = await db.Personas.FindAsync(id);
            if (personas == null)
            {
                return NotFound();
            }

            return Ok(personas);
        }

        // PUT: api/Personas/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutPersonas(int id, Personas personas)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != personas.IdPersona)
            {
                return BadRequest();
            }

            db.Entry(personas).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonasExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Personas
        [ResponseType(typeof(Personas))]
        public async Task<IHttpActionResult> PostPersonas(Personas personas)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Personas.Add(personas);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = personas.IdPersona }, personas);
        }

        // DELETE: api/Personas/5
        [ResponseType(typeof(Personas))]
        public async Task<IHttpActionResult> DeletePersonas(int id)
        {
            Personas personas = await db.Personas.FindAsync(id);
            if (personas == null)
            {
                return NotFound();
            }

            db.Personas.Remove(personas);
            await db.SaveChangesAsync();

            return Ok(personas);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PersonasExists(int id)
        {
            return db.Personas.Count(e => e.IdPersona == id) > 0;
        }

        // GET: api/Personas/Dni/35123456
        [Route("api/Personas/Dni/{dni}")]
        [HttpGet]
        [ResponseType(typeof(Personas))]
        public async Task<IHttpActionResult> GetPersonaPorDni(int dni)
        {
            var persona = await db.Personas.FirstOrDefaultAsync(p => p.DNI == dni);

            if (persona == null)
            {
                return NotFound(); // Devuelve 404 si el DNI no existe
            }

            return Ok(persona); // Devuelve los datos de la persona
        }

        // GET: api/Personas/Cuil/20-40111222-3
        // Valida el CUIT/CUIL (formato y dígito verificador) y devuelve la persona si existe.
        [Route("api/Personas/Cuil/{cuil}")]
        [HttpGet]
        [ResponseType(typeof(Personas))]
        public async Task<IHttpActionResult> GetPersonaPorCuit(string cuil)
        {
            var persona = await db.Personas.FirstOrDefaultAsync(p => p.CUIL == cuil);

            if (persona == null)
            {
                return NotFound(); // Devuelve 404 si el DNI no existe
            }

            return Ok(persona); // Devuelve los datos de la persona
        }
    }
}