using Newtonsoft.Json;
using RenaperWeb.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

public class RenaperController : Controller
{
    // Asegúrate de que este puerto sea el correcto de tu API
    private readonly string URL_BASE = "https://localhost:44328/api/Personas";
    private readonly string MI_API_KEY = "renaper-12345-seguro";

    // 1. EL MENÚ PRINCIPAL
    public ActionResult Index()
    {
        return View();
    }

    // 2. VER TODOS (LISTADO)
    public async Task<ActionResult> Listado()
    {
        List<PersonaViewModel> personas = new List<PersonaViewModel>();

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-API-KEY", MI_API_KEY);
            // Llamada a la raíz api/Personas para traer todo
            var response = await client.GetAsync(URL_BASE);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                personas = JsonConvert.DeserializeObject<List<PersonaViewModel>>(json);
            }
        }
        return View(personas);
    }

    // 3. BUSCADOR (Muestra el formulario y el resultado)
    public async Task<ActionResult> Buscador(int? dniBuscado)
    {
        // Si no hay DNI (es la primera vez que entran), devolvemos la vista vacía
        if (dniBuscado == null)
        {
            return View(); // Vista sin modelo
        }

        PersonaViewModel personaEncontrada = null;

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-API-KEY", MI_API_KEY);

            // Construimos la URL específica: api/Personas/Dni/123456
            string urlEspecifica = $"{URL_BASE}/Dni/{dniBuscado}";

            var response = await client.GetAsync(urlEspecifica);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                personaEncontrada = JsonConvert.DeserializeObject<PersonaViewModel>(json);
            }
            else
            {
                ViewBag.Error = "No se encontró ninguna persona con ese DNI.";
            }
        }

        return View(personaEncontrada);
    }
}