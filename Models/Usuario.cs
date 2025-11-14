namespace Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string User { get; set; }
    public string Pass { get; set; }
    public Roles Rol { get; set; }
}