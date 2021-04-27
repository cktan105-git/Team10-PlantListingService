﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PlantListing.Infrastructure;

namespace PlantListing.Migrations
{
    [DbContext(typeof(PlantListingContext))]
    [Migration("20210427025302_UpdatedToUserId")]
    partial class UpdatedToUserId
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.5")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.HasSequence("plant_category_hilo")
                .IncrementsBy(10);

            modelBuilder.HasSequence("plant_details_hilo")
                .IncrementsBy(10);

            modelBuilder.HasSequence("weight_unit_hilo")
                .IncrementsBy(10);

            modelBuilder.Entity("PlantListing.Models.PlantCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:HiLoSequenceName", "plant_category_hilo")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.SequenceHiLo);

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.ToTable("PlantCategory");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Category = "Vegetable"
                        },
                        new
                        {
                            Id = 2,
                            Category = "Fruit"
                        },
                        new
                        {
                            Id = 3,
                            Category = "Flower"
                        },
                        new
                        {
                            Id = 4,
                            Category = "Herb"
                        },
                        new
                        {
                            Id = 5,
                            Category = "Spice"
                        });
                });

            modelBuilder.Entity("PlantListing.Models.PlantDetails", b =>
                {
                    b.Property<long>("PlantDetailsId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:HiLoSequenceName", "plant_details_hilo")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.SequenceHiLo);

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("ImageName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<decimal>("Price")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("Stock")
                        .HasColumnType("int");

                    b.Property<int>("UnitId")
                        .HasColumnType("int");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<decimal>("Weight")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("PlantDetailsId");

                    b.ToTable("PlantDetails");

                    b.HasData(
                        new
                        {
                            PlantDetailsId = 1L,
                            CategoryId = 1,
                            Description = "Green vegetable",
                            ImageName = "898ed47a-c779-4d07-bcf5-d241adc55f89_Broccoli.jpg",
                            Name = "Broccoli",
                            Price = 2.00m,
                            Stock = 50,
                            UnitId = 1,
                            UserId = "cktan",
                            Weight = 1.0m
                        },
                        new
                        {
                            PlantDetailsId = 2L,
                            CategoryId = 2,
                            Description = "Red color sour fruit",
                            ImageName = "ddbd68bd-da6d-4ba2-a70f-5a32813ec261_Tomato.jpg",
                            Name = "Tomato",
                            Price = 1.00m,
                            Stock = 50,
                            UnitId = 1,
                            UserId = "cktan",
                            Weight = 0.5m
                        },
                        new
                        {
                            PlantDetailsId = 3L,
                            CategoryId = 2,
                            Description = "Green color fruit",
                            ImageName = "af25243b-9f08-4c1f-ad8f-d031695154d5_Japanese Cucumber.jpg",
                            Name = "Japanese Cucumber",
                            Price = 1.00m,
                            Stock = 50,
                            UnitId = 2,
                            UserId = "mgkoh",
                            Weight = 500.0m
                        },
                        new
                        {
                            PlantDetailsId = 4L,
                            CategoryId = 3,
                            Description = "Flower chasing the sun",
                            ImageName = "df9b2a47-3975-474d-82b6-5f6bcd88f743_Sunflower.jpg",
                            Name = "Sunflower",
                            Price = 50.00m,
                            Stock = 10,
                            UnitId = 3,
                            UserId = "user0001",
                            Weight = 1m
                        },
                        new
                        {
                            PlantDetailsId = 5L,
                            CategoryId = 5,
                            Description = "Home grown fresh garlic",
                            ImageName = "a624fc48-0349-4e28-985e-d5aa4c859c0c_Garlic.jpg",
                            Name = "Garlic",
                            Price = 0.50m,
                            Stock = 50,
                            UnitId = 2,
                            UserId = "wpkeoh",
                            Weight = 100.0m
                        },
                        new
                        {
                            PlantDetailsId = 6L,
                            CategoryId = 5,
                            Description = "Add flavor to your dish",
                            ImageName = "1c1b6571-cbbe-4f45-a5ed-01704a8885b8_Spring Onion.jpg",
                            Name = "Spring Onion",
                            Price = 0.50m,
                            Stock = 50,
                            UnitId = 2,
                            UserId = "wpkeoh",
                            Weight = 100.0m
                        },
                        new
                        {
                            PlantDetailsId = 7L,
                            CategoryId = 5,
                            Description = "Red Chilli",
                            ImageName = "1a048a12-4815-4ce1-9df3-02088c3f82c9_Red Chilli.jpg",
                            Name = "Red Chilli",
                            Price = 1.00m,
                            Stock = 100,
                            UnitId = 2,
                            UserId = "wpkeoh",
                            Weight = 100.0m
                        },
                        new
                        {
                            PlantDetailsId = 8L,
                            CategoryId = 5,
                            Description = "Green Chilli",
                            ImageName = "e095626b-d72f-4fc7-ac7b-0fcc6393ae00_Green Chilli.jpg",
                            Name = "Green Chilli",
                            Price = 1.00m,
                            Stock = 100,
                            UnitId = 2,
                            UserId = "wpkeoh",
                            Weight = 100.0m
                        });
                });

            modelBuilder.Entity("PlantListing.Models.WeightUnit", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:HiLoSequenceName", "weight_unit_hilo")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.SequenceHiLo);

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("WeightUnit");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Unit = "kg"
                        },
                        new
                        {
                            Id = 2,
                            Unit = "g"
                        },
                        new
                        {
                            Id = 3,
                            Unit = "bundle"
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
