using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adapters.Persistence.Persistence
{
    public class ProdutoConfiguration: IEntityTypeConfiguration<Produto>
    {
        public void Configure(EntityTypeBuilder<Produto> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Nome)
                   .IsRequired()
                   .HasMaxLength(64);

            builder.Property(p => p.EnderecoProduto)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(p => p.PrecoAtual)
                   .IsRequired();

            builder.Property(p => p.PrecoAlvo)
                   .IsRequired();
        }
    }
}
