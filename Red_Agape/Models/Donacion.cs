using System.ComponentModel.DataAnnotations;

public enum TipoEntrega { RecoleccionDomicilio, PuntoAcopio }

public class ArticuloInput
{
    [Required] public string Descripcion { get; set; } = "";
    [Required] public string Categoria { get; set; } = "";
    [Range(1, 999)] public int Cantidad { get; set; } = 1;
}

public class Donacion
{
    [Required] public string NombreDonante { get; set; } = "";
    [Required, EmailAddress] public string Correo { get; set; } = "";
    [Phone] public string? Telefono { get; set; }

    [Required] public TipoEntrega TipoEntrega { get; set; }
    public string? Direccion { get; set; }   // solo si es recolección

    public List<ArticuloInput> Articulos { get; set; } = new() { new() };
    public string? Notas { get; set; }
}