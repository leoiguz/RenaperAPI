using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RenaperWeb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

public class RenaperController : Controller
{
    private readonly string URL_RENAPER_BASE = ConfigurationManager.AppSettings["RenaperApiUrl"];
    private readonly string MI_API_KEY = ConfigurationManager.AppSettings["RenaperApiKey"];
    private readonly string URL_MP_API = ConfigurationManager.AppSettings["MercadoPagoApiUrl"];

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
            var response = await client.GetAsync(URL_RENAPER_BASE);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                personas = JsonConvert.DeserializeObject<List<PersonaViewModel>>(json);
            }
        }
        return View(personas);
    }

    // BUSCADOR: redirige a 'SeleccionPago'
    public async Task<ActionResult> Buscador(int? dniBuscado)
    {
        if (dniBuscado == null) return View();

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-API-KEY", MI_API_KEY);
            var response = await client.GetAsync($"{URL_RENAPER_BASE}/Dni/{dniBuscado}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("SeleccionPago", new { dni = dniBuscado });
            }
            else
            {
                ViewBag.Error = "No se encontró ninguna persona con ese DNI.";
                return View();
            }
        }
    }

    //Pantalla de Selección
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

    [HttpPost]
    public async Task<ActionResult> IniciarPagoExterno(int dni, string clientEmail)
    {
        try
        {
            var persona = await ObtenerPersonaPorDni(dni);

            using (var client = new HttpClient())
            {
                var paymentData = new
                {
                    description = $"Consulta Renaper - DNI {dni}",
                    paymentAmount = 60.00,
                    notificationUrl = "https://tu-sitio-real.com/webhook", // URL ficticia por ahora
                    apiKey = "tu-api-key-demo", // Clave demo
                    clientEmail,
                    clientName = $"{persona.Nombres} {persona.Apellido}",
                    backUrl = Url.Action("Ficha", "Renaper", new { dni }, Request.Url.Scheme)
                };

                var content = new StringContent(JsonConvert.SerializeObject(paymentData), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(URL_MP_API, content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    dynamic data = JObject.Parse(jsonResponse);

                    string externalUrl = data.paymentRoute;

                    return Redirect(externalUrl);
                }
                else
                {
                    ViewBag.Error = "La pasarela de pago rechazó la solicitud.";
                    return View("PagoMercadoPago", persona);
                }
            }
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error de conexión: " + ex.Message;
            return RedirectToAction("Buscador");
        }
    }

    // VISTA DE PAGO (Actualmente es Pago Fácil)
    public async Task<ActionResult> PagoPagoFacil(int dni)
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
            var response = await client.GetAsync($"{URL_RENAPER_BASE}/Dni/{dni}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<PersonaViewModel>(json);
            }
        }
        return null;
    }
}