namespace BarbeariaAPI.DTOs;

public class PaginatedResponse<T>
{
    public IEnumerable<T> Itens { get; set; } = [];
    public int Pagina { get; set; }
    public int Tamanho { get; set; }
    public int Total { get; set; }
}