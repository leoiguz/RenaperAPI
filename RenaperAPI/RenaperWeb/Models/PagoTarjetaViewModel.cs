using System.ComponentModel.DataAnnotations;

namespace RenaperWeb.Models
{
    public class PagoTarjetaViewModel
    {
        public int DNI { get; set; }

        [Required]
        public string CardNumber { get; set; }

        [Required]
        public string CardHolderName { get; set; }

        [Required]
        public string CardExpirationDate { get; set; }

        [Required]
        public string CardCvv { get; set; }

        [Required]
        [EmailAddress]
        public string ClientEmail { get; set; }
    }
}