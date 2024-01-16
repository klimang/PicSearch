using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace PicSearchAPI.db;

public partial class PicSearchContext : DbContext
{
    public PicSearchContext()
    { }
	public PicSearchContext(DbContextOptions<PicSearchContext> options)
    : base(options)
    {
		this.Database.EnsureCreated();
	}
    public virtual DbSet<Link> Links { get; set; }

    public virtual DbSet<Picture> Pictures { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Link>(entity =>
        {
            entity.HasKey(e => new { e.LinkId, e.PictureId }).HasName("links_pkey");

            entity.ToTable("links");

            entity.Property(e => e.LinkId)
                .ValueGeneratedOnAdd()
                .HasColumnName("link_id");
            entity.Property(e => e.PictureId).HasColumnName("picture_id");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.Domain).HasColumnName("type");
            entity.Property(e => e.Url).HasColumnName("url");

            entity.HasOne(d => d.Picture).WithMany(p => p.Links)
                .HasPrincipalKey(p => p.ImageId)
                .HasForeignKey(d => d.PictureId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("links_picture_id_fkey");
        });

        modelBuilder.Entity<Picture>(entity =>
        {
            entity.HasKey(e => new { e.ImageId }).HasName("picture_pkey");

            entity.ToTable("picture");

            entity.HasIndex(e => e.Hash8, "picture_hash8");

            entity.HasIndex(e => e.ImageId, "picture_image_id_key").IsUnique();

            entity.Property(e => e.ImageId)
                .ValueGeneratedOnAdd()
                .HasColumnName("image_id");
            entity.Property(e => e.Hash64).HasColumnName("hash64");
            entity.Property(e => e.Hash8).HasColumnName("hash8");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
