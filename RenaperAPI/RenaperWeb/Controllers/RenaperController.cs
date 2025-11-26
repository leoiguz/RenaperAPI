using Newtonsoft.Json;
using RenaperWeb.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

public class RenaperController : Controller
{
    private readonly string URL_BASE = "https://localhost:44328/api/Personas";
    private readonly string MI_API_KEY = "renaper-12345-seguro";

    public ActionResult Index()
    {
        return View();
    }

    // LISTADO COMPLETO
    public async Task<ActionResult> Listado()
    {
        List<PersonaViewModel> personas = new List<PersonaViewModel>();
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-API-KEY", MI_API_KEY);
            var response = await client.GetAsync(URL_BASE);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                personas = JsonConvert.DeserializeObject<List<PersonaViewModel>>(json);
            }
        }
        return View(personas);
    }

    // BUSCADOR: Ahora redirige a 'SeleccionPago'
    public async Task<ActionResult> Buscador(int? dniBuscado)
    {
        if (dniBuscado == null) return View();

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-API-KEY", MI_API_KEY);
            var response = await client.GetAsync($"{URL_BASE}/Dni/{dniBuscado}");

            if (response.IsSuccessStatusCode)
            {
                // CAMBIO IMPORTANTE: Redirige a la pantalla de selección
                return RedirectToAction("SeleccionPago", new { dni = dniBuscado });
            }
            else
            {
                ViewBag.Error = "No se encontró ninguna persona con ese DNI.";
                return View();
            }
        }
    }

    // NUEVA ACCIÓN: Pantalla de Selección
    public async Task<ActionResult> SeleccionPago(int dni)
    {
        var persona = await ObtenerPersonaPorDni(dni);
        if (persona == null) return RedirectToAction("Buscador");
        return View(persona);
    }

    // VISTA DE PAGO CON MERCADO PAGO
    public async Task<ActionResult> PagoMercadoPago(int dni)
    {
        var persona = await ObtenerPersonaPorDni(dni);
        if (persona == null) return RedirectToAction("Buscador");
        return View(persona);
    }

    // VISTA DE PAGO (Actualmente es Pago Fácil)
    public async Task<ActionResult> Pago(int dni)
    {
        var persona = await ObtenerPersonaPorDni(dni);
        if (persona == null) return RedirectToAction("Buscador");
        return View(persona);
    }

    // VISTA DE FICHA FINAL
    public async Task<ActionResult> Ficha(int dni)
    {
        var persona = await ObtenerPersonaPorDni(dni);
        if (persona == null) return RedirectToAction("Buscador");
        return View(persona);
    }

    // Helper para reutilizar código de llamada a API
    private async Task<PersonaViewModel> ObtenerPersonaPorDni(int dni)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-API-KEY", MI_API_KEY);
            var response = await client.GetAsync($"{URL_BASE}/Dni/{dni}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<PersonaViewModel>(json);
            }
        }
        return null;
    }
}