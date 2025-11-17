using BankingApp.Core.Domain.Common.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace BankingApp.Core.Application.Dtos.Loan
{
    public class LoanDto
    {
        [JsonIgnore]

        public Guid Id { get; set; }

        [JsonIgnore]
        public required string ClientId { get; set; }


        [JsonProperty("cliente")]
        public required string ClientName { get; set; }

        [JsonProperty("cedula")]

        public required string ClientDocumentIdNumber { get; set; }


        [JsonProperty("id")]
        public required string PublicId { get; set; }

        //Total del prestamo
        [JsonProperty("monto")]

        public decimal TotalLoanAmount { get; set; }

        //Total de cuotas del  prestamo
        [JsonProperty("cuotasTotales")]

        public required int TotalInstallmentsCount { get; set; }

        //Total de cuotas  pagadas del  prestamo

        [JsonProperty("cuotasPagadas")]

        public required int PaidInstallmentsCount { get; set; }


        //Monto restante a pagar  prestamo
        [JsonProperty("Pendiente")]

        public required decimal OutstandingBalance { get; set; }

        //Tasa aplicada
        [JsonProperty("Interes")]

        public required decimal InterestRate { get; set; }
        //Plazo del prestamo en meses
        [JsonProperty("Plazo")]

        public required int LoanTermInMonths { get; set; }



        [JsonProperty("estadoPago")]
        public required string LoanStatus { get; set; }
        [JsonIgnore]
        public required LoanStatus Status { get; set; }
    }
}
