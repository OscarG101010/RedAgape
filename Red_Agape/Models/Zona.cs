namespace Red_Agape.Models
{
    public class Zona
    {
        public int ZonaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Municipio { get; set; } = string.Empty;
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
    }
}
