namespace TechEval.Domain.Common.Abstractions;

public abstract class AgregadoRaiz
{
    private readonly List<IDomainEvent> _dominioEventos = [];

    public IReadOnlyCollection<IDomainEvent> DominioEventos => _dominioEventos.AsReadOnly();

    protected void RegistrarEvento(IDomainEvent evento) => _dominioEventos.Add(evento);

    public void LimpiarEventos() => _dominioEventos.Clear();
}
