using BankingApp.Core.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;


namespace BankingApp.Core.Application.ViewModels.User
{
    public class CreateUserViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cédula es requerida")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "La cédula debe tener exactamente 11 dígitos")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "La cédula debe contener solo 11 dígitos numéricos (ej: 00112345678)")]
        public string DocumentIdNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmar contraseña es requerido")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es requerido")]
        public required AppRoles Role { get; set; }

        public decimal? InitialAmount { get; set; } // Monto inicial para clientes

        public bool HasError { get; set; }
        public string? Error { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validaciones adicionales de contraseña
            if (!string.IsNullOrWhiteSpace(Password))
            {
                // Verifica al menos una mayúscula
                if (!Regex.IsMatch(Password, @"[A-Z]"))
                {
                    yield return new ValidationResult(
                        "La contraseña debe contener al menos una letra mayúscula",
                        new[] { nameof(Password) });
                }

                // Verifica al menos una minúscula
                if (!Regex.IsMatch(Password, @"[a-z]"))
                {
                    yield return new ValidationResult(
                        "La contraseña debe contener al menos una letra minúscula",
                        new[] { nameof(Password) });
                }

                // Verifica al menos un número
                if (!Regex.IsMatch(Password, @"[0-9]"))
                {
                    yield return new ValidationResult(
                        "La contraseña debe contener al menos un número",
                        new[] { nameof(Password) });
                }

                // Verifica al menos un carácter especial
                if (!Regex.IsMatch(Password, @"[\W_]"))
                {
                    yield return new ValidationResult(
                        "La contraseña debe contener al menos un carácter especial (ej: @, #, $, %, &)",
                        new[] { nameof(Password) });
                }
            }
        }
    }
}



