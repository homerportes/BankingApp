using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BankingApp.Core.Application.ViewModels.User
{
    public class EditUserWithAmountViewModel : IValidatableObject
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres.")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "La cédula es obligatoria.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "La cédula debe tener exactamente 11 dígitos")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "La cédula debe contener solo 11 dígitos numéricos (ej: 00112345678)")]
        public string DocumentIdNumber { get; set; } = null!;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        public string UserName { get; set; } = null!;

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El monto adicional no puede ser negativo.")]
        public decimal? AditionalAmount { get; set; }

        public bool IsClient { get; set; }

        public bool HasError { get; set; }
        public string? Error { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(Password))
            {
                // Verifica longitud mínima
                if (Password.Length < 8)
                {
                    yield return new ValidationResult(
                        "La contraseña debe tener al menos 8 caracteres",
                        new[] { nameof(Password) });
                }

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

                // Verifica confirmación
                if (string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    yield return new ValidationResult(
                        "Debe confirmar la contraseña",
                        new[] { nameof(ConfirmPassword) });
                }
                else if (Password != ConfirmPassword)
                {
                    yield return new ValidationResult(
                        "Las contraseñas no coinciden",
                        new[] { nameof(ConfirmPassword) });
                }
            }
        }
    }
}
