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


    private static Dictionary<int, string> _pagosRegistrados = new Dictionary<int, string>();

    public ActionResult Index()
    {
        return View();
    }

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

    public async Task<ActionResult> SeleccionPago(int dni)
    {
        var persona = await ObtenerPersonaPorDni(dni);
        if (persona == null) return RedirectToAction("Buscador");
        return View(persona);
    }

    public async Task<ActionResult> PagoMercadoPago(int dni)
    {
        var persona = await ObtenerPersonaPorDni(dni);
        if (persona == null) return RedirectToAction("Buscador");
        return View(persona);
    }

    public async Task<ActionResult> PagoPagoFacil(int dni)
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
                    notificationUrl = "https://tu-sitio.com/webhook",
                    apiKey = "tu-api-key-demo",
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

                    // Extraer y guardar el PaymentId
                    string paymentId = externalUrl.Substring(externalUrl.LastIndexOf('/') + 1);

                    // Guardamos en nuestra "BD" que este DNI inició un pago con este ID
                    if (_pagosRegistrados.ContainsKey(dni))
                    {
                        _pagosRegistrados[dni] = paymentId;
                    }
                    else
                    {
                        _pagosRegistrados.Add(dni, paymentId);
                    }

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

    // FICHA
    public async Task<ActionResult> Ficha(int dni)
    {
        // Verificamos si existe la persona
        var persona = await ObtenerPersonaPorDni(dni);
        if (persona == null) return RedirectToAction("Buscador");

        // Buscar un pago registrado para este DNI
        if (!_pagosRegistrados.ContainsKey(dni))
        {
            // Si intentan entrar directo cambiando el DNI en la URL se redirecciona el usuario a la vista de pago
            return RedirectToAction("SeleccionPago", new { dni = dni });
        }

        string paymentId = _pagosRegistrados[dni];

        // Consultar a la API si el pago está APROBADO
        bool estaPagado = await VerificarEstadoPago(paymentId);

        if (estaPagado)
        {
            return View(persona);
        }
        else
        {
            // Si el pago existe pero está pendiente o fallido
            ViewBag.Error = "El pago aún no ha sido aprobado. Por favor intente nuevamente o espere unos instantes.";
            return RedirectToAction("SeleccionPago", new { dni });
        }
    }

    // Helper para verificar el pago con la API externa
    private async Task<bool> VerificarEstadoPago(string paymentId)
    {
        try
        {
            using (var client = new HttpClient())
            {
                // GET /api/mercadopago/payment/[paymentId]
                var response = await client.GetAsync($"{URL_MP_API}/payment/{paymentId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    dynamic data = JObject.Parse(json);

                    // Verificamos el estado
                    string status = data.payment.paymentStatus;
                    return status == "approved";
                }
            }
        }
        catch
        {
            // Si falla la verificación, asumimos no pagado por seguridad
            return false;
        }
        return false;
    }

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