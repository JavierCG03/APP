

namespace CarslineApp.Models
{
    // ============================================
    // MODELOS DE AUTENTICACIÓN (SIN CAMBIOS)
    // ============================================

    public class LoginRequest
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class CrearUsuarioRequest
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int RolId { get; set; }
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UsuarioDto? Usuario { get; set; }
        public string? Token { get; set; }
    }

    public class CrearUsuarioResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UsuarioDto? Usuario { get; set; }
    }

    public class UsuarioDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public int RolId { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public string? DescripcionRol { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoAcceso { get; set; }
    }

    public class RolDto
    {
        public int Id { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}
