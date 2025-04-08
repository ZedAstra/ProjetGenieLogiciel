﻿// <auto-generated />
using System;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Backend.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250228152729_StatutTache")]
    partial class StatutTache
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.1");

            modelBuilder.Entity("Backend.Models.Communication.Annonce", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Contenu")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Publication")
                        .HasColumnType("TEXT");

                    b.Property<string>("Titre")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Annonces");
                });

            modelBuilder.Entity("Backend.Models.Communication.Cannal", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Channels");
                });

            modelBuilder.Entity("Backend.Models.Communication.Message", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("AuteurId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("CannalId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Contenu")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AuteurId");

                    b.HasIndex("CannalId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Backend.Models.Communication.Plainte", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("AuteurId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Contenu")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Titre")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AuteurId");

                    b.ToTable("Plaintes");
                });

            modelBuilder.Entity("Backend.Models.Management.Document", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Contenu")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Documents");
                });

            modelBuilder.Entity("Backend.Models.Management.Ressource", b =>
                {
                    b.Property<string>("Nom")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Quantité")
                        .HasColumnType("TEXT");

                    b.Property<string>("Unité")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Nom");

                    b.ToTable("Ressources");
                });

            modelBuilder.Entity("Backend.Models.Planning.Tache", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Début")
                        .HasColumnType("TEXT");

                    b.Property<string>("Etat")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Fin")
                        .HasColumnType("TEXT");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Taches");
                });

            modelBuilder.Entity("Backend.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("CannalId")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("CannalId1")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("TacheId")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserRole")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CannalId");

                    b.HasIndex("CannalId1");

                    b.HasIndex("TacheId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Backend.Models.Communication.Message", b =>
                {
                    b.HasOne("Backend.Models.User", "Auteur")
                        .WithMany()
                        .HasForeignKey("AuteurId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Models.Communication.Cannal", "Cannal")
                        .WithMany()
                        .HasForeignKey("CannalId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Auteur");

                    b.Navigation("Cannal");
                });

            modelBuilder.Entity("Backend.Models.Communication.Plainte", b =>
                {
                    b.HasOne("Backend.Models.User", "Auteur")
                        .WithMany()
                        .HasForeignKey("AuteurId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Auteur");
                });

            modelBuilder.Entity("Backend.Models.User", b =>
                {
                    b.HasOne("Backend.Models.Communication.Cannal", null)
                        .WithMany("Admins")
                        .HasForeignKey("CannalId");

                    b.HasOne("Backend.Models.Communication.Cannal", null)
                        .WithMany("Participants")
                        .HasForeignKey("CannalId1");

                    b.HasOne("Backend.Models.Planning.Tache", null)
                        .WithMany("Assignés")
                        .HasForeignKey("TacheId");
                });

            modelBuilder.Entity("Backend.Models.Communication.Cannal", b =>
                {
                    b.Navigation("Admins");

                    b.Navigation("Participants");
                });

            modelBuilder.Entity("Backend.Models.Planning.Tache", b =>
                {
                    b.Navigation("Assignés");
                });
#pragma warning restore 612, 618
        }
    }
}
